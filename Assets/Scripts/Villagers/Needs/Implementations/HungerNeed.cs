using UnityEngine;

public class HungerNeed : Need
{
    public override string Name => "Hunger";

    public override float DecayRate => 0.1f;

    public override ResourceType RequiredResource => ResourceType.Food;
    public override float ResourceAmountNeeded => 10f;

    public HungerNeed(VillagerServices services) : base(services) { }

}
