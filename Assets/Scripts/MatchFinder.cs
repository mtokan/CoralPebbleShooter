using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchFinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PebbleGrid pebbleGrid;

    [Header("Match Settings")]
    [SerializeField] private int minimumMatchCount = 3;

    public bool TryPopMatches(Pebble startingPebble)
    {
        var matchingGroup = FindMatchingGroup(startingPebble);

        if (matchingGroup.Count < minimumMatchCount)
            return false;

        RemovePebbles(matchingGroup);

        var floatingPebbles = pebbleGrid.GetFloatingPebbles();
        RemovePebbles(floatingPebbles);

        return true;
    }
    
    private void RemovePebbles(List<Pebble> pebbles)
    {
        foreach (var pebble in pebbles.Where(pebble => pebble != null))
        {
            pebbleGrid.RemovePebble(pebble.GridCell);
            Destroy(pebble.gameObject);
        }
    }

    private List<Pebble> FindMatchingGroup(Pebble startingPebble)
    {
        List<Pebble> result = new();

        if (startingPebble == null) return result;

        var targetColor = startingPebble.PebbleColor;

        Queue<Pebble> open = new();
        HashSet<Pebble> visited = new();

        open.Enqueue(startingPebble);
        visited.Add(startingPebble);

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            result.Add(current);

            var neighbors = PebbleGrid.GetNeighbors(current.GridCell);

            foreach (var neighborCell in neighbors.Where(neighborCell => pebbleGrid.IsInsideGrid(neighborCell)))
            {
                if (!pebbleGrid.TryGetPebble(neighborCell, out var neighbor)) continue;

                if (visited.Contains(neighbor)) continue;

                if (neighbor.PebbleColor != targetColor) continue;

                visited.Add(neighbor);
                open.Enqueue(neighbor);
            }
        }

        return result;
    }
}