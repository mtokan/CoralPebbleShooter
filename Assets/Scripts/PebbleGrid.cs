using System.Collections.Generic;
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

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int row = Mathf.RoundToInt((origin.y - worldPosition.y) / cellHeight);

        row = Mathf.Clamp(row, 0, maxRows - 1);

        float rowOffset = IsOddRow(row) ? cellWidth * 0.5f : 0f;
        int column = Mathf.RoundToInt((worldPosition.x - origin.x - rowOffset) / cellWidth);

        column = Mathf.Clamp(column, 0, columns - 1);

        return new Vector2Int(column, row);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        float rowOffset = IsOddRow(cell.y) ? cellWidth * 0.5f : 0f;

        float x = origin.x + rowOffset + cell.x * cellWidth;
        float y = origin.y - cell.y * cellHeight;

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

    public Vector2Int FindNearestAvailableCell(Vector3 worldPosition)
    {
        Vector2Int startCell = WorldToCell(worldPosition);

        if (!IsCellOccupied(startCell))
            return startCell;

        Queue<Vector2Int> open = new();
        HashSet<Vector2Int> visited = new();

        open.Enqueue(startCell);
        visited.Add(startCell);

        while (open.Count > 0)
        {
            Vector2Int current = open.Dequeue();

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!IsInsideGrid(neighbor) || visited.Contains(neighbor))
                    continue;

                if (!IsCellOccupied(neighbor))
                    return neighbor;

                visited.Add(neighbor);
                open.Enqueue(neighbor);
            }
        }

        return startCell;
    }

    public List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        int x = cell.x;
        int y = cell.y;

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
        return cell.x >= 0 &&
               cell.x < columns &&
               cell.y >= 0 &&
               cell.y < maxRows;
    }

    private static bool IsOddRow(int row)
    {
        return row % 2 != 0;
    }
}