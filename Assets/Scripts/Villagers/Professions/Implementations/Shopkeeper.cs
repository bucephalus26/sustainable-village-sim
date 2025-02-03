using UnityEngine;

public class Shopkeeper : BaseProfession
{
    protected override void PerformWork()
    {
        resources.AddCommunalWealth(data.wealthGeneration);
        villager.EarnWealth(data.wealthGeneration);
    }
}