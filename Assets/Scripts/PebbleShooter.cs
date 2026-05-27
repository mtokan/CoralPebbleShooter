using UnityEngine;
using UnityEngine.InputSystem;

public class PebbleShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Pebble pebblePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform boardParent;
    [SerializeField] private PebbleGrid pebbleGrid;
    [SerializeField] private MatchFinder matchFinder;

    [Header("Shooting")]
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private float minAimAngle = 15f;
    [SerializeField] private float maxAimAngle = 165f;
    
    [Header("Endless Mode")]
    [SerializeField] private int missesBeforeNewRows = 6;
    [SerializeField] private int rowsAddedPerPenalty = 2;
    [SerializeField] private int availableColorCount = 4;
    [SerializeField] private int dangerRow = 13;
    [SerializeField] private GameObject gameOverPanel;

    private int _missCount;
    private bool _gameOver;
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

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && _currentPebble is not null &&
            _canShoot && !_gameOver)
        {
            Shoot();
        }
    }
    
    private void CheckLoseCondition()
    {
        if (pebbleGrid.HasPebbleAtOrBelowRow(dangerRow)) TriggerGameOver();
    }
    
    private void TriggerGameOver()
    {
        if (_gameOver) return;

        _gameOver = true;
        _canShoot = false;

        if (_currentPebble is not null)
        {
            Destroy(_currentPebble.gameObject);
            _currentPebble = null;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null || _mainCamera == null)
            return;

        var mouseScreenPosition = Mouse.current.position.ReadValue();

        var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, 
            mouseScreenPosition.y, 0f));

        mouseWorldPosition.z = 0f;

        Vector2 direction = mouseWorldPosition - transform.position;

        if (direction.y <= 0f) return;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, minAimAngle, maxAimAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void LoadPebble()
    {
        _currentPebble = Instantiate(pebblePrefab, firePoint.position, Quaternion.identity);
        _currentPebble.transform.SetParent(firePoint);
        _currentPebble.transform.localPosition = Vector3.zero;
        
        _currentPebble.SetColor(GetRandomPebbleColor());

        var rb = _currentPebble.Rigidbody;
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

        var rb = _currentPebble.Rigidbody;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = transform.up * shootSpeed;

        _currentPebble.InitializeAsShot(this);

        _currentPebble = null;
    }

    public void HandlePebbleLanded(Pebble landedPebble, GameObject hitObject, Vector3 collisionPoint)
    {
        landedPebble.StopMovement();
        landedPebble.transform.SetParent(boardParent);

        if (hitObject.CompareTag("TopWall"))
        {
            var foundTopCell = pebbleGrid.TryFindTopRowLandingCell(landedPebble.transform.position, out var topCell);

            if (!foundTopCell)
            {
                Debug.LogWarning("No available top row cell found.");
                Destroy(landedPebble.gameObject);
                if (!_gameOver) LoadPebble();
                return;
            }

            SnapAndRegisterPebble(landedPebble, topCell);
            if (!_gameOver) LoadPebble();
            return;
        }

        var hitPebble = hitObject.GetComponent<Pebble>();

        var foundLandingCell = pebbleGrid.TryFindLandingCellFromCollision(hitPebble, collisionPoint, out var landingCell);

        if (!foundLandingCell)
        {
            Debug.LogWarning("No valid landing cell found from collision.");
            Destroy(landedPebble.gameObject);
            if (!_gameOver) LoadPebble();
            return;
        }

        SnapAndRegisterPebble(landedPebble, landingCell);
        if (!_gameOver) LoadPebble();
    }

    private void SnapAndRegisterPebble(Pebble pebble, Vector2Int cell)
    {
        var snappedPosition = pebbleGrid.CellToWorld(cell);

        pebble.transform.position = snappedPosition;
        pebbleGrid.RegisterPebble(pebble, cell);

        var matched = matchFinder.TryPopMatches(pebble);

        if (matched)
        {
            _missCount = 0;
            CheckLoseCondition();
            return;
        }

        _missCount++;

        if (_missCount >= missesBeforeNewRows)
        {
            _missCount = 0;
            AddPenaltyRows();
        }

        CheckLoseCondition();
    }
    
    private void AddPenaltyRows()
    {
        if (!pebbleGrid.CanMoveRowsDown(rowsAddedPerPenalty))
        {
            TriggerGameOver();
            return;
        }

        pebbleGrid.MoveRowsDown(rowsAddedPerPenalty);

        pebbleGrid.SpawnTopRows(pebblePrefab, boardParent, availableColorCount, rowsAddedPerPenalty);

        CheckLoseCondition();
    }
    
    private PebbleColor GetRandomPebbleColor()
    {
        var colorIndex = Random.Range(0, availableColorCount);
        return (PebbleColor)colorIndex;
    }
}