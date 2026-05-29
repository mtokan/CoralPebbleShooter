using System.Collections.Generic;
using UnityEngine;

public class PebbleQueue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Pebble pebblePrefab;
    [SerializeField] private Transform[] previewSlots;

    [Header("Queue Settings")]
    [SerializeField] private int queueSize = 5;
    [SerializeField] private int availableColorCount = 4;
    
    [Header("Preview Scale")]
    [SerializeField] private float largestPreviewScale = 0.9f;
    [SerializeField] private float smallestPreviewScale = 0.55f;

    private readonly List<PebbleColor> _colors = new();
    private readonly List<Pebble> _previewPebbles = new();

    private void Awake()
    {
        InitializeQueue();
        CreatePreviewPebbles();
        UpdatePreviewPebbles();
    }

    public PebbleColor TakeNextColor()
    {
        if (_colors.Count == 0)
            InitializeQueue();

        PebbleColor nextColor = _colors[0];

        _colors.RemoveAt(0);
        _colors.Add(GetRandomPebbleColor());

        UpdatePreviewPebbles();

        return nextColor;
    }

    private void InitializeQueue()
    {
        _colors.Clear();

        for (int i = 0; i < queueSize; i++)
        {
            _colors.Add(GetRandomPebbleColor());
        }
    }

    private void CreatePreviewPebbles()
    {
        foreach (Pebble previewPebble in _previewPebbles)
        {
            if (previewPebble != null)
                Destroy(previewPebble.gameObject);
        }

        _previewPebbles.Clear();

        int count = Mathf.Min(queueSize, previewSlots.Length);

        for (int i = 0; i < count; i++)
        {
            Pebble previewPebble = Instantiate(
                pebblePrefab,
                previewSlots[i].position,
                Quaternion.identity,
                previewSlots[i]
            );

            previewPebble.transform.localPosition = Vector3.zero;
            previewPebble.transform.localRotation = Quaternion.identity;

            DisablePreviewPhysics(previewPebble);

            _previewPebbles.Add(previewPebble);
        }
    }

    private void UpdatePreviewPebbles()
    {
        int index = 0;

        foreach (PebbleColor color in _colors)
        {
            if (index >= _previewPebbles.Count)
                break;

            Pebble previewPebble = _previewPebbles[index];

            previewPebble.SetColor(color);
            previewPebble.transform.localScale = Vector3.one * GetScaleForSlot(index);

            index++;
        }
    }
    
    private float GetScaleForSlot(int slotIndex)
    {
        if (queueSize <= 1)
            return largestPreviewScale;

        float t = slotIndex / (float)(queueSize - 1);

        return Mathf.Lerp(largestPreviewScale, smallestPreviewScale, t);
    }

    private void DisablePreviewPhysics(Pebble previewPebble)
    {
        Rigidbody2D rb = previewPebble.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        Collider2D col = previewPebble.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    private PebbleColor GetRandomPebbleColor()
    {
        int colorIndex = Random.Range(0, availableColorCount);
        return (PebbleColor)colorIndex;
    }
}