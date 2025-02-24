using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single villager entry in the villager panel
/// </summary>
public class VillagerEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text professionText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button detailsButton;
    [SerializeField] private Image backgroundImage;

    [Header("Styling")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new(1f, 0.5f, 0.5f, 1f);

    private Villager villagerReference;

    public string VillagerName => villagerReference?.villagerName ?? "Unknown";
    public string ProfessionType { get; private set; } = "Unknown";
    public string CurrentState { get; private set; } = "Unknown";

    public void Initialize(Villager villager)
    {
        villagerReference = villager;

        // Ensure this entry is enabled
        gameObject.SetActive(true);

        // Set villager name
        if (nameText != null)
        {
            nameText.gameObject.SetActive(true);
            nameText.text = villager.villagerName;
        }

        EventManager.Instance.AddListener<VillagerEvents.VillagerInitializedEvent>(OnVillagerInitialized);

        SetupVillagerInfo();
    }

    private void OnVillagerInitialized(VillagerEvents.VillagerInitializedEvent evt)
    {
        if (evt.VillagerName == villagerReference.villagerName)
        {
            SetupVillagerInfo();
            // Unsubscribe since we don't need to listen anymore
            EventManager.Instance.RemoveListener<VillagerEvents.VillagerInitializedEvent>(OnVillagerInitialized);
        }
    }

    private void SetupVillagerInfo()
    {
        if (villagerReference == null) return;

        // Get and set profession
        var professionManager = villagerReference.GetComponent<ProfessionManager>();
        if (professionManager != null)
        {
            ProfessionType = professionManager.GetProfessionType().ToString();
            professionText.text = $"Profession: {ProfessionType}";

            // Get and set current state
            var context = villagerReference.GetComponent<VillagerContext>();
            if (context != null)  // Check context first
            {
                if (context.CurrentState != null)  // Then check state
                {
                    UpdateStateDisplay(context.CurrentState.GetType().Name);
                }
                else
                {
                    Debug.LogWarning($"CurrentState is null for villager {villagerReference.villagerName}");
                }
            }
            else
            {
                Debug.LogWarning($"VillagerContext not found for villager {villagerReference.villagerName}");
            }

            // Set up details button
            if (detailsButton != null)
            {
                detailsButton.onClick.AddListener(ShowDetails);
            }

            // Check for critical needs
            CheckCriticalStatus();
        }
    }

    public void UpdateStateDisplay(string stateName)
    {
        // Remove "State" suffix if present for cleaner display
        if (stateName.EndsWith("State"))
        {
            stateName = stateName.Substring(0, stateName.Length - 5);
        }

        CurrentState = stateName;

        if (stateText != null)
        {
            stateText.text = $"State: {stateName}";
        }

        // Check for critical state
        if (stateName == "NeedFulfillment")
        {
            CheckCriticalStatus();
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    private void CheckCriticalStatus()
    {
        if (villagerReference == null) return;

        // This would require access to the needs manager
        // For now, we'll simplify and just check if they're in need fulfillment state
        bool isCritical = CurrentState == "NeedFulfillment";

        if (backgroundImage != null)
        {
            backgroundImage.color = isCritical ? criticalColor : normalColor;
        }
    }

    private void ShowDetails()
    {
        if (villagerReference == null) return;

        // Logic to show detailed view - could open a modal window
        Debug.Log($"Showing details for {villagerReference.villagerName}");

        // Optional: Open a detailed view panel
        // UIManager.Instance.ShowPanel("VillagerDetailsPanel");
        // UIManager.Instance.GetPanel<VillagerDetailsPanel>("VillagerDetailsPanel").SetVillager(villagerReference);
    }

    private void OnDestroy()
    {
        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveListener(ShowDetails);
        }

        EventManager.Instance?.RemoveListener<VillagerEvents.VillagerInitializedEvent>(OnVillagerInitialized);
    }

}