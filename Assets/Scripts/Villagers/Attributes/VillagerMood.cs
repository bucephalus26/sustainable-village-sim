using System.Collections.Generic;
using UnityEngine;

public class VillagerMood : MonoBehaviour
{
    [Header("Happiness Settings")]
    [SerializeField] [Range(0f, 100f)] private float happiness = 50f;
    [SerializeField] private string currentMood;
    [SerializeField] private bool showMoodIndicator = true;

    [Header("Factor Weights")]
    [SerializeField] [Range(0f, 2f)] private float needsWeight = 1.0f;
    [SerializeField] [Range(0f, 2f)] private float wealthWeight = 0.3f;
    [SerializeField] [Range(0f, 2f)] private float workWeight = 0.5f;
    [SerializeField] [Range(0f, 2f)] private float goalWeight = 0.7f;

    [Header("Temporary Boosts")]
    [SerializeField] private float temporaryBoost = 0f;
    [SerializeField] private float boostRemainingTime = 0f;

    [Header("Indicator Settings")]
    [SerializeField] private float indicatorSize = 3.5f;
    [SerializeField] private int indicatorSortingOrder = 1;

    [Header("Debug Info")]
    [SerializeField] private float needsSatisfaction;
    [SerializeField] private float wealthSatisfaction;
    [SerializeField] private float workSatisfaction;

    // Dependencies
    private Villager villager;
    private VillagerBrain brain;
    private SpriteRenderer moodIndicator;

    private List<float> happinessHistory = new();
    private const int maxHistoryLength = 10;

    // Properties
    public float Happiness => happiness;
    public enum MoodState { Unhappy, Content, Happy }
    public MoodState CurrentMood
    {
        get
        {
            if (happiness < 30f) return MoodState.Unhappy;
            if (happiness > 70f) return MoodState.Happy;
            return MoodState.Content;
        }
    }
    private MoodState previousMood;

    public float HappinessTrend
    {
        get
        {
            if (happinessHistory.Count < 2) return 0f;

            float sum = 0f;
            for (int i = 1; i < happinessHistory.Count; i++)
            {
                sum += happinessHistory[i] - happinessHistory[i - 1];
            }

            return sum / (happinessHistory.Count - 1);
        }
    }

    public void Initialize(Villager villager, VillagerBrain brain)
    {
        this.villager = villager;
        this.brain = brain;

        if (showMoodIndicator)
        {
            CreateMoodIndicator();
        }
    }

    private void CreateMoodIndicator()
    {
        if (moodIndicator != null) return;

        // Small indicator above the villager
        GameObject indicatorObj = new("MoodIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = new Vector3(0, 0.7f, 0);

        moodIndicator = indicatorObj.AddComponent<SpriteRenderer>();
        moodIndicator.sprite = CreateCircleSprite();

        moodIndicator.transform.localScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);
        moodIndicator.sortingOrder = indicatorSortingOrder;

        UpdateMoodIndicator();
    }

    private void DestroyMoodIndicator()
    {
        if (moodIndicator != null)
        {
            Destroy(moodIndicator.gameObject);
            moodIndicator = null;
        }
    }

    public void UpdateHappiness()
    {
        if (brain?.NeedsManager == null) return;

        previousMood = CurrentMood;

        // Calculate satisfaction factors
        needsSatisfaction = CalculateNeedsSatisfaction();
        wealthSatisfaction = CalculateWealthSatisfaction();
        workSatisfaction = CalculateWorkSatisfaction();

        // Goal progress satisfaction
        float goalSatisfaction = CalculateGoalSatisfaction();

        // Calculate weighted contribution
        float totalWeight = needsWeight + wealthWeight + workWeight + goalWeight; // Add goalWeight
        float weightedSatisfaction = (
            needsSatisfaction * needsWeight +
            wealthSatisfaction * wealthWeight +
            workSatisfaction * workWeight +
            goalSatisfaction * goalWeight // Add goal contribution
        ) / totalWeight;

        // Apply temporary boosts
        if (boostRemainingTime > 0)
        {
            boostRemainingTime -= Time.deltaTime;
            weightedSatisfaction += temporaryBoost;

            if (boostRemainingTime <= 0)
            {
                temporaryBoost = 0f;
            }
        }

        // Personality modifier
        float personalityModifier = 0f;
        if (brain.Personality != null)
        {
            // Optimistic people are generally happier
            personalityModifier = (brain.Personality.optimism - 0.5f) * 20f;
        }

        // Calculate target happiness
        float targetHappiness = weightedSatisfaction + personalityModifier;
        targetHappiness = Mathf.Clamp(targetHappiness, 0f, 100f);

        // Gradually move toward target
        happiness = Mathf.Lerp(happiness, targetHappiness, Time.deltaTime * 0.1f);

        currentMood = CurrentMood.ToString();

        if (previousMood != CurrentMood)
        {
            // Trigger event for mood change
            EventManager.Instance.TriggerEvent(new VillagerEvents.MoodChangedEvent
            {
                VillagerName = villager.villagerName,
                OldMood = previousMood,
                NewMood = CurrentMood,
                HappinessValue = happiness
            });
        }

        // Update visual
        if (showMoodIndicator)
        {
            if (moodIndicator == null)
                CreateMoodIndicator();
            UpdateMoodIndicator();
        }
        else
        {
            DestroyMoodIndicator();
        }


        // Update villager's happiness value
        villager.happiness = happiness;
    }

