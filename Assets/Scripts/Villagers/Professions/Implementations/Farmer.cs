using UnityEngine;

public class Farmer : BaseProfession
{
    protected override void PerformWork()
    {
        resources.AddResource(ResourceType.Food, data.resourceOutput);
        villager.EarnWealth(data.wealthGeneration);
    }
}
