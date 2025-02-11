using UnityEngine;

public class Shopkeeper : BaseProfession
{
    protected override void PerformWork()
    {
        resources.AddResource(ResourceType.CommunalWealth, data.wealthGeneration);
        villager.EarnWealth(data.wealthGeneration);
    }
}