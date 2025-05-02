using System;
using UnityEngine;

[Serializable]
public abstract class Need
{
    [SerializeField] private string name;
    [SerializeField] private float criticalThreshold = 20f;
    [SerializeField] private float baseDecayRate = 1.0f; // Per game hour
    [SerializeField] private float importanceWeight = 1.0f;
    [SerializeField] protected ResourceType requiredResource = ResourceType.None;
    [SerializeField] protected float resourceAmountNeeded = 1.0f;

    public string Name => name;
    public float CurrentValue { get; protected set; } = 100f;
    public float CriticalThreshold => criticalThreshold;
    public float ImportanceWeight => importanceWeight;
    public float DecayRate => baseDecayRate;
    public ResourceType RequiredResource => requiredResource;
    public float ResourceAmountNeeded => resourceAmountNeeded;

    // Tracking for diminishing returns
    private float lastFulfillmentTime = 0f;
    private int fulfillmentCount = 0;
    private const float fulfillmentMemoryDuration = 3f; // In game hours
    private const float diminishingReturnsFactor = 0.7f; // Each subsequent fulfillment is 70% as effective

    protected Villager Villager { get; private set; }
    protected EconomyManager EconomyManager => EconomyManager.Instance;

    protected Need(string name, Villager villager, float importanceWeight = 1.0f,
                   ResourceType requiredResource = ResourceType.None, float resourceAmountNeeded = 1.0f)
    {
        this.name = name;
        this.Villager = villager;
        this.importanceWeight = importanceWeight;
        this.requiredResource = requiredResource;
        this.resourceAmountNeeded = resourceAmountNeeded;
        CurrentValue = 100f;
    }


    public virtual void Decay(float deltaTime)
    {
        // Scale decay rate with time scale
        float timeScaledDecay = baseDecayRate * deltaTime;

        // Apply personality modifiers
        float personalityModifier = 1.0f;
        var personality = Villager.GetComponent<VillagerPersonality>();
        if (personality != null)
        {
            personalityModifier = personality.GetNeedDecayMultiplier(this);
        }

        // Store previous value to check if we crossed threshold
        float previousValue = CurrentValue;

        // Apply decay
        CurrentValue = Mathf.Max(0f, CurrentValue - (timeScaledDecay * personalityModifier));

        // Check if need became critical
        if (!IsCritical(previousValue) && IsCritical(CurrentValue))
        {
            EventManager.Instance.TriggerEvent(new VillagerEvents.NeedBecameCriticalEvent
            {
                VillagerName = Villager.villagerName,
                NeedType = Name,
                CurrentValue = CurrentValue
            });
        }
    }

    public virtual bool Fulfill(float fulfillmentAmount)
    {
        // Check if we need resources and have enough
        if (RequiredResource != ResourceType.None)
        {
            float resourceCost = ResourceAmountNeeded * EconomyManager.Instance.GetResourcePrice(RequiredResource);

            if (Villager.personalWealth >= resourceCost) // Check if villager can afford it
            {
                Villager.SpendWealth(resourceCost);

                if (!EconomyManager.ConsumeResource(RequiredResource, ResourceAmountNeeded))
                {
                    Debug.LogWarning($"{Villager.villagerName} could afford {RequiredResource}, but village supply is empty!");

                    EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfillmentFailedEvent
                    {
                        VillagerName = Villager.villagerName,
                        NeedType = Name,
                        RequiredResource = RequiredResource,
                        AmountNeeded = ResourceAmountNeeded
                    });
                    return false;
                }
                Debug.Log($"Successfully consumed {ResourceAmountNeeded} of {RequiredResource} for {Villager.villagerName}");
            }
            else
            {
                Debug.LogWarning($"{Villager.villagerName} cannot afford {ResourceAmountNeeded} {RequiredResource}. Needs {resourceCost:F1} wealth, has {Villager.personalWealth:F1}.");
                EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfillmentFailedEvent
                {
                    VillagerName = Villager.villagerName,
                    NeedType = Name,
                    RequiredResource = RequiredResource,
                    AmountNeeded = ResourceAmountNeeded,
                    Reason = "Insufficient personal wealth"
                });
                return false;
            }
        }

        // Apply diminishing returns if fulfillment is frequent
        float gameTimeNow = Time.time * TimeManager.Instance.TimeScaleFactor;
        float gameHoursSinceLastFulfillment = gameTimeNow - lastFulfillmentTime;

        if (gameHoursSinceLastFulfillment < fulfillmentMemoryDuration)
        {
            // Increase count and reduce effectiveness
            fulfillmentCount++;
            fulfillmentAmount *= Mathf.Pow(diminishingReturnsFactor, fulfillmentCount - 1);
        }
        else
        {
            // Reset count if it's been a while
            fulfillmentCount = 1;
        }
        lastFulfillmentTime = gameTimeNow; // Update last fulfillment time

        // Apply the fulfillment
        float previousValue = CurrentValue;
        CurrentValue = Mathf.Clamp(CurrentValue + fulfillmentAmount, 0f, 100f);

        // Notify about fulfillment
        if (CurrentValue > previousValue)
        {
            // Trigger event only if the need was previously critical and now isn't
            if (IsCritical(previousValue) && !IsCritical(CurrentValue))
            {
                EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfilledEvent
                {
                    VillagerName = Villager.villagerName,
                    NeedType = Name,
                    NewValue = CurrentValue
                });
            }
        }
        return true;
    }

    public virtual void FulfillGradually(float deltaTime, float ratePerGameHour = 10f)
    {
        // Scale by time
        float timeScaledFulfillment = ratePerGameHour * deltaTime * TimeManager.Instance.TimeScaleFactor;

        // Store previous value to check if we crossed threshold
        float previousValue = CurrentValue;

        // Apply fulfillment
        CurrentValue = Mathf.Min(100f, CurrentValue + timeScaledFulfillment);

        // Check if need became non-critical
        if (IsCritical(previousValue) && !IsCritical(CurrentValue))
        {
            EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfilledEvent
            {
                VillagerName = Villager.villagerName,
                NeedType = Name,
                NewValue = CurrentValue
            });
        }
    }

    public virtual float GetUrgency()
    {
        // Calculate urgency: lower value = more urgent
        // Combine the current value with importance weighting
        float normalizedValue = CurrentValue / 100f;
        float urgency = (1f - normalizedValue) * importanceWeight;

        // Increase urgency dramatically if below critical threshold
        if (CurrentValue <= criticalThreshold)
        {
            urgency *= 2f;
        }

        return urgency;
    }

    public virtual bool IsCritical() => CurrentValue <= criticalThreshold;
    protected bool IsCritical(float value) => value <= criticalThreshold;

}
