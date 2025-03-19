using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class SimulationStatistics : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject statsPanel;
    [SerializeField] public TextMeshProUGUI timeDisplay;
    [SerializeField] public TextMeshProUGUI dayDisplay;
    [SerializeField] public TextMeshProUGUI resourcesDisplay;
    [SerializeField] public TextMeshProUGUI villagerStatsDisplay;
    [SerializeField] public TextMeshProUGUI stateStatsDisplay;
    [SerializeField] public TextMeshProUGUI moodStatsDisplay;
    [SerializeField] public TextMeshProUGUI goalStatsDisplay;
    [SerializeField] public Toggle showDetailedStats;

    // Tracking statistics
    private Dictionary<System.Type, int> villagerStateStats = new();
    private Dictionary<ProfessionType, int> professionStats = new();
    private Dictionary<Need, float> averageNeedValues = new();
    private Dictionary<GoalType, int> activeGoalStats = new();
    private Dictionary<GoalType, int> completedGoalStats = new();

    // Mood tracking
    private float totalHappiness = 0f;
    private int villagerCount = 0;
    private float averageHappiness = 50f;
    private float minHappiness = 100f;
    private float maxHappiness = 0f;
    private string happyVillager = "None";
    private string unhappyVillager = "None";
    private Dictionary<VillagerMood.MoodState, int> moodCounts = new()
    {
        { VillagerMood.MoodState.Happy, 0 },
        { VillagerMood.MoodState.Content, 0 },
        { VillagerMood.MoodState.Unhappy, 0 }
    };

    // Update timing
    private float updateInterval = 1.0f;
    private float timer = 0f;

    private void Start()
    {
        // Initialize the toggle
        if (showDetailedStats != null)
        {
            showDetailedStats.onValueChanged.AddListener(OnToggleDetailedStats);
        }

        // Subscribe to events
        EventManager.Instance.AddListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChange);
        EventManager.Instance.AddListener<VillagerEvents.VillagerInitializedEvent>(OnVillagerInitialized);
        EventManager.Instance.AddListener<VillagerEvents.MoodChangedEvent>(OnVillagerMoodChanged);

        // Initialize goal stats
        foreach (GoalType type in System.Enum.GetValues(typeof(GoalType)))
        {
            activeGoalStats[type] = 0;
            completedGoalStats[type] = 0;
        }

        // Initialize villager moods
        InitializeVillagerMoods();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChange);
            EventManager.Instance.RemoveListener<VillagerEvents.VillagerInitializedEvent>(OnVillagerInitialized);
            EventManager.Instance.RemoveListener<VillagerEvents.MoodChangedEvent>(OnVillagerMoodChanged);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateStats();
        }

        // Update time display every frame for smooth display
        if (timeDisplay != null && TimeManager.Instance != null)
        {
            timeDisplay.text = $"Time: {TimeManager.Instance.GetFormattedTime()} - {TimeManager.Instance.GetTimeOfDayName()}";
        }

        if (dayDisplay != null && TimeManager.Instance != null)
        {
            dayDisplay.text = $"Day: {TimeManager.Instance.CurrentDay}";
        }
    }

    private void InitializeVillagerMoods()
    {
        if (VillagerManager.Instance == null) return;

        var villagers = VillagerManager.Instance.GetVillagers();
        villagerCount = villagers.Count;

        // Reset counters
        moodCounts[VillagerMood.MoodState.Happy] = 0;
        moodCounts[VillagerMood.MoodState.Content] = 0;
        moodCounts[VillagerMood.MoodState.Unhappy] = 0;
        totalHappiness = 0f;

        // Initialize mood tracking
        foreach (var villager in villagers)
        {
            var moodComponent = villager.GetComponent<VillagerMood>();
            if (moodComponent != null)
            {
                float happiness = moodComponent.Happiness;
                totalHappiness += happiness;

                // Track min/max happiness
                UpdateHappinessExtremes(villager.villagerName, happiness);

                // Count mood categories
                moodCounts[moodComponent.CurrentMood]++;
            }
        }

        // Calculate average
        if (villagerCount > 0)
        {
            averageHappiness = totalHappiness / villagerCount;
        }
    }

    private void UpdateStats()
    {
        if (!statsPanel.activeSelf) return;

        // Update resource display
        UpdateResourceDisplay();

        // Update villager stats
        UpdateVillagerDisplay();

        // Update state distribution
        UpdateStateDistribution();

        // Update mood stats
        UpdateMoodDisplay();

        // Update goal stats
        UpdateGoalsDisplay();
    }

    private void UpdateResourceDisplay()
    {
        if (resourcesDisplay == null || EconomyManager.Instance == null) return;

        string resourceText = "Resources:\n";
        resourceText += $"Food: {EconomyManager.Instance.GetResourceAmount(ResourceType.Food):F1} (${EconomyManager.Instance.GetResourcePrice(ResourceType.Food):F2})\n";
        resourceText += $"Goods: {EconomyManager.Instance.GetResourceAmount(ResourceType.Goods):F1} (${EconomyManager.Instance.GetResourcePrice(ResourceType.Goods):F2})\n";
        resourceText += $"Wealth: {EconomyManager.Instance.GetResourceAmount(ResourceType.Wealth):F1}\n";
        resourceText += $"Stone: {EconomyManager.Instance.GetResourceAmount(ResourceType.Stone):F1} (${EconomyManager.Instance.GetResourcePrice(ResourceType.Stone):F2})";

        resourcesDisplay.text = resourceText;
    }

    private void UpdateVillagerDisplay()
    {
        if (villagerStatsDisplay == null || VillagerManager.Instance == null) return;

        var villagers = VillagerManager.Instance.GetVillagers();

        // Count professions
        professionStats.Clear();
        foreach (var villager in villagers)
        {
            var profManager = villager.GetComponent<VillagerProfession>();
            if (profManager != null)
            {
                var profType = profManager.ProfessionType;
                if (!professionStats.ContainsKey(profType))
                {
                    professionStats[profType] = 0;
                }
                professionStats[profType]++;
            }
        }

        // Calculate average need values
        averageNeedValues.Clear();
        if (showDetailedStats != null && showDetailedStats.isOn)
        {
            foreach (var villager in villagers)
            {
                var brain = villager.GetComponent<VillagerBrain>();
                if (brain != null && brain.NeedsManager != null)
                {
                    foreach (var need in brain.NeedsManager.GetAllNeeds())
                    {
                        if (!averageNeedValues.ContainsKey(need))
                        {
                            averageNeedValues[need] = 0;
                        }
                        averageNeedValues[need] += need.CurrentValue;
                    }
                }
            }

            // Calculate averages
            foreach (var need in averageNeedValues.Keys.ToList())
            {
                averageNeedValues[need] /= villagers.Count;
            }
        }

        // Build the display text
        string villagerText = "Villagers:\n";
        villagerText += $"Total: {villagers.Count}\n";

        // Add profession distribution
        foreach (var prof in professionStats)
        {
            villagerText += $"{prof.Key}: {prof.Value}\n";
        }

        villagerStatsDisplay.text = villagerText;
    }

    private void UpdateStateDistribution()
    {
        if (stateStatsDisplay == null) return;

        string stateText = "Current Activities:\n";

        // Add state distribution
        foreach (var stateStat in villagerStateStats)
        {
            string stateName = stateStat.Key.Name.Replace("State", "");
            stateText += $"{stateName}: {stateStat.Value}\n";
        }

        stateStatsDisplay.text = stateText;
    }

    private void UpdateMoodDisplay()
    {
        if (moodStatsDisplay == null || VillagerManager.Instance == null) return;

        // We'll only recalculate the total happiness and average
        // since the mood counts are maintained by the event system
        var villagers = VillagerManager.Instance.GetVillagers();

        // Reset total
        totalHappiness = 0f;

        // Sum up all happiness values
        foreach (var villager in villagers)
        {
            totalHappiness += villager.happiness;
        }

        // Recalculate average
        if (villagers.Count > 0)
        {
            averageHappiness = totalHappiness / villagers.Count;
        }

        // Build the display text
        string moodText = "Happiness:\n";
        moodText += $"Average: {averageHappiness:F1}\n";
        moodText += $"Happy: {moodCounts[VillagerMood.MoodState.Happy]} villagers\n";
        moodText += $"Content: {moodCounts[VillagerMood.MoodState.Content]} villagers\n";
        moodText += $"Unhappy: {moodCounts[VillagerMood.MoodState.Unhappy]} villagers\n";

        moodStatsDisplay.text = moodText;
    }

    private void UpdateGoalsDisplay()
    {
        if (goalStatsDisplay == null || VillagerManager.Instance == null) return;

        var villagers = VillagerManager.Instance.GetVillagers();

        // Reset goal counters
        foreach (GoalType type in System.Enum.GetValues(typeof(GoalType)))
        {
            activeGoalStats[type] = 0;
        }

        // Count active goals
        int totalActiveGoals = 0;
        Dictionary<GoalType, float> goalProgressStats = new();

        foreach (var villager in villagers)
        {
            var goals = villager.GetComponent<VillagerGoals>();
            if (goals != null)
            {
                var activeGoals = goals.GetActiveGoals();
                foreach (var goal in activeGoals)
                {
                    activeGoalStats[goal.type]++;
                    totalActiveGoals++;

                    // Track progress for each goal type
                    if (!goalProgressStats.ContainsKey(goal.type))
                    {
                        goalProgressStats[goal.type] = 0;
                    }

                    goalProgressStats[goal.type] += goal.ProgressPercentage;
                }
            }
        }

        // Calculate average progress for each goal type
        foreach (var entry in goalProgressStats.Keys.ToList())
        {
            if (activeGoalStats[entry] > 0)
            {
                goalProgressStats[entry] /= activeGoalStats[entry];
            }
        }

        // Build the display text
        string goalText = "Goals:\n";
        goalText += $"Active Goals: {totalActiveGoals}\n\n";

        goalText += "Active Goal Types:\n";
        foreach (var entry in activeGoalStats.OrderByDescending(x => x.Value))
        {
            if (entry.Value > 0)
            {
                goalText += $"{entry.Key}: {entry.Value}";

                // Add progress if available
                if (goalProgressStats.ContainsKey(entry.Key))
                {
                    goalText += $" ({goalProgressStats[entry.Key]:F0}% avg)";
                }

                goalText += "\n";
            }
        }

        goalStatsDisplay.text = goalText;
    }

    private void OnToggleDetailedStats(bool isOn)
    {
        // Update immediately with the new settings
        UpdateStats();
    }

    private void OnVillagerStateChange(VillagerEvents.StateChangeEvent evt)
    {
        // This helps us track state distribution
        var newStateType = evt.NewState;

        // Ensure all state types are tracked
        if (!villagerStateStats.ContainsKey(newStateType))
        {
            villagerStateStats[newStateType] = 0;
        }

        // Find and decrement the old state count if possible
        foreach (var stateType in villagerStateStats.Keys.ToList())
        {
            if (stateType != newStateType)
            {
                // We don't know which is the old state, so we take a conservative approach
                // of only decrementing if the state count is positive
                if (villagerStateStats[stateType] > 0)
                {
                    villagerStateStats[stateType]--;
                    break; // Only decrement one state
                }
            }
        }

        // Increment the new state
        villagerStateStats[newStateType]++;
    }

    private void OnVillagerInitialized(VillagerEvents.VillagerInitializedEvent evt)
    {
        // Increment villager count
        villagerCount++;

        // Find the villager and get its mood
        var villagerObj = GameObject.Find(evt.VillagerObject.name);
        if (villagerObj != null)
        {
            var moodComponent = villagerObj.GetComponent<VillagerMood>();
            if (moodComponent != null)
            {
                // Update happiness total
                float happiness = moodComponent.Happiness;
                totalHappiness += happiness;

                // Update extremes
                UpdateHappinessExtremes(evt.VillagerName, happiness);

                // Count mood
                moodCounts[moodComponent.CurrentMood]++;

                // Recalculate average
                averageHappiness = totalHappiness / villagerCount;
            }
        }
    }

    private void OnVillagerMoodChanged(VillagerEvents.MoodChangedEvent evt)
    {
        // Update mood counts
        moodCounts[evt.OldMood]--;
        moodCounts[evt.NewMood]++;

        // Update happiness extremes
        UpdateHappinessExtremes(evt.VillagerName, evt.HappinessValue);
    }

    private void UpdateHappinessExtremes(string villagerName, float happiness)
    {
        // Track happiness stats
        if (happiness > maxHappiness)
        {
            maxHappiness = happiness;
            happyVillager = villagerName;
        }

        if (happiness < minHappiness)
        {
            minHappiness = happiness;
            unhappyVillager = villagerName;
        }
    }

    public void RecordGoalCompletion(GoalType goalType)
    {
        // Track completed goals
        if (!completedGoalStats.ContainsKey(goalType))
        {
            completedGoalStats[goalType] = 0;
        }

        completedGoalStats[goalType]++;
    }

    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(!statsPanel.activeSelf);

            if (statsPanel.activeSelf)
            {
                // Update immediately when panel is opened
                UpdateStats();
            }
        }
    }
}