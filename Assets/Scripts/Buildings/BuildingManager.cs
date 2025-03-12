using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private Dictionary<BuildingType, List<Building>> buildingsByType = new();
    private Dictionary<string, Building> buildingsByName = new();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeBuildingDictionaries();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeBuildingDictionaries()
    {
        foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            buildingsByType[type] = new List<Building>();
        }
    }

    public void RegisterBuilding(Building building)
    {
        if (!buildingsByType.ContainsKey(building.BuildingType))
        {
            buildingsByType[building.BuildingType] = new List<Building>();
        }

        buildingsByType[building.BuildingType].Add(building);
        buildingsByName[building.BuildingName] = building;

        EventManager.Instance.TriggerEvent(new BuildingEvents.BuildingRegisteredEvent
        {
            BuildingName = building.BuildingName,
            BuildingType = building.BuildingType
        });
    }

    public List<Building> GetBuildingsByType(BuildingType type)
    {
        if (buildingsByType.TryGetValue(type, out List<Building> buildings))
        {
            return buildings;
        }
        return new List<Building>();
    }

    public Building GetRandomBuildingByType(BuildingType type)
    {
        var buildings = GetBuildingsByType(type);
        if (buildings.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, buildings.Count);
            return buildings[randomIndex];
        }
        return null;
    }

    public Building GetNearestBuildingByType(BuildingType type, Vector3 position)
    {
        var buildings = GetBuildingsByType(type);

        Building nearest = null;
        float minDistance = float.MaxValue;

        foreach (var building in buildings)
        {
            float distance = Vector3.Distance(position, building.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = building;
            }
        }

        return nearest;
    }

    // Method to update all buildings when the time changes - Once TimeManager is implemented
    public void UpdateAllBuildings()
    {
        foreach (var buildingList in buildingsByType.Values)
        {
            foreach (var building in buildingList)
            {
                building.OnTimeUpdate();
            }
        }
    }
}
