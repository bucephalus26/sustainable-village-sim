using UnityEngine;
using System.Collections.Generic;

public class ResourcesTabContent : MonoBehaviour
{
    [Header("Resource Displays")]
    [SerializeField] private List<ResourceDisplayItem> resourceDisplays = new();

    [Header("Resource Chart")]
    [SerializeField] private ResourceChartController resourceChartController;

    [Header("Update Settings")]
    private float updateTimer = 0f;
    [SerializeField] private float updateInterval = 0.5f;

    private void Update()
    {
        // Update resource displays periodically
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateResourceDisplays();
        }
    }

    public void UpdateResourceDisplays()
    {
        foreach (var display in resourceDisplays)
        {
            if (display != null)
            {
                display.UpdateDisplay();
            }
        }

        // Update resource chart
        UpdateResourceChart();
    }

    private void UpdateResourceChart()
    {
        if (resourceChartController != null)
        {
            resourceChartController.UpdateChart();
        }
    }
}