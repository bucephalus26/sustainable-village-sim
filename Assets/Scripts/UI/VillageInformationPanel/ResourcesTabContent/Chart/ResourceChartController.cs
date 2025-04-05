using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public class ResourceChartController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UILineChart lineChart;
    [SerializeField] private TextMeshProUGUI chartTitle;
    [SerializeField] private Transform xAxisLabelContainer;
    [SerializeField] private GameObject xAxisLabelTemplate;
    [SerializeField] private Transform yAxisLabelContainer;
    [SerializeField] private GameObject yAxisLabelTemplate;
    [SerializeField] private Transform legendContainer;
    [SerializeField] private GameObject legendItemTemplate;

    [Header("Chart Configuration")]
    [SerializeField] private string titleFormat = "Resource History (Last {0} Days)";
    [SerializeField] private int historyDaysToShow = 5;
    [SerializeField] private int yAxisSteps = 4; // For 0, 25, 50, 75, 100
    [SerializeField] private float yAxisPadding = 0.1f; // Add 10% padding above max value

    [Header("Resource Colors")]
    [SerializeField] private Color foodColor = new(46f / 255f, 204f / 255f, 113f / 255f); // #2ecc71
    [SerializeField] private Color wealthColor = new(241f / 255f, 196f / 255f, 15f / 255f); // #f1c40f
    [SerializeField] private Color goodsColor = new(230f / 255f, 126f / 255f, 34f / 255f); // #e67e22
    [SerializeField] private Color stoneColor = new(149f / 255f, 165f / 255f, 166f / 255f); // #95a5a6

    private Dictionary<ResourceType, Color> resourceColors;
    private List<GameObject> activeXLabels = new();
    private List<GameObject> activeYLabels = new();
    private List<GameObject> activeLegendItems = new();

    void Awake()
    {
        resourceColors = new Dictionary<ResourceType, Color> {
            { ResourceType.Food, foodColor },
            { ResourceType.Wealth, wealthColor },
            { ResourceType.Goods, goodsColor },
            { ResourceType.Stone, stoneColor }
        };

        if (xAxisLabelTemplate != null) xAxisLabelTemplate.SetActive(false);
        if (yAxisLabelTemplate != null) yAxisLabelTemplate.SetActive(false);
        if (legendItemTemplate != null) legendItemTemplate.SetActive(false);
    }

    public void UpdateChart()
    {
        if (EconomyManager.Instance == null || lineChart == null)
        {
            Debug.LogWarning("EconomyManager or UILineChart not found.");
            return;
        }

        // 1. Prepare Data
        List<UILineChart.LineData> chartData = new();
        float overallMinY = float.MaxValue;
        float overallMaxY = float.MinValue;
        int maxDataPoints = 0; // How many days/points are we plotting

        foreach (var pair in resourceColors)
        {
            ResourceType type = pair.Key;
            List<float> history = EconomyManager.Instance.GetResourceHistory(type, historyDaysToShow);

            if (history.Count > 0)
            {
                chartData.Add(new UILineChart.LineData
                {
                    points = history,
                    color = pair.Value,
                    name = type.ToString()
                });

                // Track min/max values across all datasets being shown
                float currentMin = history.Min();
                float currentMax = history.Max();
                if (currentMin < overallMinY) overallMinY = currentMin;
                if (currentMax > overallMaxY) overallMaxY = currentMax;

                // Track the maximum number of points (days) we have data for
                maxDataPoints = Mathf.Max(maxDataPoints, history.Count);
            }
        }

        // Handle case with no data or flat data
        if (overallMinY == float.MaxValue) overallMinY = 0;
        if (overallMaxY == float.MinValue) overallMaxY = 100; // Default max if no data
        if (overallMaxY <= overallMinY) overallMaxY = overallMinY + 10; // Ensure max > min

        // Add padding to the top
        float range = overallMaxY - overallMinY;
        overallMaxY += range * yAxisPadding;
        overallMinY = Mathf.Max(0, overallMinY); // Assuming resources can't be negative

        // Round Y axis labels nicely (optional but good practice)
        // Find a nice step value
        float niceRange = GetNiceRange(overallMinY, overallMaxY, yAxisSteps + 1);
        float niceStep = niceRange / yAxisSteps;
        float niceMinY = Mathf.Floor(overallMinY / niceStep) * niceStep;
        float niceMaxY = Mathf.Ceil(overallMaxY / niceStep) * niceStep;

        if (niceMaxY <= niceMinY) niceMaxY = niceMinY + niceStep; // Ensure max > min after rounding


        // Determine the number of X-axis points (days) based on the max data points found
        // If we want to *always* show 5 days even if data is less, use historyDaysToShow
        int actualXPoints = Mathf.Min(maxDataPoints, historyDaysToShow);
        if (actualXPoints < 1) actualXPoints = 1; // Need at least one point/label

        // 2. Update Chart Title
        if (chartTitle != null)
        {
            chartTitle.text = string.Format(titleFormat, actualXPoints);
        }

        // 3. Update Y Axis Labels
        UpdateYAxisLabels(niceMinY, niceMaxY, yAxisSteps);

        // 4. Update X Axis Labels
        UpdateXAxisLabels(actualXPoints); // Pass the actual number of points

        // 5. Update Legend
        UpdateLegend(chartData);

        // 6. Pass data to the UILineChart for drawing
        lineChart.horizontalGridLines = yAxisSteps;

        lineChart.SetData(chartData, niceMinY, niceMaxY, actualXPoints);
    }

    private void UpdateYAxisLabels(float minY, float maxY, int steps)
    {
        ClearLabels(activeYLabels);
        if (yAxisLabelContainer == null || yAxisLabelTemplate == null || steps <= 0) return;

        float stepValue = (maxY - minY) / steps;

        for (int i = steps; i >= 0; i--)
        {
            GameObject labelObj = Instantiate(yAxisLabelTemplate, yAxisLabelContainer);
            TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
            {
                float value = minY + (i * stepValue);
                string format = (maxY - minY < 10f && maxY - minY > 0) ? "N1" : "N0";
                labelText.text = value.ToString(format);
            }
            labelObj.SetActive(true);
            activeYLabels.Add(labelObj);
        }
    }

    private void UpdateXAxisLabels(int pointCount)
    {
        ClearLabels(activeXLabels);
        if (xAxisLabelContainer == null || xAxisLabelTemplate == null || pointCount <= 0) return;

        // Determine the current game day
        int currentDay = (TimeManager.Instance != null) ? TimeManager.Instance.CurrentDay : pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            GameObject labelObj = Instantiate(xAxisLabelTemplate, xAxisLabelContainer);
            TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
            {
                // Calculate the day number for this historical point
                // Assuming the last point is 'currentDay', the first is 'currentDay - pointCount + 1'
                int dayNumber = currentDay - pointCount + 1 + i;
                labelText.text = $"Day {dayNumber}";
            }
            labelObj.SetActive(true);
            activeXLabels.Add(labelObj);
        }
    }

    private void UpdateLegend(List<UILineChart.LineData> data)
    {
        ClearLabels(activeLegendItems);
        if (legendContainer == null || legendItemTemplate == null) return;

        foreach (var dataSet in data)
        {
            GameObject itemObj = Instantiate(legendItemTemplate, legendContainer);
            Image swatch = itemObj.transform.Find("ColorSwatch")?.GetComponent<Image>();
            TextMeshProUGUI label = itemObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            if (swatch != null) swatch.color = dataSet.color;
            if (label != null) label.text = dataSet.name;

            itemObj.SetActive(true);
            activeLegendItems.Add(itemObj);
        }
    }

    private void ClearLabels(List<GameObject> labelList)
    {
        foreach (GameObject obj in labelList)
        {
            Destroy(obj);
        }
        labelList.Clear();
    }

    // Helper to calculate a "nice" number range for axis scales
    private float GetNiceRange(float min, float max, int targetSteps)
    {
        if (targetSteps <= 0) targetSteps = 1;
        double range = max - min;
        if (range == 0) return 10.0f; // Handle zero range

        double roughStep = range / (targetSteps - 1);
        double[] niceSteps = { 1, 2, 2.5, 5, 10 }; // Common nice steps

        double stepPower = Math.Pow(10, -Math.Floor(Math.Log10(Math.Abs(roughStep))));
        double normalizedStep = roughStep * stepPower;

        double bestNiceStep = niceSteps[niceSteps.Length - 1]; // Default to largest
        foreach (double niceStep in niceSteps)
        {
            if (niceStep >= normalizedStep)
            {
                bestNiceStep = niceStep;
                break;
            }
        }

        return (float)(bestNiceStep / stepPower);
    }

}