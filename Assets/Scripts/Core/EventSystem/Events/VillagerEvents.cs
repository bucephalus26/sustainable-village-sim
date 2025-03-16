using UnityEngine;
using System;

public static class VillagerEvents
{
    public class VillagerInitializedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public ProfessionType ProfessionType { get; set; }
        public GameObject VillagerObject { get; set; }
    }

    public class AllVillagersInitializedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public int VillagerCount { get; set; }
    }

    public class StateChangeEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public string ProfessionType { get; set; }
        public Type NewState { get; set; }
    }

    public class NeedBecameCriticalEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public string NeedType { get; set; }
        public float CurrentValue { get; set; }
    }

    public class NeedFulfilledEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public string NeedType { get; set; }
        public float NewValue { get; set; }
    }

    public class NeedFulfillmentFailedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public string NeedType { get; set; }
        public ResourceType RequiredResource { get; set; }
        public float AmountNeeded { get; set; }
    }

    public class ProfessionWorkCompletedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public ProfessionType ProfessionType { get; set; }
        public float ResourcesProduced { get; set; }
    }

    public class WealthChangedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public float Amount { get; set; }
        public float NewTotal { get; set; }
    }

    public class MoodChangedEvent : IVillageEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string VillagerName { get; set; }
        public VillagerMood.MoodState OldMood { get; set; }
        public VillagerMood.MoodState NewMood { get; set; }
        public float HappinessValue { get; set; }
    }


}
