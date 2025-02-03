using UnityEngine;

public class WorkplaceFinder : MonoBehaviour
{
    private Transform farm;
    private Transform shop;
    private Transform home;
    private Transform restaurant;

    void Start()
    {
        farm = GameObject.FindGameObjectWithTag("Farm")?.transform;
        shop = GameObject.FindGameObjectWithTag("Shop")?.transform;
        home = GameObject.FindGameObjectWithTag("Home")?.transform;
        restaurant = GameObject.FindGameObjectWithTag("Restaurant")?.transform;
    }

    public Transform FindWorkplace(ProfessionType profession)
    {
        switch (profession)
        {
            case ProfessionType.Farmer:
                return farm;
            case ProfessionType.Shopkeeper:
                return shop;
            case ProfessionType.Priest:
                return home;
            default:
                return home;
        }
    }

    public Transform FindRestaurant()
    {
        return restaurant;
    }

    public Transform FindHome()
    {
        return home;
    }

}
