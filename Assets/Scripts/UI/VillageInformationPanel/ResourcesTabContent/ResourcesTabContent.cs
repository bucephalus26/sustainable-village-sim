using UnityEngine;
using System.Collections.Generic;

public class ResourcesTabContent : MonoBehaviour
{
    [Header("Resource Displays")]
    [SerializeField] private List<ResourceDisplayItem> resourceDisplays = new();

    [Header("Resource Chart")]
    [SerializeField] private GameObject resourceChart;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f; // How often to update (in seconds)

    private float updateTimer = 0f;

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
        // This would update your resource history chart
        // For now, this is a placeholder
        if (resourceChart != null)
        {
            // Update chart data
            // resourceChart.GetComponent<ResourceChart>().UpdateChart();
        }
    }
}