using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageInformationPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button togglePanelButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;

    [Header("Tab Navigation")]
    [SerializeField] private Button resourcesTabButton;
    [SerializeField] private Button populationTabButton;
    [SerializeField] private Button goalsTabButton;
    [SerializeField] private Image resourcesActiveBar;
    [SerializeField] private Image populationActiveBar;
    [SerializeField] private Image goalsActiveBar;

    [Header("Tab Content")]
    [SerializeField] private GameObject tabContentContainer;
    [SerializeField] private GameObject resourcesTabContent;
    [SerializeField] private GameObject populationTabContent;
    [SerializeField] private GameObject goalsTabContent;

    [Header("Dashboard")]
    [SerializeField] private Button dashboardButton;

    [Header("Panel Settings")]
    [SerializeField] private RectTransform panelRectTransform;

    // Panel sizes
    private Vector2 expandedSize;
    private Vector2 collapsedSize;
    private bool isPanelCollapsed = false;

    // Currently active tab
    private enum TabType { Resources, Population, Goals }
    private TabType activeTab = TabType.Resources;

    // Colors for tab states
    private Color tabActiveColor = new(0.2f, 0.6f, 0.9f, 0.4f);
    private Color tabHoverColor = new(1f, 1f, 1f, 0.1f);
    private Color tabNormalColor = new(0f, 0f, 0f, 0f);

    private void Start()
    {
        // Get panel RectTransform if not set
        if (panelRectTransform == null)
        {
            panelRectTransform = GetComponent<RectTransform>();
        }

        // Ensure pivot is set to top-left (0,1)
        panelRectTransform.pivot = new Vector2(0, 1);

        // Store panel sizes
        expandedSize = panelRectTransform.sizeDelta;

        // Calculate collapsed size (without content area)
        float contentHeight = tabContentContainer ? tabContentContainer.GetComponent<RectTransform>().rect.height : 0;
        collapsedSize = new Vector2(expandedSize.x, expandedSize.y - contentHeight);

        // Set up button listeners
        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.AddListener(TogglePanelCollapse);
        }

        if (resourcesTabButton != null)
        {
            resourcesTabButton.onClick.AddListener(() => TabButtonClicked(TabType.Resources));
            ConfigureTabButtonColors(resourcesTabButton);
        }

        if (populationTabButton != null)
        {
            populationTabButton.onClick.AddListener(() => TabButtonClicked(TabType.Population));
            ConfigureTabButtonColors(populationTabButton);
        }

        if (goalsTabButton != null)
        {
            goalsTabButton.onClick.AddListener(() => TabButtonClicked(TabType.Goals));
            ConfigureTabButtonColors(goalsTabButton);
        }

        if (dashboardButton != null)
        {
            dashboardButton.onClick.AddListener(OpenDashboard);
        }

        // Initialize UI state
        SetActiveTab(activeTab);
    }

    private void ConfigureTabButtonColors(Button button)
    {
        // Configure tab button visual states
        ColorBlock colors = button.colors;
        colors.normalColor = tabNormalColor;
        colors.highlightedColor = tabHoverColor;
        colors.pressedColor = tabActiveColor;
        colors.selectedColor = tabActiveColor;
        button.colors = colors;
    }

    private void TabButtonClicked(TabType tab)
    {
        // If panel is collapsed, expand it first
        if (isPanelCollapsed)
        {
            ExpandPanel();
        }

        // Set the active tab
        SetActiveTab(tab);
    }

    private void SetActiveTab(TabType tab)
    {
        // Update active tab
        activeTab = tab;

        // Hide all content first
        if (resourcesTabContent != null) resourcesTabContent.SetActive(false);
        if (populationTabContent != null) populationTabContent.SetActive(false);
        if (goalsTabContent != null) goalsTabContent.SetActive(false);

        // Hide all active bars
        if (resourcesActiveBar != null) resourcesActiveBar.gameObject.SetActive(false);
        if (populationActiveBar != null) populationActiveBar.gameObject.SetActive(false);
        if (goalsActiveBar != null) goalsActiveBar.gameObject.SetActive(false);

        // Show active content and indicator
        switch (tab)
        {
            case TabType.Resources:
                if (resourcesTabContent != null) resourcesTabContent.SetActive(true);
                if (resourcesActiveBar != null) resourcesActiveBar.gameObject.SetActive(true);
                break;

            case TabType.Population:
                if (populationTabContent != null) populationTabContent.SetActive(true);
                if (populationActiveBar != null) populationActiveBar.gameObject.SetActive(true);
                break;

            case TabType.Goals:
                if (goalsTabContent != null) goalsTabContent.SetActive(true);
                if (goalsActiveBar != null) goalsActiveBar.gameObject.SetActive(true);
                break;
        }
    }

    public void TogglePanelCollapse()
    {
        if (isPanelCollapsed)
        {
            ExpandPanel();
        }
        else
        {
            CollapsePanel();
        }
    }

    private void CollapsePanel()
    {
        isPanelCollapsed = true;

        // Update toggle button text
        if (toggleButtonText != null)
        {
            toggleButtonText.text = "+";
        }

        // Hide content immediately
        if (tabContentContainer != null)
        {
            tabContentContainer.SetActive(false);
        }

        // Change panel size immediately
        panelRectTransform.sizeDelta = collapsedSize;
    }

    private void ExpandPanel()
    {
        isPanelCollapsed = false;

        // Update toggle button text
        if (toggleButtonText != null)
        {
            toggleButtonText.text = "-";
        }

        // Change panel size immediately
        panelRectTransform.sizeDelta = expandedSize;

        // Show content immediately
        if (tabContentContainer != null)
        {
            tabContentContainer.SetActive(true);
        }
    }

    public void OpenDashboard()
    {
        // Find the dashboard GameObject and show it
        GameObject dashboard = UIManager.Instance.GetDashboard();
        if (dashboard != null)
        {
            dashboard.SetActive(true);
        }

        Debug.Log("Opening dashboard");
    }
}