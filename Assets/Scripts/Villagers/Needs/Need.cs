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
        CurrentValue = Mathf.Max(0, CurrentValue - DecayRate * deltaTime);
    }

    public virtual void Fulfill(ResourceManager resourceManager)
    {
        if (resourceManager.ConsumeResource(RequiredResource, ResourceAmountNeeded))
        {
            CurrentValue = 100f;
            Debug.Log($"Villager {services.VillagerComponent.villagerName} ({services.ProfessionManager.GetProfessionType()}) fulfilled {RequiredResource} need. Need replenished.");
        }
        else
        {
            Debug.Log($"Villager {services.VillagerComponent.villagerName} ({services.ProfessionManager.GetProfessionType()}) tried to fulfill {RequiredResource} need but there weren't enough resources.");
        }
    }

    public virtual bool IsCritical() => CurrentValue <= CriticalThreshold;
}
