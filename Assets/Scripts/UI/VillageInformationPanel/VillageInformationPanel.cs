using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageInformationPanel : MonoBehaviour
{
    [Header("Panel Components")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private Button dashboardButton;

    [Header("Tab System")]
    [SerializeField] private VillageInformationPanelTabController tabController;

    [Header("Tab Contents")]
    [SerializeField] private GameObject resourcesTab;
    [SerializeField] private GameObject populationTab;
    [SerializeField] private GameObject goalsTab;

    private bool isCollapsed = false;
    private float expandedHeight;

    public bool IsCollapsed => isCollapsed;

    private void Start()
    {
        // Store the original height
        expandedHeight = ((RectTransform)transform).sizeDelta.y;

        // Set up toggle button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }

        // Set up dashboard button
        if (dashboardButton != null)
        {
            dashboardButton.onClick.AddListener(OpenDashboard);
        }

        // Default to showing the Resources tab
        if (tabController != null)
        {
            tabController.SelectTab(0); // 0 = Resources, 1 = Population, 2 = Goals
        }
    }

    public void TogglePanel()
    {
        isCollapsed = !isCollapsed;

        // Update the toggle button text
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isCollapsed ? "+" : "-";
        }

        // Resize the panel
        var rectTransform = (RectTransform)transform;
        if (isCollapsed)
        {
            // Only show the header when collapsed
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 50f);
            if (contentContainer != null)
                contentContainer.gameObject.SetActive(false);
            if (dashboardButton != null)
                dashboardButton.gameObject.SetActive(false);
        }
        else
        {
            // Restore full size
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, expandedHeight);
            if (contentContainer != null)
                contentContainer.gameObject.SetActive(true);
            if (dashboardButton != null)
                dashboardButton.gameObject.SetActive(true);
        }
    }

    public void OpenDashboard()
    {
        // Notify UIManager to open the dashboard
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleMainDashboard(true);
        }
    }
}