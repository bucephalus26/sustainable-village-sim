using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopulationTabContent : MonoBehaviour
{
    [Header("Header References")]
    [SerializeField] private TextMeshProUGUI totalPopulationText;
    [SerializeField] private TextMeshProUGUI happinessIconText;
    [SerializeField] private TextMeshProUGUI happinessValueText;

    [Header("Profession List")]
    [SerializeField] private Transform professionListContainer;
    [SerializeField] private GameObject professionItemTemplate;

    [Header("Filter References")]
    [SerializeField] private TMP_Dropdown professionFilterDropdown;
    [SerializeField] private TMP_Dropdown activityFilterDropdown;
    [SerializeField] private TMP_Dropdown moodFilterDropdown;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 1.0f;
    private float updateTimer = 0f;

    // VillagerManager, Profession tracking
    private VillagerManager villagerManager;
    private Dictionary<ProfessionType, int> professionCounts = new Dictionary<ProfessionType, int>();
    private Dictionary<ProfessionType, ProfessionDisplayItem> professionDisplayItems = new Dictionary<ProfessionType, ProfessionDisplayItem>();

    // Current filter settings
    private string selectedProfession = "All Professions";
    private string selectedActivity = "All Activities";
    private string selectedMood = "All Moods";

    void Start()
    {
        villagerManager = VillagerManager.Instance;

        if (professionItemTemplate != null) professionItemTemplate.SetActive(false);

        InitializeFilters();
        CreateProfessionItems();

        // UI Listeners
        professionFilterDropdown?.onValueChanged.AddListener(OnProfessionFilterChanged);
        activityFilterDropdown?.onValueChanged.AddListener(OnActivityFilterChanged);
        moodFilterDropdown?.onValueChanged.AddListener(OnMoodFilterChanged);

        if (gameObject.activeInHierarchy)
        {
            UpdateUI();
            ApplyFilters();
        }
    }

    void OnEnable()
    {
        // Update UI and Filters when tab becomes visible
        UpdateUI();
        ApplyFilters();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || villagerManager == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateUI();
        }
    }

    private void InitializeFilters()
    {
        // Profession filter
        if (professionFilterDropdown != null)
        {
            professionFilterDropdown.ClearOptions();
            List<string> options = new() { "All Professions" };
            foreach (ProfessionType type in System.Enum.GetValues(typeof(ProfessionType)))
            {
                options.Add(type.ToString());
            }
            professionFilterDropdown.AddOptions(options);
        }

        // Activity filter
        if (activityFilterDropdown != null)
        {
            activityFilterDropdown.ClearOptions();
            List<string> options = new() { "All Activities", "Working", "Socializing", "Sleeping", "Idle", "Fulfilling Need" };
            activityFilterDropdown.AddOptions(options);
        }

        // Mood filter
        if (moodFilterDropdown != null)
        {
            moodFilterDropdown.ClearOptions();
            List<string> options = new() { "All Moods", "Happy", "Content", "Unhappy" };
            moodFilterDropdown.AddOptions(options);
        }
    }

    private void CreateProfessionItems()
    {
        if (professionListContainer == null || professionItemTemplate == null) return;

        // Clear existing items
        foreach (Transform child in professionListContainer) { Destroy(child.gameObject); }
        professionDisplayItems.Clear();

        foreach (ProfessionType type in System.Enum.GetValues(typeof(ProfessionType)))
        {
            GameObject newItem = Instantiate(professionItemTemplate, professionListContainer);
            ProfessionDisplayItem displayItem = newItem.GetComponent<ProfessionDisplayItem>();

            if (displayItem != null)
            {
                ProfessionData profData = villagerManager.GetProfessionData(type);
                displayItem.Initialize(type, profData);
                professionDisplayItems[type] = displayItem;
                newItem.SetActive(false);
            }
        }

    }

    // Main UI update function
    public void UpdateUI()
    {
        if (villagerManager == null) return;

        List<Villager> villagers = villagerManager.GetVillagers();
        int totalVillagers = villagers.Count;

        // Update Header
        if (totalPopulationText != null)
            totalPopulationText.text = $"{totalVillagers} Villagers";

        float avgHappiness = villagerManager.GetAverageHappiness();
        UpdateHappinessDisplay(avgHappiness);

        // Update Profession Counts and Visibility
        UpdateProfessionCounts(villagers);
    }

    void UpdateHappinessDisplay(float averageHappiness)
    {
        if (happinessValueText != null)
            happinessValueText.text = $"{averageHappiness:F0}%";

        if (happinessIconText != null)
        {
            if (averageHappiness >= 70f) happinessIconText.text = "😃";
            else if (averageHappiness >= 40f) happinessIconText.text = "😐";
            else happinessIconText.text = "😞";
        }
    }

    private void UpdateProfessionCounts(List<Villager> villagers)
    {
        // Reset counts
        professionCounts.Clear();
        foreach (ProfessionType type in System.Enum.GetValues(typeof(ProfessionType)))
        {
            professionCounts[type] = 0;
        }

        // Count current villagers
        foreach (var villager in villagers)
        {
            ProfessionType profession = villager.Brain?.Profession?.ProfessionType ?? ProfessionType.Unemployed;
            if (professionCounts.ContainsKey(profession))
            {
                professionCounts[profession]++;
            }
        }

        // Update the display items
        foreach (var kvp in professionDisplayItems)
        {
            ProfessionType type = kvp.Key;
            ProfessionDisplayItem displayItem = kvp.Value;
            int count = professionCounts.ContainsKey(type) ? professionCounts[type] : 0;

            displayItem.UpdateCount(count);
            // Show/Hide the item based on count
            displayItem.gameObject.SetActive(count > 0);
        }
    }

    private void OnProfessionFilterChanged(int index)
    {
        if (professionFilterDropdown != null)
            selectedProfession = professionFilterDropdown.options[index].text;
        ApplyFilters();
    }

    private void OnActivityFilterChanged(int index)
    {
        if (activityFilterDropdown != null)
            selectedActivity = activityFilterDropdown.options[index].text;
        ApplyFilters();
    }

    private void OnMoodFilterChanged(int index)
    {
        if (moodFilterDropdown != null)
            selectedMood = moodFilterDropdown.options[index].text;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (villagerManager == null) return;

        List<Villager> allVillagers = villagerManager.GetVillagers();

        foreach (var villager in allVillagers)
        {
            bool shouldBeVisible = CheckVillagerVisibility(villager);
            villager.SetVisibility(shouldBeVisible);
        }
    }

    // Checks if a single villager meets the current filter criteria
    private bool CheckVillagerVisibility(Villager villager)
    {
        // Check Profession Filter
        if (selectedProfession != "All Professions")
        {
            ProfessionType currentVillagerProfession = villager.Brain?.Profession?.ProfessionType ?? ProfessionType.Unemployed;
            if (currentVillagerProfession.ToString() != selectedProfession)
            {
                return false;
            }
        }

        // Check Activity Filter
        if (selectedActivity != "All Activities")
        {
            if (villager.Brain == null || !MatchesActivityFilter(villager.Brain, selectedActivity))
            {
                return false;
            }
        }

        // Check Mood Filter
        if (selectedMood != "All Moods")
        {
            if (!MatchesMoodFilter(villager, selectedMood))
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesActivityFilter(VillagerBrain brain, string activityFilterString)
    {
        if (brain.CurrentState == null) return activityFilterString == "Idle";

        var stateType = brain.CurrentState.GetType();

        return activityFilterString switch
        {
            "Working" => stateType == typeof(WorkingState),
            "Socializing" => stateType == typeof(SocializingState),
            "Sleeping" => stateType == typeof(SleepingState),
            "Fulfilling Need" => stateType == typeof(NeedFulfillmentState),
            "Idle" => stateType == typeof(IdleState),
            _ => false,
        };
    }

    private bool MatchesMoodFilter(Villager villager, string moodFilterString)
    {
        float currentHappiness = villager.happiness;

        switch (moodFilterString)
        {
            case "Happy": return currentHappiness >= 70f;
            case "Content": return currentHappiness >= 40f && currentHappiness < 70f;
            case "Unhappy": return currentHappiness < 40f;
            default: return false;
        }
    }
}
