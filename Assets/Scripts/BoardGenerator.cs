using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PebbleGrid pebbleGrid;
    [SerializeField] private Pebble pebblePrefab;
    [SerializeField] private Transform boardParent;

    [Header("Board Settings")]
    [SerializeField] private int startingRows = 5;
    [SerializeField] private int startingColorCount = 4;

    private void Start()
    {
        GenerateStartingBoard();
    }

    private void GenerateStartingBoard()
    {
        for (var row = 0; row < startingRows; row++)
        {
            var columnCount = pebbleGrid.GetColumnCountForRow(row);

            for (var column = 0; column < columnCount; column++)
            {
                Vector2Int cell = new(column, row);

                if (pebbleGrid.IsCellOccupied(cell)) continue;

                SpawnPebbleAtCell(cell);
            }
        }
    }

    private void SpawnPebbleAtCell(Vector2Int cell)
    {
        var position = pebbleGrid.CellToWorld(cell);

        var pebble = Instantiate(pebblePrefab, position, Quaternion.identity, boardParent);

        pebble.SetColor(GetRandomStartingColor());
        pebble.StopMovement();

        pebbleGrid.RegisterPebble(pebble, cell);
    }

    private PebbleColor GetRandomStartingColor()
    {
        var colorIndex = Random.Range(0, startingColorCount);
        return (PebbleColor)colorIndex;
    }
}