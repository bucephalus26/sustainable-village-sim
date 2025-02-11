using System.Collections.Generic;
using UnityEngine;

public class WorkplaceFinder : MonoBehaviour
{
    private Dictionary<string, Transform> locations;
    private Dictionary<string, string> professionToLocation;

    void Awake()
    {
        InitializeLocations();
        InitializeProfessionMappings();
    }

    private void InitializeLocations()
    {
        locations = new Dictionary<string, Transform>
        {
            ["Home"] = GameObject.FindGameObjectWithTag("Home")?.transform,
            ["Farm"] = GameObject.FindGameObjectWithTag("Farm")?.transform,
            ["Shop"] = GameObject.FindGameObjectWithTag("Shop")?.transform,
            ["Restaurant"] = GameObject.FindGameObjectWithTag("Restaurant")?.transform
        };
    }

    private void InitializeProfessionMappings()
    {
        professionToLocation = new Dictionary<string, string>
        {
            ["Farmer"] = "Farm",
            ["Shopkeeper"] = "Shop",
            ["Priest"] = "Home",
        };
    }

    public Transform GetCurrentWorkplace()
    {
        var professionManager = GetComponent<ProfessionManager>();
        var professionType = professionManager.GetProfessionType().ToString();

        if (professionToLocation.TryGetValue(professionType, out string locationKey))
        {
            return locations[locationKey];
        }

        Debug.LogWarning($"No workplace mapping found for profession: {professionType}. Defaulting to Home.");
        return locations["Home"];
    }

    public Transform GetNeedLocation(Need need)
    {
        return need.Name switch
        {
            "Hunger" => locations["Restaurant"],
            "Rest" => locations["Home"],
            _ => locations["Home"]
        };
    }
}
