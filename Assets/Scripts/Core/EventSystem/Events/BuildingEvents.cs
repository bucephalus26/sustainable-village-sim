using UnityEngine;

public static class BuildingEvents
{
    public class BuildingRegisteredEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string BuildingName { get; set; }
        public BuildingType BuildingType { get; set; }
    }

    public class VillagerEnteredEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
        public BuildingType BuildingType;
    }

    public class VillagerExitedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
        public BuildingType BuildingType { get; set; }
    }

    public class WorkerAssignedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
        public BuildingType BuildingType { get; set; }
    }

    public class WorkerRemovedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
        public BuildingType BuildingType { get; set; }
    }

    public class ResidentAssignedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
    }

    public class ResidentRemovedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string VillagerName { get; set; }
        public string BuildingName { get; set; }
    }

    public class ResourceProducedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public string BuildingName { get; set; }
        public ResourceType ResourceType { get; set; }
        public float Amount { get; set; }
        public float Wealth { get; set; }
        public string WorkerName { get; set; }
    }
}
