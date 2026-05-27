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
    [SerializeField] private SpriteRenderer bodyRenderer;

    private PebbleShooter _shooter;
    private bool _isShot;
    private bool _hasLanded;

    public PebbleColor PebbleColor => pebbleColor;
    public Vector2Int GridCell { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

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
        if (bodyRenderer == null) return;

        bodyRenderer.color = pebbleColor switch
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
        return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
    }
    
    public void SetGridCell(Vector2Int cell)
    {
        GridCell = cell;
    }
}