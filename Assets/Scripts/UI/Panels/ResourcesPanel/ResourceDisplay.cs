using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays and manages a single resource type UI element
/// </summary>
public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private TMP_Text resourceLabel;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image backgroundImage;

    [Header("Styling")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private float criticalPulseSpeed = 2f;

    private bool isCritical = false;
    private float pulseTime = 0f;

    private void Start()
    {
        // Set label text based on resource type
        if (resourceLabel != null)
        {
            resourceLabel.text = $"{resourceType}";
        }
    }

    private void Update()
    {
        if (isCritical && backgroundImage != null)
        {
            // Create pulsing effect for critical resources
            pulseTime += Time.deltaTime * criticalPulseSpeed;
            float alpha = 0.7f + Mathf.Sin(pulseTime) * 0.3f;
            Color pulseColor = criticalColor;
            pulseColor.a = alpha;
            backgroundImage.color = pulseColor;
        }
    }

    public void UpdateDisplay(float amount)
    {
        if (valueText != null)
        {
            valueText.text = amount.ToString("F0");
        }
    }

    public void SetCriticalState(bool critical)
    {
        isCritical = critical;
        if (backgroundImage != null)
        {
            backgroundImage.color = critical ? criticalColor : normalColor;
        }

        // Reset pulse time when entering critical state
        if (critical)
        {
            pulseTime = 0f;
        }
    }
}
