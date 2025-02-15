using UnityEngine;

public static class ResourceEvents 
{
    public class ResourceChangeEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public ResourceType ResourceType { get; set; }
        public float Amount { get; set; }
        public float NewTotal { get; set; }
        public string Source { get; set; }
    }

    public class ResourceCriticalEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public ResourceType ResourceType { get; set; }
        public float CurrentAmount { get; set; }
    }
}
