using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceLineChart : MonoBehaviour
{
    [Header("Chart Settings")]
    [SerializeField] private int dataPoints = 10;
    [SerializeField] private float maxValue = 200f;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private bool showAllResources = true;

    [Header("Resources to Display")]
    [SerializeField] private bool showFood = true;
    [SerializeField] private bool showWealth = true;
    [SerializeField] private bool showGoods = true;
    [SerializeField] private bool showStone = true;

    [Header("Resource Colors")]
    [SerializeField] private Color foodColor = new Color(0.18f, 0.8f, 0.44f, 1f); // Green
    [SerializeField] private Color wealthColor = new Color(0.95f, 0.77f, 0.06f, 1f); // Gold
    [SerializeField] private Color goodsColor = new Color(0.9f, 0.49f, 0.13f, 1f); // Orange
    [SerializeField] private Color stoneColor = new Color(0.58f, 0.65f, 0.65f, 1f); // Gray

    [Header("UI References")]
    [SerializeField] private RectTransform chartContainer;
    [SerializeField] private Image linePrefab;
    [SerializeField] private Image pointPrefab;
    [SerializeField] private Image gridLinePrefab;
    [SerializeField] private GameObject legendPrefab;
    [SerializeField] private RectTransform legendContainer;

    [Header("Grid Settings")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private int horizontalGridLines = 4; // Number of horizontal grid lines
    [SerializeField] private int verticalGridLines = 5; // Number of vertical grid lines
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f); // Light gray, translucent
    [SerializeField] private float gridLineThickness = 1f;
    [SerializeField] private bool showAxisLabels = true;
    [SerializeField] private GameObject axisLabelPrefab; // Text prefab for axis labels

    // Line containers
    private GameObject foodLineContainer;
    private GameObject wealthLineContainer;
    private GameObject goodsLineContainer;
    private GameObject stoneLineContainer;
    private GameObject gridContainer;

    // Update timer
    private float updateTimer = 0f;
    [SerializeField] private float updateInterval = 2f;

    private void Start()
    {
        InitializeChart();
        CreateLegend();
        UpdateChart();
    }

    private void Update()
    {
        // Update periodically
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateChart();
        }
    }

    private void InitializeChart()
    {
        // Create containers if they don't exist
        if (foodLineContainer == null)
        {
            foodLineContainer = new GameObject("Food Lines");
            foodLineContainer.transform.SetParent(chartContainer, false);
        }

        if (wealthLineContainer == null)
        {
            wealthLineContainer = new GameObject("Wealth Lines");
            wealthLineContainer.transform.SetParent(chartContainer, false);
        }

        if (goodsLineContainer == null)
        {
            goodsLineContainer = new GameObject("Goods Lines");
            goodsLineContainer.transform.SetParent(chartContainer, false);
        }

        if (stoneLineContainer == null)
        {
            stoneLineContainer = new GameObject("Stone Lines");
            stoneLineContainer.transform.SetParent(chartContainer, false);
        }

        if (gridContainer == null)
        {
            gridContainer = new GameObject("Grid Lines");
            gridContainer.transform.SetParent(chartContainer, false);
            // Grid should be behind other elements
            gridContainer.transform.SetAsFirstSibling();
        }

        // Clear existing lines
        ClearLines(foodLineContainer);
        ClearLines(wealthLineContainer);
        ClearLines(goodsLineContainer);
        ClearLines(stoneLineContainer);
        ClearLines(gridContainer);

        // Draw grid (if enabled)
        if (showGrid)
        {
            DrawGrid();
        }
    }

    private void DrawGrid()
    {
        float chartWidth = chartContainer.rect.width;
        float chartHeight = chartContainer.rect.height;

        // Draw horizontal grid lines
        for (int i = 0; i <= horizontalGridLines; i++)
        {
            float y = (i * chartHeight) / horizontalGridLines;
            DrawGridLine(0, y, chartWidth, y, true);

            // Add Y-axis label if enabled
            if (showAxisLabels && axisLabelPrefab != null)
            {
                float value = minValue + ((maxValue - minValue) * i / horizontalGridLines);
                CreateAxisLabel($"{value:F0}", -5, y, TextAlignmentOptions.Right, true);
            }
        }

        // Draw vertical grid lines
        for (int i = 0; i <= verticalGridLines; i++)
        {
            float x = (i * chartWidth) / verticalGridLines;
            DrawGridLine(x, 0, x, chartHeight, false);

            // Add X-axis label if enabled
            if (showAxisLabels && axisLabelPrefab != null && dataPoints > 0)
            {
                // For X-axis, show day numbers counting backward
                int day = dataPoints - 1 - (int)((float)i / verticalGridLines * (dataPoints - 1));
                CreateAxisLabel($"Day {day}", x, -5, TextAlignmentOptions.Center, false);
            }
        }
    }

    private void DrawGridLine(float x1, float y1, float x2, float y2, bool isHorizontal)
    {
        if (gridLinePrefab == null) return;

        // Instantiate grid line
        Image line = Instantiate(gridLinePrefab, gridContainer.transform);
        line.color = gridColor;

        // Calculate line position
        RectTransform rectTransform = line.GetComponent<RectTransform>();

        // Calculate center and length
        float centerX = (x1 + x2) / 2;
        float centerY = (y1 + y2) / 2;
        float length = isHorizontal ? x2 - x1 : y2 - y1;

        // Set position and size
        rectTransform.anchoredPosition = new Vector2(centerX, centerY);
        rectTransform.sizeDelta = isHorizontal
            ? new Vector2(length, gridLineThickness)
            : new Vector2(gridLineThickness, length);

        if (!isHorizontal)
        {
            rectTransform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    private void CreateAxisLabel(string text, float x, float y, TextAlignmentOptions alignment, bool isYAxis)
    {
        GameObject labelObj = Instantiate(axisLabelPrefab, gridContainer.transform);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();

        if (label != null)
        {
            label.text = text;
            label.alignment = alignment;
            label.fontSize = 10;
            label.color = gridColor;
        }

        // Position the label
        if (isYAxis)
        {
            labelRect.anchoredPosition = new Vector2(x, y);
            labelRect.sizeDelta = new Vector2(30, 20);
        }
        else
        {
            labelRect.anchoredPosition = new Vector2(x, y);
            labelRect.sizeDelta = new Vector2(40, 20);
        }
    }

    private void CreateLegend()
    {
        if (legendContainer == null || legendPrefab == null) return;

        // Clear existing legend items
        foreach (Transform child in legendContainer)
        {
            Destroy(child.gameObject);
        }

        // Create legend items for each resource
        if (showFood)
        {
            CreateLegendItem("Food", foodColor);
        }

        if (showWealth)
        {
            CreateLegendItem("Wealth", wealthColor);
        }

        if (showGoods)
        {
            CreateLegendItem("Goods", goodsColor);
        }

        if (showStone)
        {
            CreateLegendItem("Stone", stoneColor);
        }
    }

    private void CreateLegendItem(string name, Color color)
    {
        GameObject legendItem = Instantiate(legendPrefab, legendContainer);

        // Find the color swatch and text
        Image colorSwatch = legendItem.transform.Find("ColorSwatch")?.GetComponent<Image>();
        TMPro.TextMeshProUGUI nameText = legendItem.transform.Find("NameText")?.GetComponent<TMPro.TextMeshProUGUI>();

        if (colorSwatch != null)
        {
            colorSwatch.color = color;
        }

        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    public void UpdateChart()
    {
        if (EconomyManager.Instance == null) return;

        // Clear existing lines (but not grid lines)
        ClearLines(foodLineContainer);
        ClearLines(wealthLineContainer);
        ClearLines(goodsLineContainer);
        ClearLines(stoneLineContainer);

        // Draw lines for each resource
        if (showFood || showAllResources)
        {
            List<float> foodHistory = EconomyManager.Instance.GetResourceHistory(ResourceType.Food, dataPoints);
            DrawLines(foodHistory, foodLineContainer, foodColor);
        }

        if (showWealth || showAllResources)
        {
            List<float> wealthHistory = EconomyManager.Instance.GetResourceHistory(ResourceType.Wealth, dataPoints);
            DrawLines(wealthHistory, wealthLineContainer, wealthColor);
        }

        if (showGoods || showAllResources)
        {
            List<float> goodsHistory = EconomyManager.Instance.GetResourceHistory(ResourceType.Goods, dataPoints);
            DrawLines(goodsHistory, goodsLineContainer, goodsColor);
        }

        if (showStone || showAllResources)
        {
            List<float> stoneHistory = EconomyManager.Instance.GetResourceHistory(ResourceType.Stone, dataPoints);
            DrawLines(stoneHistory, stoneLineContainer, stoneColor);
        }
    }

    private void DrawLines(List<float> dataPoints, GameObject container, Color color)
    {
        if (dataPoints.Count < 2) return;

        float chartWidth = chartContainer.rect.width;
        float chartHeight = chartContainer.rect.height;

        // Calculate x step based on number of points
        float xStep = chartWidth / (dataPoints.Count - 1);

        // Draw lines connecting points
        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            // Calculate positions for current and next point
            float x1 = i * xStep;
            float x2 = (i + 1) * xStep;

            float normalizedY1 = Mathf.InverseLerp(minValue, maxValue, dataPoints[i]);
            float normalizedY2 = Mathf.InverseLerp(minValue, maxValue, dataPoints[i + 1]);

            float y1 = normalizedY1 * chartHeight;
            float y2 = normalizedY2 * chartHeight;

            // Draw line between points
            DrawLine(container, color, x1, y1, x2, y2);

            // Draw point at current position
            DrawPoint(container, color, x1, y1);

            // Draw the final point
            if (i == dataPoints.Count - 2)
            {
                DrawPoint(container, color, x2, y2);
            }
        }
    }

    private void DrawLine(GameObject container, Color color, float x1, float y1, float x2, float y2)
    {
        if (linePrefab == null) return;

        // Instantiate line image
        Image line = Instantiate(linePrefab, container.transform);
        line.color = color;

        // Calculate line position and size
        RectTransform rectTransform = line.GetComponent<RectTransform>();

        // Calculate the center position of the line
        float centerX = (x1 + x2) / 2;
        float centerY = (y1 + y2) / 2;

        // Calculate the length and angle of the line
        float length = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
        float angle = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;

        // Set position, rotation and size
        rectTransform.anchoredPosition = new Vector2(centerX, centerY);
        rectTransform.sizeDelta = new Vector2(length, lineThickness);
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void DrawPoint(GameObject container, Color color, float x, float y)
    {
        if (pointPrefab == null) return;

        // Instantiate point image
        Image point = Instantiate(pointPrefab, container.transform);
        point.color = color;

        // Position the point
        RectTransform rectTransform = point.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(x, y);
    }

    private void ClearLines(GameObject container)
    {
        if (container == null) return;

        // Destroy all children
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }
}