using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageInformationPanelTabController : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private List<TabButton> tabButtons = new();

    [Header("Tab Content")]
    [SerializeField] private List<GameObject> tabContents = new();

    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public TextMeshProUGUI label;
        public Image background;
    }

    private int activeTabIndex = -1;

    private void Start()
    {
        // Set up tab button listeners
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i; // Create local copy for lambda
            if (tabButtons[i].button != null)
            {
                tabButtons[i].button.onClick.AddListener(() => SelectTab(index));
            }
        }

        // Start with the first tab selected
        if (tabButtons.Count > 0 && tabContents.Count > 0)
        {
            SelectTab(0);
        }
    }

    public void SelectTab(int index)
    {
        if (index < 0 || index >= tabButtons.Count || index >= tabContents.Count)
            return;

        // Don't re-select the current tab
        if (activeTabIndex == index)
            return;

        // Deactivate current tab
        if (activeTabIndex >= 0 && activeTabIndex < tabButtons.Count)
        {
            if (tabButtons[activeTabIndex].background != null)
            {
                tabButtons[activeTabIndex].background.color = new Color(0f, 0f, 0f, 0f); // Clear color
            }

            if (activeTabIndex < tabContents.Count && tabContents[activeTabIndex] != null)
            {
                tabContents[activeTabIndex].SetActive(false);
            }
        }

        // Activate new tab
        activeTabIndex = index;

        if (tabButtons[activeTabIndex].background != null)
        {
            tabButtons[activeTabIndex].background.color = new Color(0.2f, 0.6f, 0.9f, 0.4f); // Active blue
        }

        if (tabContents[activeTabIndex] != null)
        {
            tabContents[activeTabIndex].SetActive(true);
        }
    }
}
