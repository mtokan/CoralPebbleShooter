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

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Pebble : MonoBehaviour
{
    [SerializeField] private PebbleColor pebbleColor;

    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private PebbleShooter _shooter;
    private bool _isShot;
    private bool _hasLanded;
    private Vector2Int _gridCell;

    public PebbleColor PebbleColor => pebbleColor;
    public Vector2Int GridCell => _gridCell;
    public Rigidbody2D Rigidbody => _rb;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

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
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isShot || _hasLanded)
            return;

        bool hitTopWall = collision.gameObject.CompareTag("TopWall");
        bool hitPebble = collision.gameObject.GetComponent<Pebble>() != null;

        if (!hitTopWall && !hitPebble)
            return;

        _hasLanded = true;
        _isShot = false;

        _shooter.HandlePebbleLanded(this);
    }

    private void ApplyColor()
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.color = pebbleColor switch
        {
            PebbleColor.CoralPink => HexToColor("#FF9AA2"),
            PebbleColor.SoftPeach => HexToColor("#FFB7A1"),
            PebbleColor.SeafoamGreen => HexToColor("#B5EAD7"),
            PebbleColor.PearlYellow => HexToColor("#FFF1A8"),
            PebbleColor.Lavender => HexToColor("#C7B9FF"),
            PebbleColor.SoftAqua => HexToColor("#A8E6E0"),
            _ => Color.white
        };
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.white;
    }
    
    public void SetGridCell(Vector2Int cell)
    {
        _gridCell = cell;
    }
}