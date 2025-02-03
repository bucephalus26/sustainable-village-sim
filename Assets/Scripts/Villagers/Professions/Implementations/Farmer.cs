using UnityEngine;

public class Farmer : BaseProfession
{
    protected override void PerformWork()
    {
        resources.AddFood(data.resourceOutput);
        villager.EarnWealth(data.wealthGeneration);
    }
}
