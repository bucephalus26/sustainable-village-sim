using UnityEngine;

public class HungerNeed : Need
{
    public HungerNeed(Villager villager)
        : base(
            name: "Hunger",
            villager: villager,
            importanceWeight: 1.5f,
            requiredResource: ResourceType.Food,
            resourceAmountNeeded: 1.0f)
    {
        // Start slightly hungry
        CurrentValue = 80f;
    }
}