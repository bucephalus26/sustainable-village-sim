using UnityEngine;

public class RestNeed : Need
{
    public RestNeed(Villager villager)
        : base(
            name: "Rest",
            villager: villager,
            importanceWeight: 1.2f)
    {
        // Rest doesn't require resources
        // Start a bit tired
        CurrentValue = 90f;
    }
}
