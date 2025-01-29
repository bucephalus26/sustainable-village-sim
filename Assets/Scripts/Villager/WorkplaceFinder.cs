using UnityEngine;

public class WorkplaceFinder : MonoBehaviour
{
    private const string FARM_TAG = "Farm";
    private const string SHOP_TAG = "Shop";
    private const string HOME_TAG = "Home";

    public Transform FindWorkplace(Villager.ProfessionType profession)
    {
        switch (profession)
        {
            case Villager.ProfessionType.Farmer:
                return GameObject.FindGameObjectWithTag(FARM_TAG)?.transform;
            case Villager.ProfessionType.Shopkeeper:
                return GameObject.FindGameObjectWithTag(SHOP_TAG)?.transform;
            default:
                return GameObject.FindGameObjectWithTag(HOME_TAG)?.transform;
        }
    }

}
