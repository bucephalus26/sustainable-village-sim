using UnityEngine;

public class Priest : BaseProfession
{
    protected override void PerformWork()
    {
        // Implement priest-specific work logic here (e.g., increase happiness)
        Debug.Log($"{villager.villagerName} is leading a prayer.");
    }

}