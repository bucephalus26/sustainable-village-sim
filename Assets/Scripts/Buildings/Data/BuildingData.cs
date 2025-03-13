using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Village/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Information")]
    public string buildingName;
    public BuildingType type;
    public string description;

    [Header("Capacity Settings")]
    public int maxOccupants = 5;
    public int maxWorkers = 2;
    public int maxResidents = 4; // Only used for homes

    [Header("Production Settings")]
    public bool isWorkplace = false;
    public List<ProfessionType> validProfessions = new();
    public ResourceType producedResourceType = ResourceType.None;
    public float productionInterval = 1.0f;
    public float baseResourceProduction = 2.0f;
    public float baseWealthProduction = 1.0f;

    [Header("Need Fulfillment")]
    public List<NeedFulfillmentData> needsFulfilled = new();
}