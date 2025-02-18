using UnityEngine;

public class RestNeed : Need
{
    public override string Name => "Rest";

    public override float DecayRate => 0.05f;

    public override ResourceType RequiredResource => ResourceType.None;
    public override float ResourceAmountNeeded => 0f;

    public RestNeed(VillagerServices services)
        : base(services) { }

    public override void Fulfill(ResourceManager resourceManager)
    {
        CurrentValue = 100f; // Rest doesn't consume resources

        EventManager.Instance.TriggerEvent(new VillagerEvents.NeedFulfilledEvent
        {
            VillagerName = services.VillagerComponent.villagerName,
            NeedType = Name,
            NewValue = CurrentValue
        });
    }
}
