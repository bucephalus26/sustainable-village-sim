using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private TimeControlPanel timeControlPanel;

    [Header("Panel References")]
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private GameObject mainDashboard;
    // We'll add references to other panels later

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

    // Show/hide main panels
    public void ToggleLeftPanel(bool show)
    {
        if (leftPanel != null)
        {
            leftPanel.SetActive(show);
        }
    }

    public void ToggleMainDashboard(bool show)
    {
        if (mainDashboard != null)
        {
            mainDashboard.SetActive(show);
        }
    }
}