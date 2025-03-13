using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building: MonoBehaviour
{
    [Header("Building Configuration")]
    [SerializeField] private BuildingData data;
    [SerializeField] private Transform entrancePoint;

    private IBuildingFunctionality functionality;

    // Tracking occupants, workers, residents (for homes)
    private List<Villager> currentOccupants = new();
    private List<Villager> assignedWorkers = new();
    private List<Villager> residents = new();

    public BuildingType BuildingType => data.type;
    public string BuildingName => data.buildingName;

    private void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        // Make sure we have an entrance point
        if (entrancePoint == null)
            entrancePoint = transform;

        // Create appropriate functionality based on building type
        functionality = data.type switch
        {
            BuildingType.Home => new HomeFunctionality(this),
            BuildingType.Farm => new WorkplaceFunctionality(this),
            BuildingType.Shop => new WorkplaceFunctionality(this),
            BuildingType.Workshop => new WorkplaceFunctionality(this),
            BuildingType.Restaurant => new WorkplaceFunctionality(this),
            BuildingType.Church => new SocialFunctionality(this),
            BuildingType.Tavern => new SocialFunctionality(this),
            _ => new BasicFunctionality(this)
        };

        // Register with building manager
        BuildingManager.Instance.RegisterBuilding(this);
    }

    public bool CanEnter(Villager villager)
    {
        return currentOccupants.Count < data.maxOccupants;
    }

    public virtual void Enter(Villager villager)
    {
        if (CanEnter(villager))
        {
            currentOccupants.Add(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.VillagerEnteredEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName,
                BuildingType = data.type
            });
        }
    }

    public virtual void Exit(Villager villager)
    {
        if (currentOccupants.Contains(villager))
        {
            currentOccupants.Remove(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.VillagerExitedEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName,
                BuildingType = data.type
            });
        }
    }

    public void AssignWorker(Villager villager)
    {
        if (assignedWorkers.Count < data.maxWorkers && !assignedWorkers.Contains(villager))
        {
            assignedWorkers.Add(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.WorkerAssignedEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName,
                BuildingType = data.type
            });
        }
    }

    public void RemoveWorker(Villager villager)
    {
        if (assignedWorkers.Contains(villager))
        {
            assignedWorkers.Remove(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.WorkerRemovedEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName,
                BuildingType = data.type
            });
        }
    }

    public void AddResident(Villager villager)
    {
        if (data.type != BuildingType.Home) return;

        if (residents.Count < data.maxResidents && !residents.Contains(villager))
        {
            residents.Add(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.ResidentAssignedEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName
            });
        }
    }

    public void RemoveResident(Villager villager)
    {
        if (residents.Contains(villager))
        {
            residents.Remove(villager);

            EventManager.Instance.TriggerEvent(new BuildingEvents.ResidentRemovedEvent
            {
                VillagerName = villager.villagerName,
                BuildingName = data.buildingName
            });
        }
    }

    // Method to fulfill needs for a villager
    public bool FulfillNeed(Villager villager, Need need)
    {
        NeedFulfillmentData fulfillmentData = data.needsFulfilled.Find(f => f.needType == need.Name);

        if (fulfillmentData == null)
            return false;

        need.Fulfill(fulfillmentData.fulfillmentAmount);
        return true;
    }

    // Check if this building can fulfill a specific need
    public bool CanFulfillNeed(string needType)
    {
        return data.needsFulfilled.Any(f => f.needType == needType);
    }

    public Transform GetEntrancePoint()
    {
        return entrancePoint;
    }

    public bool HasOccupant(Villager villager)
    {
        return currentOccupants.Contains(villager);
    }

    public bool HasWorker(Villager villager)
    {
        return assignedWorkers.Contains(villager);
    }

    public bool IsResident(Villager villager)
    {
        return residents.Contains(villager);
    }

    public List<Villager> GetResidents()
    {
        return residents;
    }

    public bool CanWork(Villager villager)
    {
        if (!data.isWorkplace) return false;

        var professionManager = villager.GetComponent<VillagerProfession>();
        return professionManager != null &&
               data.validProfessions.Contains(professionManager.ProfessionType) &&
               assignedWorkers.Count < data.maxWorkers;
    }

    public void OnTimeUpdate()
    {
        functionality?.OnTimeUpdate();
    }

    public ResourceType GetProducedResourceType()
    {
        return data.producedResourceType;
    }

}
