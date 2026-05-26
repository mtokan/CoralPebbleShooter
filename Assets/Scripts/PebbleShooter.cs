using UnityEngine;
using UnityEngine.InputSystem;

public class PebbleShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Pebble pebblePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform boardParent;
    [SerializeField] private PebbleGrid pebbleGrid;

    [Header("Shooting")]
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private float minAimAngle = 15f;
    [SerializeField] private float maxAimAngle = 165f;

    private Camera _mainCamera;
    private Pebble _currentPebble;
    private bool _canShoot = true;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        LoadPebble();
    }

    private void Update()
    {
        AimAtMouse();

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            _currentPebble is not null &&
            _canShoot)
        {
            Shoot();
        }
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null || _mainCamera == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f)
        );

        mouseWorldPosition.z = 0f;

        Vector2 direction = mouseWorldPosition - transform.position;

        if (direction.y <= 0f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, minAimAngle, maxAimAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void LoadPebble()
    {
        _currentPebble = Instantiate(pebblePrefab, firePoint.position, Quaternion.identity);
        _currentPebble.transform.SetParent(firePoint);
        _currentPebble.transform.localPosition = Vector3.zero;

        Rigidbody2D rb = _currentPebble.Rigidbody;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;

        _currentPebble.InitializeAsLoaded(this);

        _canShoot = true;
    }

    private void Shoot()
    {
        _canShoot = false;

        _currentPebble.transform.SetParent(null);

        Rigidbody2D rb = _currentPebble.Rigidbody;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = transform.up * shootSpeed;

        _currentPebble.InitializeAsShot(this);

        _currentPebble = null;
    }

    public void HandlePebbleLanded(Pebble landedPebble)
    {
        landedPebble.StopMovement();
        landedPebble.transform.SetParent(boardParent);

        Vector2Int cell = pebbleGrid.FindNearestAvailableCell(landedPebble.transform.position);
        Vector3 snappedPosition = pebbleGrid.CellToWorld(cell);

        landedPebble.transform.position = snappedPosition;
        pebbleGrid.RegisterPebble(landedPebble, cell);

        LoadPebble();
    }
}