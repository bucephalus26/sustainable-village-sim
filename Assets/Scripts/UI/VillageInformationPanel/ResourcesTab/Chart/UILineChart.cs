using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UILineChart : Graphic
{
    [System.Serializable]
    public struct LineData
    {
        public List<float> points;
        public Color color;
        public string name;
    }

    public List<LineData> dataSets = new();

    [Header("Appearance")]
    public float lineThickness = 2f;
    public float pointRadius = 3f;
    [Range(3, 32)]
    public int pointSegments = 8;
    public float pointBorderThickness = 1f;
    public bool drawPoints = true;

    [Header("Grid & Axes")]
    public bool drawGrid = true;
    public bool drawAxes = true;
    public Color gridColor = new(1f, 1f, 1f, 0.1f);
    public Color axisColor = new(1f, 1f, 1f, 0.2f);
    public float gridLineThickness = 1f;
    public float axisLineThickness = 1f;
    public int horizontalGridLines = 4;

    public float yMin = 0f;
    public float yMax = 100f;
    public int xMaxPoints = 5;

    private Vector2 chartSize;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetData(List<LineData> newData, float minY, float maxY, int pointsCount)
    {
        dataSets = newData ?? new List<LineData>();
        yMin = minY;
        yMax = maxY;
        // Ensure yMax is slightly larger than yMin to avoid division by zero
        if (yMax <= yMin) yMax = yMin + 1f;

        xMaxPoints = Mathf.Max(1, pointsCount); // Need at least one point interval
        SetVerticesDirty(); // Trigger redraw
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (rectTransform == null || dataSets == null) return;

        chartSize = rectTransform.rect.size;

        if (chartSize.x <= 0 || chartSize.y <= 0) return;

        // Draw Grid and Axes
        if (drawGrid) DrawGridLines(vh);
        if (drawAxes) DrawAxisLines(vh);

        // Draw Data Lines and Points
        foreach (var dataSet in dataSets)
        {
            if (dataSet.points == null || dataSet.points.Count == 0) continue;

            if (dataSet.points.Count >= 2)
            {
                DrawDataLine(vh, dataSet);
            }

            if (drawPoints)
            {
                DrawDataPoints(vh, dataSet);
            }
        }
    }

    private void DrawAxisLines(VertexHelper vh)
    {
        // Y-Axis
        DrawLine(vh, Vector2.zero, new Vector2(0, chartSize.y), axisColor, axisLineThickness);
        // X-Axis
        DrawLine(vh, Vector2.zero, new Vector2(chartSize.x, 0), axisColor, axisLineThickness);
    }

    private void DrawGridLines(VertexHelper vh)
    {
        // Horizontal Grid Lines
        if (horizontalGridLines > 0)
        {
            for (int i = 0; i <= horizontalGridLines; i++)
            {
                float normalizedY = (float)i / horizontalGridLines;
                float yPos = normalizedY * chartSize.y;
                DrawLine(vh, new Vector2(0, yPos), new Vector2(chartSize.x, yPos), gridColor, gridLineThickness);
            }
        }

        // Vertical Grid Lines (based on number of data points/days)
        if (xMaxPoints > 1)
        {
            // Draw lines between points, not necessarily at each point index if points < xMaxPoints
            int numSegments = Mathf.Max(1, GetMaxPointsInDatasets() - 1); // Use actual data points for spacing
            if (numSegments <= 0) numSegments = xMaxPoints - 1; // Fallback if no data

            for (int i = 1; i < GetMaxPointsInDatasets(); i++) // Start from second point
            {
                float normalizedX = (float)i / numSegments;
                float xPos = normalizedX * chartSize.x;
                DrawLine(vh, new Vector2(xPos, 0), new Vector2(xPos, chartSize.y), gridColor, gridLineThickness);
            }
        }
    }

    private int GetMaxPointsInDatasets()
    {
        int max = 0;
        foreach (var ds in dataSets)
        {
            if (ds.points != null) max = Mathf.Max(max, ds.points.Count);
        }
        return max;
    }


    private void DrawDataLine(VertexHelper vh, LineData dataSet)
    {
        int pointCount = dataSet.points.Count;
        int numSegments = Mathf.Max(1, pointCount - 1);
        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector2 start = GetLocalPosition(i, dataSet.points[i], numSegments);
            Vector2 end = GetLocalPosition(i + 1, dataSet.points[i + 1], numSegments);
            DrawLine(vh, start, end, dataSet.color, lineThickness);
        }
    }

    private void DrawDataPoints(VertexHelper vh, LineData dataSet)
    {
        int pointCount = dataSet.points.Count;
        if (pointCount == 0) return;

        // Handle single point case or calculate segments for lines
        int numSegments = Mathf.Max(1, pointCount - 1);

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 center = GetLocalPosition(i, dataSet.points[i], numSegments);

            float outerRadius = pointRadius;
            DrawCircle(vh, center, outerRadius, dataSet.color, pointSegments);
            float innerRadius = Mathf.Max(0f, pointRadius - pointBorderThickness);

            if (innerRadius > 0f)
            {
                DrawCircle(vh, center, innerRadius, Color.white, pointSegments);
            }
        }
    }

    // convert data point (index, value) to local chart coordinates
    private Vector2 GetLocalPosition(int index, float value, int numSegments)
    {
        float x = 0;
        // Calculate X position based on segments
        if (numSegments > 0)
        {
            x = ((float)index / numSegments) * chartSize.x;
        }
        else if (xMaxPoints > 0) // Handle single point case
        {
            x = 0.5f * chartSize.x; // Center it if only one point
        }


        float yRange = yMax - yMin;
        float normalizedY = (yRange > 0) ? Mathf.Clamp01((value - yMin) / yRange) : 0.5f; // Clamp & handle flat data
        float y = normalizedY * chartSize.y;

        return new Vector2(x, y);
    }

    // draw a line segment with thickness
    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Color lineColor, float thickness)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        Vector3 p1 = start - perpendicular;
        Vector3 p2 = start + perpendicular;
        Vector3 p3 = end + perpendicular;
        Vector3 p4 = end - perpendicular;

        AddQuad(vh, p1, p2, p3, p4, lineColor);
    }

    // draw a filled circle using triangles fan
    private void DrawCircle(VertexHelper vh, Vector2 center, float radius, Color circleColor, int segments)
    {
        if (segments < 3) segments = 3; // Minimum segments for a triangle

        int centerVertexIndex = vh.currentVertCount;

        // Pivot Offset
        Vector2 pivotOffset = new(this.rectTransform.pivot.x * chartSize.x,
                                          this.rectTransform.pivot.y * chartSize.y);

        // Add center vertex
        UIVertex centerVert = UIVertex.simpleVert;
        centerVert.color = circleColor;
        centerVert.position = (Vector3)(center - pivotOffset); // Apply pivot offset
        vh.AddVert(centerVert);

        // Add edge vertices
        float angleIncrement = 360f / segments;
        for (int i = 0; i <= segments; i++) // <= to close the circle
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            Vector2 edgePos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            UIVertex edgeVert = UIVertex.simpleVert;
            edgeVert.color = circleColor;
            edgeVert.position = (Vector3)(edgePos - pivotOffset); // Apply pivot offset
            vh.AddVert(edgeVert);
        }

        // Create triangles (Triangle Fan around the center vertex)
        for (int i = 0; i < segments; i++)
        {
            int edgeIndex1 = centerVertexIndex + 1 + i;
            int edgeIndex2 = centerVertexIndex + 1 + (i + 1);
            vh.AddTriangle(centerVertexIndex, edgeIndex1, edgeIndex2);
        }
    }

    // add a quad (two triangles) to VertexHelper
    private void AddQuad(VertexHelper vh, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color quadColor)
    {
        int startIndex = vh.currentVertCount;

        Vector2 pivotOffset = new(this.rectTransform.pivot.x * chartSize.x,
                                      this.rectTransform.pivot.y * chartSize.y);

        p1 -= (Vector3)pivotOffset;
        p2 -= (Vector3)pivotOffset;
        p3 -= (Vector3)pivotOffset;
        p4 -= (Vector3)pivotOffset;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = quadColor;

        vert.position = p1; vh.AddVert(vert);
        vert.position = p2; vh.AddVert(vert);
        vert.position = p3; vh.AddVert(vert);
        vert.position = p4; vh.AddVert(vert);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    // Ensure the chart redraws when RectTransform dimensions change
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
    }
}