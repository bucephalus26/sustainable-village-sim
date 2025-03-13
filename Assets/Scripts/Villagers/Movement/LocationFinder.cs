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

    public Transform GetLeisureLocation()
    {
        // Pick appropriate leisure location based on personality if available
        VillagerPersonality personality = GetComponent<VillagerPersonality>();

        if (personality != null && personality.sociability > 0.7f)
        {
            // More social villagers prefer taverns
            Building tavern = BuildingManager.Instance.GetRandomBuildingByType(BuildingType.Tavern);
            if (tavern != null) return tavern.GetEntrancePoint();
        }
        else if (personality != null && personality.sociability < 0.3f)
        {
            // Less social villagers prefer smaller gatherings like church
            Building church = BuildingManager.Instance.GetRandomBuildingByType(BuildingType.Church);
            if (church != null) return church.GetEntrancePoint();
        }

        // Randomly select between tavern or church as fallback
        BuildingType[] leisureTypes = { BuildingType.Tavern, BuildingType.Church };
        BuildingType selectedType = leisureTypes[UnityEngine.Random.Range(0, leisureTypes.Length)];

        Building leisureBuilding = BuildingManager.Instance.GetRandomBuildingByType(selectedType);

        return leisureBuilding?.GetEntrancePoint() ?? transform;
    }

    // Method for getting a random position nearby for idle wandering
    public Vector3 GetRandomNearbyPosition(float radius = 3f)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * radius;
        return new Vector3(transform.position.x + randomDirection.x, transform.position.y + randomDirection.y, transform.position.z);
    }
}
