using UnityEngine;

public class Shopkeeper : BaseProfession
{
    protected override void DoWork()
    {
        resources.AddResource(ResourceType.Wealth, data.wealthGeneration);
        villager.EarnWealth(data.wealthGeneration);
    }
}