using UnityEngine;

public class LocationFinder : MonoBehaviour
{
    private VillagerProfession profession;
    private Villager villager;

    void Awake()
    {
        villager = GetComponent<Villager>();
    }

    private void Start()
    {
        profession = GetComponent<VillagerProfession>();
    }

    public Transform GetCurrentWorkplace()
    {
        BuildingType workplaceType = GetWorkplaceTypeForProfession();

        Building workplace = null;

        // Find workplace where villager is assigned as a worker
        var workplaces = BuildingManager.Instance.GetBuildingsByType(workplaceType);
        foreach (var building in workplaces)
        {
            if (building.HasWorker(villager))
            {
                workplace = building;
                break;
            }
        }

        // If no assigned workplace, get a random one
        if (workplace == null)
        {
            workplace = BuildingManager.Instance.GetRandomBuildingByType(workplaceType);
        }

        return workplace?.GetEntrancePoint() ?? transform;
    }

    private BuildingType GetWorkplaceTypeForProfession()
    {
        return profession.ProfessionType switch
        {
            ProfessionType.Farmer => BuildingType.Farm,
            ProfessionType.Shopkeeper => BuildingType.Shop,
            ProfessionType.Priest => BuildingType.Church,
            ProfessionType.Craftsman => BuildingType.Workshop,
            ProfessionType.Cook => BuildingType.Restaurant,
            _ => BuildingType.Home
        };
    }

    public Transform GetNeedLocation(Need need)
    {
        BuildingType buildingType = need.Name switch
        {
            "Hunger" => BuildingType.Restaurant,
            "Social" => BuildingType.Tavern,
            "Rest" => BuildingType.Home,
            _ => BuildingType.Home
        };

        Building building = null;

        // For homes, try to find the villager's assigned home
        if (buildingType == BuildingType.Home)
        {
            building = GetHomeBuilding();
        }

        // If no specific building was found, get the nearest one of that type
        if (building == null)
        {
            building = BuildingManager.Instance.GetNearestBuildingByType(buildingType, transform.position);
        }

        return building?.GetEntrancePoint() ?? transform;
    }

    public Transform GetHomeLocation()
    {
        Building home = GetHomeBuilding();

        // If no home is assigned, find any available home
        if (home == null)
        {
            home = BuildingManager.Instance.GetRandomBuildingByType(BuildingType.Home);
        }

        return home?.GetEntrancePoint() ?? transform;
    }

    private Building GetHomeBuilding()
    {
        var homes = BuildingManager.Instance.GetBuildingsByType(BuildingType.Home);
        foreach (var building in homes)
        {
            if (building.IsResident(villager))
            {
                return building;
            }
        }
        return null;
    }

    public Building GetLeisureBuilding()
    {
        VillagerPersonality personality = GetComponent<VillagerPersonality>();
        Building leisureBuilding = null;

        BuildingType preferredType;
        if (personality != null)
        {
            if (personality.sociability > 0.7f) preferredType = BuildingType.Tavern;
            else if (personality.sociability < 0.3f) preferredType = BuildingType.Church; // for less social people
            else preferredType = (Random.value < 0.6f) ? BuildingType.Tavern : BuildingType.Church;
        }
        else
        {
            preferredType = (Random.value < 0.6f) ? BuildingType.Tavern : BuildingType.Church;
        }

        leisureBuilding = BuildingManager.Instance.GetRandomBuildingByType(preferredType);

        if (leisureBuilding == null)
        {
            BuildingType fallbackType = (preferredType == BuildingType.Tavern) ? BuildingType.Church : BuildingType.Tavern;
            leisureBuilding = BuildingManager.Instance.GetRandomBuildingByType(fallbackType);
        }

        return leisureBuilding;
    }

    // gets a random position nearby for idle wandering
    public Vector3 GetRandomNearbyPosition(float radius = 3f)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * radius;
        return new Vector3(transform.position.x + randomDirection.x, transform.position.y + randomDirection.y, transform.position.z);
    }

    public Transform GetLeisureLocation()
    {
        Building building = GetLeisureBuilding();
        return building?.GetEntrancePoint() ?? transform;
    }

}
