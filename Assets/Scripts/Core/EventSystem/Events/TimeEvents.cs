using UnityEngine;

public static class TimeEvents
{
    public class TimeOfDayChangedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public TimeOfDay NewTimeOfDay { get; set; }
        public float Hour { get; set; }
        public int Minute { get; set; }
    }

    public class DayChangedEvent : IVillageEvent
    {
        public float Timestamp { get; } = Time.time;
        public int NewDay { get; set; }
    }
}