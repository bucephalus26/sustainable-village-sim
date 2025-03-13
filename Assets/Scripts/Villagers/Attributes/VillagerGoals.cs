using System.Collections.Generic;
using UnityEngine;

public class VillagerGoals : MonoBehaviour
{
    [Header("Goals Configuration")]
    [SerializeField] private int maxActiveGoals = 2;
    [SerializeField] private float progressCheckInterval = 5f; // In seconds

    [Header("Goal Status")]
    [SerializeField] private List<Goal> activeGoals = new();
    [SerializeField] private List<Goal> completedGoals = new();

    // Dependencies
    private Villager villager;
    private VillagerBrain brain;
    private VillagerMood mood;

    // Progress tracking
    private float checkTimer;

    // Available goal types
    private static readonly GoalType[] availableGoalTypes = {
        GoalType.AccumulateWealth,
        GoalType.SocialProminence,
        GoalType.WorkMastery,
        GoalType.VillageContributor
    };

    public void Initialize(Villager villager, VillagerBrain brain)
    {
        this.villager = villager;
        this.brain = brain;
        this.mood = GetComponent<VillagerMood>();

        // Start with 1-2 goals based on personality
        AssignRandomGoals();
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;

        // Check goal progress
        if (checkTimer >= progressCheckInterval)
        {
            checkTimer = 0f;
            UpdateGoalProgress();
        }
    }

    private void AssignRandomGoals()
    {
        activeGoals.Clear();

        var personality = GetComponent<VillagerPersonality>();

        // Determine how many goals this villager should have
        int goalCount = 1;
        if (personality != null && personality.ambition > 0.5f)
        {
            goalCount = 2;
        }

        // Create a prioritized list of goals based on personality
        List<GoalType> personalizedGoals = PrioritizeGoalsByPersonality(personality);

        // Assign top goals
        for (int i = 0; i < goalCount && i < personalizedGoals.Count; i++)
        {
            AddGoal(personalizedGoals[i]);
        }

        Debug.Log($"{villager.villagerName} has been assigned {goalCount} goals: {string.Join(", ", activeGoals)}");
    }

    private List<GoalType> PrioritizeGoalsByPersonality(VillagerPersonality personality)
    {

        // Score each goal type based on personality match
        Dictionary<GoalType, float> goalScores = new();

        // Wealth goal is influenced by materialism
        goalScores[GoalType.AccumulateWealth] = personality.optimism * 0.3f +
                                               personality.workEthic * 0.7f;

        // Social goal is influenced by sociability
        goalScores[GoalType.SocialProminence] = personality.sociability * 0.8f +
                                               personality.optimism * 0.2f;

        // Work mastery is influenced by work ethic
        goalScores[GoalType.WorkMastery] = personality.workEthic * 0.8f +
                                         personality.resilience * 0.2f;

        // Village contribution is influenced by altruism
        goalScores[GoalType.VillageContributor] = personality.altruism * 0.6f +
                                                personality.workEthic * 0.4f;

        // Sort goals by score
        List<GoalType> sortedGoals = new(goalScores.Keys);
        sortedGoals.Sort((a, b) => goalScores[b].CompareTo(goalScores[a]));

        return sortedGoals;
    }

    public void AddGoal(GoalType goalType)
    {
        if (activeGoals.Count >= maxActiveGoals)
        {
            Debug.LogWarning($"{villager.villagerName} already has maximum goals");
            return;
        }

        // Create the goal with parameters based on type
        Goal newGoal = new()
        {
            type = goalType,
            progress = 0f
        };

        // Set target based on goal type
        switch (goalType)
        {
            case GoalType.AccumulateWealth:
                newGoal.target = 100f + Random.Range(50f, 150f);
                newGoal.description = $"Accumulate {newGoal.target} wealth";
                break;

            case GoalType.SocialProminence:
                newGoal.target = 10f + Random.Range(5f, 15f);
                newGoal.description = $"Interact with {Mathf.RoundToInt(newGoal.target)} different villagers";
                break;

            case GoalType.WorkMastery:
                newGoal.target = 100f;
                newGoal.description = "Master your profession";
                break;

            case GoalType.VillageContributor:
                newGoal.target = 200f + Random.Range(100f, 300f);
                newGoal.description = $"Contribute {newGoal.target} resources to the village";
                break;
        }

        activeGoals.Add(newGoal);
    }

