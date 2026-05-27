using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PebbleGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellWidth = 0.62f;
    [SerializeField] private float cellHeight = 0.54f;
    [SerializeField] private int columns = 10;
    [SerializeField] private int maxRows = 14;
    [SerializeField] private Vector2 origin = new(-2.8f, 5.0f);

    private readonly Dictionary<Vector2Int, Pebble> _cells = new();

    public Vector3 CellToWorld(Vector2Int cell)
    {
        var rowOffset = IsOddRow(cell.y) ? cellWidth * 0.5f : 0f;

        var x = origin.x + rowOffset + cell.x * cellWidth;
        var y = origin.y - cell.y * cellHeight;

        return new Vector3(x, y, 0f);
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        return _cells.ContainsKey(cell);
    }

    public void RegisterPebble(Pebble pebble, Vector2Int cell)
    {
        if (!_cells.TryAdd(cell, pebble))
        {
            Debug.LogWarning($"Cell {cell} is already occupied.");
            return;
        }

        pebble.SetGridCell(cell);
    }
    
    public static List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        var x = cell.x;
        var y = cell.y;

        if (IsOddRow(y))
        {
            return new List<Vector2Int>
            {
                new(x - 1, y),
                new(x + 1, y),
                new(x, y - 1),
                new(x + 1, y - 1),
                new(x, y + 1),
                new(x + 1, y + 1)
            };
        }

        return new List<Vector2Int>
        {
            new(x - 1, y),
            new(x + 1, y),
            new(x - 1, y - 1),
            new(x, y - 1),
            new(x - 1, y + 1),
            new(x, y + 1)
        };
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        if (cell.y < 0 || cell.y >= maxRows)
            return false;

        var columnCount = GetColumnCountForRow(cell.y);

        return cell.x >= 0 && cell.x < columnCount;
    }

    private static bool IsOddRow(int row)
    {
        return row % 2 != 0;
    }
    
    public int GetColumnCountForRow(int row)
    {
        return IsOddRow(row) ? columns - 1 : columns;
    }
    
    public bool TryFindTopRowLandingCell(Vector3 worldPosition, out Vector2Int resultCell)
    {
        const int row = 0;
        var columnCount = GetColumnCountForRow(row);

        var bestColumn = 0;
        var bestDistance = float.MaxValue;
        var found = false;

        for (var column = 0; column < columnCount; column++)
        {
            Vector2Int cell = new(column, row);
 
            if (IsCellOccupied(cell))
                continue;

            var cellWorldPosition = CellToWorld(cell);
            var distance = Mathf.Abs(worldPosition.x - cellWorldPosition.x);

            if (!(distance < bestDistance)) continue;
            bestDistance = distance;
            bestColumn = column;
            found = true;
        }

        resultCell = new Vector2Int(bestColumn, row);
        return found;
    }
    
    public bool TryFindLandingCellFromCollision(Pebble hitPebble, Vector3 collisionPoint, out Vector2Int resultCell)
    {
        resultCell = default;
        if (hitPebble == null) return false;
    
        var hitCell = hitPebble.GridCell;
        var directionFromHitCenter = ((Vector2)collisionPoint - (Vector2)hitPebble.transform.position).normalized;
        var candidateCells = GetLandingCandidatesByDirection(
            hitCell,
            directionFromHitCenter
        );
    
        foreach (var candidate in candidateCells.Where(IsInsideGrid)
                     .Where(candidate => !IsCellOccupied(candidate)))
        {
            resultCell = candidate;
            return true;
        }
    
        return false;
    }
    
    private List<Vector2Int> GetLandingCandidatesByDirection(Vector2Int hitCell, Vector2 directionFromHitCenter)
    {
        var neighbors = GetNeighbors(hitCell);
        
        var validCandidates = neighbors.Where(neighbor => neighbor.y >= hitCell.y).ToList();

        validCandidates.Sort((a, b) =>
        {
            var dirA = ((Vector2)(CellToWorld(a) - CellToWorld(hitCell))).normalized;
            var dirB = ((Vector2)(CellToWorld(b) - CellToWorld(hitCell))).normalized;

            var scoreA = Vector2.Dot(directionFromHitCenter, dirA);
            var scoreB = Vector2.Dot(directionFromHitCenter, dirB);
            
            return scoreB.CompareTo(scoreA);
        });
 
        return validCandidates;
    }
    
    public bool TryGetPebble(Vector2Int cell, out Pebble pebble)
    {
        return _cells.TryGetValue(cell, out pebble);
    }
    
    public void RemovePebble(Vector2Int cell)
    {
        _cells.Remove(cell);
    }
    
    public List<Pebble> GetAllPebbles()
    {
        return _cells.Values.ToList();
    }
    
    public List<Pebble> GetFloatingPebbles()
    {
        var connectedToTop = FindPebblesConnectedToTop();

        return _cells.Values.Where(pebble => !connectedToTop.Contains(pebble)).ToList();
    }
    
    private HashSet<Pebble> FindPebblesConnectedToTop()
    {
        HashSet<Pebble> connected = new();
        Queue<Pebble> open = new();

        const int topRow = 0;
        var columnCount = GetColumnCountForRow(topRow);

        for (var column = 0; column < columnCount; column++)
        {
            Vector2Int cell = new(column, topRow);

            if (!TryGetPebble(cell, out var pebble))
                continue;

            connected.Add(pebble);
            open.Enqueue(pebble);
        }

        while (open.Count > 0)
        {
            var current = open.Dequeue();

            foreach (var neighborCell in GetNeighbors(current.GridCell).Where(IsInsideGrid))
            {
                if (!TryGetPebble(neighborCell, out var neighbor))
                    continue;

                if (!connected.Add(neighbor))
                    continue;

                open.Enqueue(neighbor);
            }
        }

        return connected;
    }
    
    public bool CanMoveRowsDown(int rowCount)
    {
        return _cells.Keys.Select(cell => new Vector2Int(cell.x, cell.y + rowCount)).All(IsInsideGrid);
    }

    public void MoveRowsDown(int rowCount)
    {
        List<KeyValuePair<Vector2Int, Pebble>> existingCells = new(_cells);

        _cells.Clear();

        existingCells.Sort((a, b) => b.Key.y.CompareTo(a.Key.y));

        foreach (var (oldCell, pebble) in existingCells)
        {
            Vector2Int newCell = new(oldCell.x, oldCell.y + rowCount);

            if (!IsInsideGrid(newCell))
            {
                Debug.LogWarning($"Cannot move pebble from {oldCell} to {newCell}.");
                continue;
            }

            pebble.SetGridCell(newCell);
            pebble.transform.position = CellToWorld(newCell);

            _cells[newCell] = pebble;
        }
    }
    
    public void SpawnTopRows(Pebble pebblePrefab, Transform boardParent, int availableColorCount, int rowCount)
    {
        for (var row = 0; row < rowCount; row++)
        {
            var columnCount = GetColumnCountForRow(row);

            for (var column = 0; column < columnCount; column++)
            {
                Vector2Int cell = new(column, row);

                if (IsCellOccupied(cell))
                    continue;

                var pebble = Instantiate(pebblePrefab, CellToWorld(cell), Quaternion.identity, boardParent);

                var randomColor = GetRandomPebbleColor(availableColorCount);
                pebble.SetColor(randomColor);
                pebble.StopMovement();

                RegisterPebble(pebble, cell);
            }
        }
    }

    private static PebbleColor GetRandomPebbleColor(int availableColorCount)
    {
        var colorIndex = Random.Range(0, availableColorCount);
        return (PebbleColor)colorIndex;
    }
    
    public bool HasPebbleAtOrBelowRow(int dangerRow)
    {
        return _cells.Keys.Any(cell => cell.y >= dangerRow);
    }
}