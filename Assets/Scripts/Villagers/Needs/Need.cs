using UnityEngine;

public abstract class Need
{
    public abstract string Name { get; }
    public float CurrentValue { get; protected set; } = 20.5f;
    public float CriticalThreshold { get; protected set; } = 20f;
    public abstract float DecayRate { get; }
    public abstract ResourceType RequiredResource { get; }
    public abstract float ResourceAmountNeeded { get; }

    protected VillagerServices services;

    protected Need(VillagerServices services)
    {
        this.services = services;
    }

    public virtual void Decay(float deltaTime)
    {
        float previousValue = CurrentValue;
        CurrentValue = Mathf.Max(0, CurrentValue - DecayRate * deltaTime);

        if (!IsCritical(previousValue) && IsCritical(CurrentValue))
        {
            EventManager.Instance.TriggerEvent(new VillagerEvents.NeedBecameCriticalEvent
            {
               VillagerName = services.VillagerComponent.villagerName,
               NeedType = Name,
               CurrentValue = CurrentValue
            });
        }
    }

    public virtual void Fulfill(ResourceManager resourceManager)
    {
        if (resourceManager.ConsumeResource(RequiredResource, ResourceAmountNeeded))
        {
            CurrentValue = 100f;
            EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfilledEvent
            {
                VillagerName = services.VillagerComponent.villagerName,
                NeedType = Name,
                NewValue = CurrentValue
            });
        }
        else
        {
            EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfillmentFailedEvent
            {
                VillagerName = services.VillagerComponent.villagerName,
                NeedType = Name,
                RequiredResource = RequiredResource,
                AmountNeeded = ResourceAmountNeeded
            });
        }
    }

    public virtual bool IsCritical() => CurrentValue <= CriticalThreshold;
    private bool IsCritical(float value) => value <= CriticalThreshold;

}
