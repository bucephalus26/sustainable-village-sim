using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceDisplayItem : MonoBehaviour
{
    [Header("Resource Type")]
    [SerializeField] private ResourceType resourceType;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI resourceNameText;
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI trendText;
    [SerializeField] private Image fillBarImage;
    [SerializeField] private TextMeshProUGUI priceText;

    // Resource display settings
    [Header("Display Settings")]
    [SerializeField] private float maxResourceAmount = 200f; // For fill bar scaling
    [SerializeField] private Color positiveColor = new(0.18f, 0.8f, 0.44f, 1f); // Green for positive trend
    [SerializeField] private Color negativeColor = new(0.91f, 0.3f, 0.24f, 1f); // Red for negative trend
    [SerializeField] private Color neutralColor = new(0.78f, 0.78f, 0.78f, 1f); // Grey for neutral trend

    private void Start()
    {
        // Set resource name if not already set
        if (resourceNameText != null && string.IsNullOrEmpty(resourceNameText.text))
        {
            resourceNameText.text = resourceType.ToString();
        }

        // Initial update
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (EconomyManager.Instance == null) return;

        // Get current resource data
        float amount = EconomyManager.Instance.GetResourceAmount(resourceType);
        float price = EconomyManager.Instance.GetResourcePrice(resourceType);

        // Update amount text
        if (amountText != null)
        {
            amountText.text = amount.ToString("F1");
        }

        // Update price text
        if (priceText != null)
        {
            priceText.text = $"Current Price: {price.ToString("F1")}";
        }

        // Update fill bar
        if (fillBarImage != null)
        {
            fillBarImage.fillAmount = Mathf.Clamp01(amount / maxResourceAmount);
        }

        // Update trend
        UpdateTrend();
    }

    private void UpdateTrend()
    {
        if (trendText == null) return;

        float dailyChange = EconomyManager.Instance.GetResourceDailyChange(resourceType);

        // Display trend
        if (Mathf.Abs(dailyChange) < 0.1f)
        {
            trendText.text = "0.0/day";
            trendText.color = neutralColor;
        }
        else if (dailyChange > 0)
        {
            trendText.text = $"+{dailyChange.ToString("F1")}/day";
            trendText.color = positiveColor;
        }
        else
        {
            trendText.text = $"{dailyChange.ToString("F1")}/day";
            trendText.color = negativeColor;
        }
    }
}