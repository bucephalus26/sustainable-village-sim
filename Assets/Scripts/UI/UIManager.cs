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

    [Header("Villager Interaction")]
    [SerializeField] private VillagerInfoPopupController villagerInfoPopupPrefab;
    [SerializeField] private VillagerDetailPanelController villagerDetailPanelPrefab;
    [SerializeField] private GameObject detailViewOverlay;

    private VillagerInfoPopupController villagerInfoPopupInstance;
    private VillagerDetailPanelController villagerDetailPanelInstance;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InstantiateVillagerPopup();
            InstantiateDetailPanel();
            if (detailViewOverlay != null) detailViewOverlay.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InstantiateVillagerPopup()
    {
        if (villagerInfoPopupPrefab != null && villagerInfoPopupInstance == null)
        {
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                villagerInfoPopupInstance = Instantiate(villagerInfoPopupPrefab, mainCanvas.transform);
                villagerInfoPopupInstance.gameObject.SetActive(false);
                villagerInfoPopupInstance.name = "VillagerInfoPopup_Instance";
            }
            else
            {
                Debug.LogError("UIManager could not find a Canvas to instantiate the Villager Info Popup into!");
            }
        }
        else if (villagerInfoPopupPrefab == null)
        {
            Debug.LogError("VillagerInfoPopup Prefab not assigned in UIManager Inspector!");
        }
    }

    void InstantiateDetailPanel()
    {
        if (villagerDetailPanelPrefab != null && villagerDetailPanelInstance == null)
        {
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                villagerDetailPanelInstance = Instantiate(villagerDetailPanelPrefab, mainCanvas.transform);
                villagerDetailPanelInstance.gameObject.SetActive(false); // Start hidden
                villagerDetailPanelInstance.name = "VillagerDetailPanel_Instance";
            }
            else { Debug.LogError("UIManager could not find Canvas for Detail Panel!"); }
        }
        else if (villagerDetailPanelPrefab == null) { Debug.LogError("VillagerDetailPanel Prefab not assigned in UIManager!"); }
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

    // Villager Popup
    public void ShowVillagerPopup(Villager villager)
    {
        if (villagerInfoPopupInstance != null)
        {
            villagerInfoPopupInstance.ShowPopup(villager);
        }
        else
        {
            Debug.LogWarning("Villager Info Popup instance is missing. Cannot show popup.");
        }
    }

    public void HideVillagerPopup()
    {
        if (villagerInfoPopupInstance != null && villagerInfoPopupInstance.gameObject.activeInHierarchy)
        {
            villagerInfoPopupInstance.HidePopup();
        }
    }

    public void ShowVillagerDetailPanel(Villager villager)
    {
        HideVillagerPopup(); // Close small popup if open
        if (villagerDetailPanelInstance != null)
        {
            villagerDetailPanelInstance.ShowPanel(villager);
            if (detailViewOverlay != null) detailViewOverlay.SetActive(true); // Show overlay
        }
        else { Debug.LogWarning("Villager Detail Panel instance is missing."); }
    }

    public void HideVillagerDetailPanel()
    {
        if (villagerDetailPanelInstance != null && villagerDetailPanelInstance.gameObject.activeInHierarchy)
        {
            villagerDetailPanelInstance.HidePanel();
            if (detailViewOverlay != null) detailViewOverlay.SetActive(false); // Hide overlay
        }
    }

    public void ShowMainDashboard()
    {
        if (mainDashboard != null)
        {
            mainDashboard.SetActive(true);
        }
    }

    // Method to close the dashboard
    public void CloseDashboard()
    {
        if (mainDashboard != null)
        {
            mainDashboard.SetActive(false);
        }
    }

    public GameObject GetDashboard()
    {
        return mainDashboard;
    }
}