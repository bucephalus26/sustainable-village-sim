using UnityEngine;

public class SocialNeed : Need
{
    public SocialNeed(Villager villager)
        : base(
            name: "Social",
            villager: villager,
            importanceWeight: 0.8f)
    {
        // Social doesn't require resources
        // Start with some social need
        CurrentValue = 85f;
    }
}