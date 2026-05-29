using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimTrajectory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform trajectoryStartPoint;

    [Header("Trajectory Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private int maxBounces = 2;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float projectileRadius = 0.24f;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
    }

    private void Update()
    {
        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        if (trajectoryStartPoint == null)
            return;

        List<Vector3> points = new();

        Vector2 currentPosition = trajectoryStartPoint.position;
        Vector2 currentDirection = transform.up;

        points.Add(currentPosition);

        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(
                currentPosition,
                projectileRadius,
                currentDirection,
                maxDistance,
                collisionMask
            );

            if (hit.collider == null)
            {
                points.Add(currentPosition + currentDirection * maxDistance);
                break;
            }

            points.Add(hit.centroid);

            bool hitWall = hit.collider.CompareTag("SideWall");

            if (!hitWall)
                break;

            currentPosition = hit.centroid + hit.normal * 0.02f;
            currentDirection = Vector2.Reflect(currentDirection, hit.normal);
        }

        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }

    private void ConfigureLineRenderer()
    {
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = 0;

        _lineRenderer.startWidth = 0.035f;
        _lineRenderer.endWidth = 0.035f;

        _lineRenderer.numCapVertices = 8;
        _lineRenderer.numCornerVertices = 8;

        _lineRenderer.sortingOrder = 40;

        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = new Color(1f, 1f, 1f, 0.45f);

        _lineRenderer.material = material;
    }
}