    // Calculate satisfaction affecting overall happiness
    private float CalculateNeedsSatisfaction()
    {
        float sum = 0f;
        int count = 0;

        foreach (var need in brain.NeedsManager.GetAllNeeds())
        {
            sum += need.CurrentValue;
            count++;
        }

        return count > 0 ? sum / count : 50f;
    }

    private float CalculateWealthSatisfaction()
    {
        // Simple logarithmic function for diminishing returns
        return Mathf.Min(100f, Mathf.Log10(villager.personalWealth + 1) * 20f);
    }

    private float CalculateWorkSatisfaction()
    {
        // Simple work satisfaction
        if (brain.Profession == null ||
            brain.Profession.ProfessionType == ProfessionType.Unemployed)
        {
            return 40f; // Slight dissatisfaction from unemployment
        }

        // Has job, influenced by personality
        float baseValue = 60f;
        if (brain.Personality != null)
        {
            // Work ethic influences job satisfaction
            if (brain.Personality.workEthic > 0.7f)
            {
                baseValue += 15f;
            }
            else if (brain.Personality.workEthic < 0.3f)
            {
                baseValue -= 10f;
            }
        }

        return Mathf.Clamp(baseValue, 0f, 100f);
    }

    private float CalculateGoalSatisfaction()
    {
        VillagerGoals goals = GetComponent<VillagerGoals>();
        if (goals == null) return 50f; // Neutral if no goals component

        // Get all active goals
        var activeGoals = goals.GetActiveGoals();
        if (activeGoals.Count == 0) return 50f; // Neutral if no active goals

        // Calculate average progress
        float totalProgress = 0f;
        foreach (var goal in activeGoals)
        {
            totalProgress += goal.ProgressPercentage;
        }

        // Return average progress as satisfaction level
        return totalProgress / activeGoals.Count;
    }

    // Add this method to handle happiness boosts from completing goals
    public void AddHappinessBoost(float boostAmount, float durationSeconds)
    {
        temporaryBoost = boostAmount;
        boostRemainingTime = durationSeconds;

        Debug.Log($"{villager.villagerName} got a happiness boost of {boostAmount} for {durationSeconds} seconds");
    }


    // Work efficiency multiplier for profession system
    public float GetWorkEfficiencyMultiplier()
    {
        return Mathf.Lerp(0.5f, 1.5f, happiness / 100f);
    }

    // Social interaction quality for future social system
    public float GetSocialInteractionQuality()
    {
        return Mathf.Lerp(0.7f, 1.3f, happiness / 100f);
    }

    private void UpdateMoodIndicator()
    {
        if (moodIndicator == null) return;

        // Set color based on mood
        switch (CurrentMood)
        {
            case MoodState.Happy:
                moodIndicator.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
                break;
            case MoodState.Content:
                moodIndicator.color = new Color(0.8f, 0.8f, 0.2f, 0.7f);
                break;
            case MoodState.Unhappy:
                moodIndicator.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
                break;
        }
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple dot texture
        int resolution = 32;
        Texture2D texture = new(resolution, resolution);

        float centerX = resolution / 2;
        float centerY = resolution / 2;
        float radius = resolution / 2 - 2;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                texture.SetPixel(x, y, distance < radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
}
