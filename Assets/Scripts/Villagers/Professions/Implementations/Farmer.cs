using UnityEngine;

public class Farmer : BaseProfession
{
    protected override void DoWork()
    {
        resources.AddResource(ResourceType.Food, data.resourceOutput);
        villager.EarnWealth(data.wealthGeneration);
    }
}
