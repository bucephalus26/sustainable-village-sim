using System.Collections.Generic;
using System.Linq;
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
            CheckGoalStatus();
        }
    }

    private void AssignRandomGoals()
    {
        activeGoals.Clear();

        VillagerPersonality personality = GetComponent<VillagerPersonality>();

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
        if (activeGoals.Count >= maxActiveGoals) return;

        // Create goal based on type
        Goal newGoal = new() { type = goalType, progress = 0f };
        VillagerPersonality personality = GetComponent<VillagerPersonality>();
        float ambitionFactor = (personality != null) ? (1f + (personality.ambition - 0.5f)) : 1f;
        float sociabilityFactor = (personality != null) ? (1f + (personality.sociability - 0.5f)) : 1f;
        float altruismFactor = (personality != null) ? (1f + (personality.altruism - 0.5f)) : 1f;

        // Set target based on goal type, personality
        switch (goalType)
        {
            case GoalType.AccumulateWealth:  // influenced by ambition
                newGoal.target = Mathf.Max(50f, (100f + Random.Range(50f, 150f)) * ambitionFactor);
                newGoal.description = $"Accumulate {newGoal.target:F0} wealth";
                break;

            case GoalType.SocialProminence: // influenced by blend of sociability and ambition
                float baseSocialTarget = 10f + Random.Range(5f, 15f);
                newGoal.target = Mathf.Max(5f, baseSocialTarget * Mathf.Lerp(sociabilityFactor, ambitionFactor, 0.3f));
                newGoal.description = $"Interact with {Mathf.RoundToInt(newGoal.target)} different villagers";
                break;

            case GoalType.WorkMastery:
                newGoal.target = 100f;
                newGoal.description = "Master your profession";
                break;

            case GoalType.VillageContributor: // influenced by blend of sociability and ambition
                float baseContribTarget = 200f + Random.Range(100f, 300f);
                newGoal.target = Mathf.Max(100f, baseContribTarget * Mathf.Lerp(altruismFactor, ambitionFactor, 0.4f));
                newGoal.description = $"Contribute {newGoal.target:F0} resources value";
                break;
        }

        activeGoals.Add(newGoal);
    }

    public void CheckGoalStatus()
    {
        if (villager == null) return;

        // Check active goals
        for (int i = activeGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = activeGoals[i];
            if (goal.completed) continue; // Skip already completed

            // Update progress based on goal type
            switch (goal.type)
            {
                case GoalType.AccumulateWealth:
                    goal.progress = villager.personalWealth;
                    break;

                case GoalType.SocialProminence:
                    if (brain.CurrentState is SocializingState)
                    {
                        UpdateGoalProgress(GoalType.SocialProminence, 0.05f);
                    }
                    break;

                case GoalType.WorkMastery:
                case GoalType.VillageContributor:
                    break;
            }
        }
        CheckAllGoalCompletions();
    }

    private void CheckAllGoalCompletions()
    {
        List<Goal> justCompleted = new();
        for (int i = activeGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = activeGoals[i];
            CheckGoalCompletion(goal); // Ensure completion status is up-to-date
            if (goal.completed)
            {
                justCompleted.Add(goal);
                activeGoals.RemoveAt(i);
            }
        }

        if (justCompleted.Count > 0)
        {
            completedGoals.AddRange(justCompleted);
            // Assign a new goal only
            Invoke("AssignNewGoal", Random.Range(15f, 45f));
        }
    }

    private void CheckGoalCompletion(Goal goal)
    {
        if (goal.completed) return;

        if (goal.progress >= goal.target)
        {
            goal.completed = true;
            if (mood != null)
            {
                float happinessBoost = Random.Range(20f, 30f); // Completion boost
                mood.AddHappinessBoost(happinessBoost, 30f);
                Debug.Log($"{villager.villagerName} completed goal: {goal.description} (+{happinessBoost} happiness)");
            }
        }
    }

    // Goal modified by profession
    public void UpdateGoalProgress(GoalType typeToUpdate, float progressToAdd)
    {
        Goal goal = activeGoals.FirstOrDefault(g => g.type == typeToUpdate && !g.completed);
        if (goal != null)
        {
            float oldProgress = goal.progress;
            goal.progress = Mathf.Min(goal.target, goal.progress + progressToAdd);

            if (goal.progress > oldProgress + 0.1f)
            {
                Debug.Log($"{villager.villagerName} made progress on {goal.description}: {goal.progress:F1}/{goal.target:F1} (+{progressToAdd:F1})");
                // Small happiness boost for progress
                if (mood != null) mood.AddHappinessBoost(1f, 5f); // Small boost for incremental progress
            }

            CheckGoalCompletion(goal); // Check if goal is now complete
        }
    }

    private void AssignNewGoal()
    {
        if (activeGoals.Count < maxActiveGoals)
        {
            // Get candidate goals (not already active or recently completed)
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
