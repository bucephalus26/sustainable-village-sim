using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float dayLengthInSeconds = 300f; // 5 minutes = 1 in-game day
    [SerializeField] private float startHour = 6f;

    // Time tracking
    private float currentTimeInHours;
    private int currentDay = 1;
    private TimeOfDay currentTimeOfDay;

    // Time conversion factors
    public float TimeScaleFactor => 24f / dayLengthInSeconds; // How many game hours per real second
    public float CurrentHour => currentTimeInHours;
    public int CurrentMinute => Mathf.FloorToInt((currentTimeInHours % 1f) * 60f);
    public int CurrentDay => currentDay;
    public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentTimeInHours = startHour;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentTimeOfDay = DetermineTimeOfDay();
        // Broadcast initial time state
        BroadcastTimeChange();
    }

    private void Update()
    {
        // Update time based on real seconds
        float hourIncrement = Time.deltaTime * TimeScaleFactor;
        currentTimeInHours += hourIncrement;

        // Handle day change
        if (currentTimeInHours >= 24f)
        {
            currentTimeInHours -= 24f;
            currentDay++;

            // Trigger day change event
            EventManager.Instance.TriggerEvent(new TimeEvents.DayChangedEvent
            {
                NewDay = currentDay
            });
        }

        // Check if time of day changed
        TimeOfDay newTimeOfDay = DetermineTimeOfDay();
        if (newTimeOfDay != currentTimeOfDay)
        {
            currentTimeOfDay = newTimeOfDay;
            BroadcastTimeChange();
        }
    }

    private TimeOfDay DetermineTimeOfDay()
    {
        float hour = currentTimeInHours;

        if (hour >= 22 || hour < 6)
            return TimeOfDay.Night;
        else if (hour >= 6 && hour < 12)
            return TimeOfDay.Morning;
        else if (hour >= 12 && hour < 14)
            return TimeOfDay.Noon;
        else if (hour >= 14 && hour < 18)
            return TimeOfDay.Afternoon;
        else // 18-22
            return TimeOfDay.Evening;
    }

    private void BroadcastTimeChange()
    {
        // Notify all systems about time change
        EventManager.Instance.TriggerEvent(new TimeEvents.TimeOfDayChangedEvent
        {
            NewTimeOfDay = currentTimeOfDay,
            Hour = CurrentHour,
            Minute = CurrentMinute
        });

        // Update all buildings
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UpdateAllBuildings();
        }
    }
    public string GetTimeOfDayName()
    {
        return currentTimeOfDay.ToString();
    }

    // Method to get formatted time string (HH:MM)
    public string GetFormattedTime()
    {
        int hour = Mathf.FloorToInt(currentTimeInHours);
        int minute = Mathf.FloorToInt((currentTimeInHours % 1f) * 60f);

        // Convert to 12-hour format
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;
        string period = hour >= 12 ? "PM" : "AM";

        return $"{displayHour}:{minute:D2} {period}";
    }
}
