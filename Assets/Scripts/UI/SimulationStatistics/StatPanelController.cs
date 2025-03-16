using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the expansion and collapsing of stat panels based on the detailed view toggle
/// </summary>
public class StatPanelController : MonoBehaviour
{
    [Header("Panel Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI basicContentText;
    public TextMeshProUGUI detailedContentText;
    public CanvasGroup detailedCanvasGroup;
    public Button expandButton;
    public RectTransform expandButtonRect;

    [Header("Animation")]
    public float expandDuration = 0.2f;
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Private members
    private Toggle detailedStatsToggle;
    private bool isExpanded = false;
    private ContentSizeFitter contentSizeFitter;
    private float expandProgress = 0f;
    private bool isAnimating = false;

    // Properties to expose text elements for SimulationStatistics
    public TextMeshProUGUI BasicContent => basicContentText;
    public TextMeshProUGUI DetailedContent => detailedContentText;

    private void Awake()
    {
        contentSizeFitter = GetComponent<ContentSizeFitter>();

        // Setup initial state
        SetExpanded(false, false);

        // Setup expand button
        if (expandButton != null)
        {
            expandButton.onClick.AddListener(ToggleExpand);
        }
    }

    private void Start()
    {
        // Find the detailed stats toggle from SimulationStatistics
        SimulationStatistics stats = FindFirstObjectByType<SimulationStatistics>();
        if (stats != null && stats.ShowDetailedStats != null)
        {
            detailedStatsToggle = stats.ShowDetailedStats;
            detailedStatsToggle.onValueChanged.AddListener(OnDetailedToggleChanged);

            // Initialize state based on current toggle value
            OnDetailedToggleChanged(detailedStatsToggle.isOn);
        }
    }

    private void Update()
    {
        // Handle smooth animation if needed
        if (isAnimating)
        {
            AnimateExpansion();
        }
    }

    private void OnDetailedToggleChanged(bool showDetailed)
    {
        // Enable or disable the expand button
        if (expandButton != null)
        {
            expandButton.gameObject.SetActive(showDetailed);
        }

        // If detailed view is turned off, collapse the panel
        if (!showDetailed && isExpanded)
        {
            SetExpanded(false);
        }

        // If there's no detailed content, hide the expand button
        if (string.IsNullOrEmpty(detailedContentText.text.Trim()))
        {
            expandButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle between expanded and collapsed states
    /// </summary>
    public void ToggleExpand()
    {
        SetExpanded(!isExpanded);
    }

    /// <summary>
    /// Set the expanded state with optional animation
    /// </summary>
    public void SetExpanded(bool expanded, bool animate = true)
    {
        // Don't change if already in that state
        if (isExpanded == expanded && !isAnimating)
            return;

        isExpanded = expanded;

        if (animate && gameObject.activeInHierarchy)
        {
            // Start animation
            isAnimating = true;
            expandProgress = expanded ? 0f : 1f;
        }
        else
        {
            // Immediate change
            detailedCanvasGroup.alpha = expanded ? 1f : 0f;
            detailedCanvasGroup.interactable = expanded;
            detailedCanvasGroup.blocksRaycasts = expanded;

            // Update button rotation
            if (expandButtonRect != null)
            {
                expandButtonRect.localRotation = Quaternion.Euler(0, 0, expanded ? 180f : 0f);
            }

            // Force layout rebuild
            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
        }
    }

    /// <summary>
    /// Set the title of the panel
    /// </summary>
    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    /// <summary>
    /// Set the basic content text
    /// </summary>
    public void SetBasicContent(string content)
    {
        if (basicContentText != null)
        {
            basicContentText.text = content;
        }
    }

    /// <summary>
    /// Set the detailed content text
    /// </summary>
    public void SetDetailedContent(string content)
    {
        if (detailedContentText != null)
        {
            detailedContentText.text = content;

            // Hide expand button if there's no detailed content
            if (expandButton != null)
            {
                expandButton.gameObject.SetActive(!string.IsNullOrEmpty(content.Trim()) &&
                                               detailedStatsToggle != null &&
                                               detailedStatsToggle.isOn);
            }
        }
    }

    /// <summary>
    /// Handle smooth animation of panel expansion
    /// </summary>
    private void AnimateExpansion()
    {
        // Update progress
        float direction = isExpanded ? 1f : -1f;
        expandProgress += Time.deltaTime / expandDuration * direction;

        // Clamp and check if animation is complete
        if (expandProgress >= 1f)
        {
            expandProgress = 1f;
            isAnimating = false;
        }
        else if (expandProgress <= 0f)
        {
            expandProgress = 0f;
            isAnimating = false;
        }

        // Apply curve
        float curvedProgress = expandCurve.Evaluate(expandProgress);

        // Update visuals
        detailedCanvasGroup.alpha = curvedProgress;
        detailedCanvasGroup.interactable = curvedProgress > 0.5f;
        detailedCanvasGroup.blocksRaycasts = curvedProgress > 0.5f;

        // Update button rotation
        if (expandButtonRect != null)
        {
            expandButtonRect.localRotation = Quaternion.Euler(0, 0, curvedProgress * 180f);
        }

        // Force layout rebuild
        if (contentSizeFitter != null && !isAnimating)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}