    private void UpdateGoalProgress()
    {
        if (villager == null) return;

        // Track which goals were updated
        bool anyProgressMade = false;

        // Check each active goal
        for (int i = activeGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = activeGoals[i];
            float oldProgress = goal.progress;

            // Update progress based on goal type
            switch (goal.type)
            {
                case GoalType.AccumulateWealth:
                    goal.progress = villager.personalWealth;
                    break;

                case GoalType.SocialProminence:
                    // For now, just gradual progress when socializing
                    if (brain.CurrentState is SocializingState)
                    {
                        goal.progress += 0.1f; // Very simple implementation
                    }
                    break;

                case GoalType.WorkMastery:
                    // Progress based on time spent working efficiently
                    if (brain.CurrentState is WorkingState)
                    {
                        // This is placeholder - a real implementation would track work accomplishments
                        goal.progress += 0.5f;
                    }
                    break;

                case GoalType.VillageContributor:
                    // Progress based on resources contributed through work
                    if (brain.CurrentState is WorkingState)
                    {
                        // Simple placeholder implementation
                        goal.progress += 1f;
                    }
                    break;
            }

            // Check if meaningful progress was made
            if (goal.progress > oldProgress + 0.5f)
            {
                anyProgressMade = true;
                Debug.Log($"{villager.villagerName} made progress on {goal.description}: {goal.progress:F1}/{goal.target:F1}");
            }

            // Check for completion
            if (goal.progress >= goal.target && !goal.completed)
            {
                goal.completed = true;

                // Apply happiness boost for goal completion
                if (mood != null)
                {
                    float happinessBoost = Random.Range(20f, 30f);
                    mood.AddHappinessBoost(happinessBoost, 30f); // Give a temporary boost

                    Debug.Log($"{villager.villagerName} completed goal: {goal.description} (+{happinessBoost} happiness)");
                }

                // Move to completed goals
                completedGoals.Add(goal);
                activeGoals.RemoveAt(i);

                // Assign a new goal after delay
                Invoke("AssignNewGoal", Random.Range(30f, 60f));
            }
        }

        // If progress was made, apply small happiness boost
        if (anyProgressMade && mood != null)
        {
            mood.AddHappinessBoost(3f, 10f);
        }
    }

    private void AssignNewGoal()
    {
        if (activeGoals.Count < maxActiveGoals)
        {
            // Get candidate goals (ones not already active or recently completed)
            List<GoalType> candidates = new();
            foreach (GoalType goalType in availableGoalTypes)
            {
                bool alreadyActive = activeGoals.Exists(g => g.type == goalType);
                bool recentlyCompleted = false;

                // Check if this goal type was recently completed
                foreach (Goal completed in completedGoals)
                {
                    if (completed.type == goalType && completed.completed)
                    {
                        recentlyCompleted = true;
                        break;
                    }
                }

                if (!alreadyActive && !recentlyCompleted)
                {
                    candidates.Add(goalType);
                }
            }

            // If we have candidates, add a random one
            if (candidates.Count > 0)
            {
                GoalType newType = candidates[Random.Range(0, candidates.Count)];
                AddGoal(newType);
            }
        }
    }

    public float GetGoalPreference(IVillagerState stateType)
    {
        float preference = 0f;

        foreach (Goal goal in activeGoals)
        {
            switch (goal.type)
            {
                case GoalType.AccumulateWealth:
                    if (stateType is WorkingState) preference += 2f;
                    break;

                case GoalType.SocialProminence:
                    if (stateType is SocializingState) preference += 2f;
                    break;

                case GoalType.WorkMastery:
                    if (stateType is WorkingState) preference += 3f;
                    break;

                case GoalType.VillageContributor:
                    if (stateType is WorkingState) preference += 2f;
                    break;
            }
        }

        return preference;
    }

    public bool HasGoalOfType(GoalType type)
    {
        return activeGoals.Exists(g => g.type == type);
    }

    public List<Goal> GetActiveGoals()
    {
        return activeGoals;
    }

}
