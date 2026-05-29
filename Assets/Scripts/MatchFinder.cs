using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchFinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PebbleGrid pebbleGrid;
    [SerializeField] private PopEffectPlayer popEffectPlayer;

    [Header("Match Settings")]
    [SerializeField] private int minimumMatchCount = 3;
    
    [Header("Float Away Settings")]
    [SerializeField] private float surfacePopY = 5.1f;

    public bool TryPopMatches(Pebble startingPebble)
    {
        List<Pebble> matchingGroup = FindMatchingGroup(startingPebble);

        if (matchingGroup.Count < minimumMatchCount)
            return false;

        PopPebbles(matchingGroup);

        List<Pebble> floatingPebbles = pebbleGrid.GetFloatingPebbles();
        FloatAwayPebbles(floatingPebbles);

        return true;
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
    
    private void PopPebbles(List<Pebble> pebbles)
    {
        foreach (Pebble pebble in pebbles)
        {
            if (pebble == null)
                continue;

            if (popEffectPlayer != null)
                popEffectPlayer.Play(pebble.transform.position, pebble.GetVisualColor());

            pebbleGrid.RemovePebble(pebble.GridCell);
            Destroy(pebble.gameObject);
        }
    }

    private void FloatAwayPebbles(List<Pebble> pebbles)
    {
        foreach (Pebble pebble in pebbles)
        {
            if (pebble == null)
                continue;

            pebbleGrid.RemovePebble(pebble.GridCell);
            pebble.FloatAway(surfacePopY, popEffectPlayer);
        }
    }
}