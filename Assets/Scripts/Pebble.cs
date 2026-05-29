using UnityEngine;

public enum PebbleColor
{
    CoralPink,
    SoftPeach,
    SeafoamGreen,
    PearlYellow,
    Lavender,
    SoftAqua
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Pebble : MonoBehaviour
{
    [SerializeField] private PebbleColor pebbleColor;
    [SerializeField] private SpriteRenderer glassBodyRenderer;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer shellRenderer;
    
    [Header("Shell Sprites")]
    [SerializeField] private Sprite fanShell;
    [SerializeField] private Sprite spiralShell;
    [SerializeField] private Sprite sandDollarShell;
    [SerializeField] private Sprite leafShell;
    [SerializeField] private Sprite clamShell;
    [SerializeField] private Sprite conchShell;

    private PebbleShooter _shooter;
    private bool _isShot;
    private bool _hasLanded;
    private bool _isFloatingAway;
    private float _surfacePopY;
    private PopEffectPlayer _popEffectPlayer;
    private Collider2D _collider;

    public PebbleColor PebbleColor => pebbleColor;
    public Vector2Int GridCell { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        ApplyColor();
    }

    public void InitializeAsLoaded(PebbleShooter shooter)
    {
        _shooter = shooter;
        _isShot = false;
        _hasLanded = false;
    }

    public void InitializeAsShot(PebbleShooter shooter)
    {
        _shooter = shooter;
        _isShot = true;
        _hasLanded = false;
    }

    public void SetColor(PebbleColor newColor)
    {
        pebbleColor = newColor;
        ApplyColor();
    }

    public void StopMovement()
    {
        Rigidbody.linearVelocity = Vector2.zero;
        Rigidbody.angularVelocity = 0f;
        Rigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isShot || _hasLanded) return;

        var hitTopWall = collision.gameObject.CompareTag("TopWall");
        var hitPebble = collision.gameObject.GetComponent<Pebble>();

        if (!hitTopWall && hitPebble == null) return;

        var contact = collision.GetContact(0);

        _hasLanded = true;
        _isShot = false;

        _shooter.HandlePebbleLanded(this, collision.gameObject, contact.point);
    }

    private void ApplyColor()
    {
        if (glassBodyRenderer != null) glassBodyRenderer.color = GetBodyColor(pebbleColor);
        if (backgroundRenderer != null) backgroundRenderer.color = GetBackgroundColor(pebbleColor);
        if (shellRenderer != null) shellRenderer.sprite = GetShellSprite(pebbleColor);
    }

    private static Color GetBodyColor(PebbleColor color)
    {
        return color switch
        {
            PebbleColor.CoralPink => HexToColor("#F27B9B6E"),
            PebbleColor.SoftPeach => HexToColor("#F29A4B6E"),
            PebbleColor.SeafoamGreen => HexToColor("#63CFA76E"),
            PebbleColor.PearlYellow => HexToColor("#E8D45A6E"),
            PebbleColor.Lavender => HexToColor("#9B7BE86E"),
            PebbleColor.SoftAqua => HexToColor("#45BFD66E"),
            _ => Color.white
        };
    }

    private static Color GetBackgroundColor(PebbleColor color)
    {
        return color switch
        {
            PebbleColor.CoralPink => HexToColor("#F27B9BB2"),
            PebbleColor.SoftPeach => HexToColor("#F29A4BB2"),
            PebbleColor.SeafoamGreen => HexToColor("#63CFA7B2"),
            PebbleColor.PearlYellow => HexToColor("#E8D45AB2"),
            PebbleColor.Lavender => HexToColor("#9B7BE8B2"),
            PebbleColor.SoftAqua => HexToColor("#45BFD6B2"),
            _ => Color.white
        };
    }

    private Sprite GetShellSprite(PebbleColor color)
    {
        return color switch
        {
            PebbleColor.CoralPink => fanShell,
            PebbleColor.SoftPeach => spiralShell,
            PebbleColor.SeafoamGreen => leafShell,
            PebbleColor.PearlYellow => sandDollarShell,
            PebbleColor.Lavender => conchShell,
            PebbleColor.SoftAqua => clamShell,
            _ => fanShell
        };
    }

    private static Color HexToColor(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
    }
    
    public void SetGridCell(Vector2Int cell)
    {
        GridCell = cell;
    }
    
    public Color GetVisualColor()
    {
        return glassBodyRenderer != null ? glassBodyRenderer.color : Color.white;
    }
    
    private void Update()
    {
        if (!_isFloatingAway)
            return;

        if (transform.position.y < _surfacePopY)
            return;

        if (_popEffectPlayer != null)
            _popEffectPlayer.Play(transform.position, GetVisualColor());

        Destroy(gameObject);
    }

    public void FloatAway(float surfacePopY, PopEffectPlayer popEffectPlayer)
    {
        transform.SetParent(null);

        SetColliderEnabled(false);

        _isFloatingAway = true;
        _surfacePopY = surfacePopY;
        _popEffectPlayer = popEffectPlayer;

        Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody.simulated = true;
        Rigidbody.gravityScale = -0.25f;

        Rigidbody.linearVelocity = new Vector2(
            Random.Range(-0.25f, 0.25f),
            Random.Range(0.6f, 1.0f)
        );

        Rigidbody.angularVelocity = Random.Range(-90f, 90f);
    }
    
    public void SetColliderEnabled(bool isEnabled)
    {
        if (_collider != null)
            _collider.enabled = isEnabled;
    }
}