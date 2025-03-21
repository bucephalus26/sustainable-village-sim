using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }

    [Header("Panel References")]
    [SerializeField] private TimeControlPanel timeControlPanel;
    [SerializeField] private VillageInformationPanel villageInformationPanel;
    [SerializeField] private GameObject mainDashboard;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to important events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener<TimeEvents.TimeOfDayChangedEvent>(OnTimeOfDayChanged);
        }
        else
        {
            Debug.LogWarning("EventManager instance not found");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener<TimeEvents.TimeOfDayChangedEvent>(OnTimeOfDayChanged);
        }
    }

    private void OnTimeOfDayChanged(TimeEvents.TimeOfDayChangedEvent evt)
    {
        // Update UI components when time changes
        if (timeControlPanel != null)
        {
            timeControlPanel.UpdateTimeDisplay();
        }
    }

    // Show/hide time control panel
    public void ToggleTimeControlPanel(bool show)
    {
        if (timeControlPanel != null)
        {
            timeControlPanel.gameObject.SetActive(show);
        }
    }

    public void ShowMainDashboard()
    {
        if (mainDashboard != null)
        {
            mainDashboard.SetActive(true);
        }
    }

    public GameObject GetDashboard()
    {
        return mainDashboard;
    }

    // Method to close the dashboard
    public void CloseDashboard()
    {
        if (mainDashboard != null)
        {
            mainDashboard.SetActive(false);
        }
    }

}