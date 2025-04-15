using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // For FirstOrDefault
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VillagerInfoPopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI villagerNameText;
    [SerializeField] private TextMeshProUGUI professionValueText;
    [SerializeField] private TextMeshProUGUI activityValueText;
    [SerializeField] private TextMeshProUGUI moodValueText;

    [Header("Needs References")]
    [SerializeField] private TextMeshProUGUI hungerValueText;
    [SerializeField] private Image hungerFillImage;
    [SerializeField] private TextMeshProUGUI restValueText;
    [SerializeField] private Image restFillImage;
    [SerializeField] private TextMeshProUGUI socialValueText;
    [SerializeField] private Image socialFillImage;

    [Header("Interaction")]
    [SerializeField] private Button viewDetailsButton;

    [Header("Positioning")]
    [SerializeField] private Vector2 screenOffset = new(20f, 0f);
    [SerializeField] private float screenPadding = 10f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Villager currentVillager;
    private Camera mainCamera;
    private Canvas parentCanvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null || rectTransform == null)
        {
            Debug.LogError("VillagerInfoPopupController requires CanvasGroup and RectTransform components.", this);
            enabled = false;
            return;
        }

        viewDetailsButton?.onClick.AddListener(OnViewDetailsClicked);
        HidePopup();
    }

    public void ShowPopup(Villager villager)
    {
        if (villager == null || villager.Brain == null || mainCamera == null)
        {
            Debug.LogWarning("Cannot show popup - villager, brain, or camera is missing.");
            HidePopup();
            return;
        }

        currentVillager = villager;

        // --- Populate UI ---
        villagerNameText.text = villager.villagerName;
        professionValueText.text = GetProfessionString(villager);
        activityValueText.text = GetActivityString(villager);
        moodValueText.text = GetMoodString(villager);

        UpdateNeedsDisplay(villager.Brain.NeedsManager);


        // --- Position UI ---
        PositionPopup(villager.transform.position);

        // --- Show ---
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true); // Make sure GO is active
    }

    public void HidePopup()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        currentVillager = null;
    }

    void UpdateNeedsDisplay(INeedsManager needsManager)
    {
        if (needsManager == null) return;

        var needs = needsManager.GetAllNeeds();

        Need hunger = needs.FirstOrDefault(n => n.Name == "Hunger");
        Need rest = needs.FirstOrDefault(n => n.Name == "Rest");
        Need social = needs.FirstOrDefault(n => n.Name == "Social");

        UpdateSingleNeedUI(hungerValueText, hungerFillImage, hunger);
        UpdateSingleNeedUI(restValueText, restFillImage, rest);
        UpdateSingleNeedUI(socialValueText, socialFillImage, social);
    }

    void UpdateSingleNeedUI(TextMeshProUGUI valueText, Image fillImage, Need need)
    {
        if (need != null)
        {
            float percentage = need.CurrentValue;
            valueText.text = $"{percentage:F0}%";
            fillImage.fillAmount = percentage / 100f;
            valueText.gameObject.SetActive(true);
            fillImage.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            // Hide if need doesn't exist for some reason
            valueText.text = "N/A";
            fillImage.fillAmount = 0;
            valueText.gameObject.SetActive(false);
            fillImage.transform.parent.gameObject.SetActive(false);
        }
    }

    void PositionPopup(Vector3 targetWorldPosition)
    {
        // villager's center position on screen
        Vector3 villagerScreenCenter = mainCamera.WorldToScreenPoint(targetWorldPosition);

        // Apply offset
        Vector2 desiredTopLeftPos = new(
           villagerScreenCenter.x + screenOffset.x,
           villagerScreenCenter.y + screenOffset.y);

        // 3. Get Canvas Scale Factor (important for accurate size calculation)
        float canvasScale = parentCanvas.scaleFactor;

        // 4. Get Popup size in actual screen pixels
        float popupWidth = rectTransform.rect.width * canvasScale;
        float popupHeight = rectTransform.rect.height * canvasScale;

        // 5. Clamp the desired Top-Left position to screen bounds
        // Pivot is (0, 1) - Top Left
        float clampedX = Mathf.Clamp(desiredTopLeftPos.x,
                                    screenPadding,                     // Min X (Left edge can't go past left padding)
                                    Screen.width - screenPadding - popupWidth); // Max X (Left edge can't make right edge go past right padding)

        float clampedY = Mathf.Clamp(desiredTopLeftPos.y,
                                    screenPadding + popupHeight,       // Min Y (Top edge can't make bottom edge go past bottom padding)
                                    Screen.height - screenPadding);    // Max Y (Top edge can't go past top padding)
        // Set anchored position
        rectTransform.position = new Vector3(clampedX, clampedY, villagerScreenCenter.z);
    }

    void OnViewDetailsClicked()
    {
        Debug.Log("--- OnViewDetailsClicked METHOD ENTERED ---");
        if (currentVillager != null && UIManager.Instance != null)
        {
            // Tell UIManager to open the detailed panel
            UIManager.Instance.ShowVillagerDetailPanel(currentVillager);
        }
        HidePopup(); // Hide this small popup
    }

    // Methods for Display Strings
    string GetProfessionString(Villager v)
    {
        if (v.Brain?.Profession?.ProfessionData != null)
        {
            // Use display name from data if available
            return string.IsNullOrEmpty(v.Brain.Profession.ProfessionData.professionName) ?
                   v.Brain.Profession.ProfessionData.professionName :
                   v.Brain.Profession.ProfessionData.professionName;
        }
        return v.Brain?.Profession?.ProfessionType.ToString() ?? "Unemployed"; // Fallback
    }

    string GetActivityString(Villager v)
    {
        if (v.Brain?.CurrentState == null) return "Idle";
        // Simple mapping from state type name
        return v.Brain.CurrentState.GetType().Name.Replace("State", "");
    }

    string GetMoodString(Villager v)
    {
        if (v.Brain?.VillagerMood == null) return "Content";
        return v.Brain.VillagerMood.CurrentMood.ToString();
    }

    void Update()
    {
        // Check for left mouse button down ONLY
        if (!Input.GetMouseButtonDown(0)) return;

        // Check if the popup is actually visible and meant to be interactive
        if (canvasGroup.alpha < 0.9f || !canvasGroup.interactable || currentVillager == null) return;


        // --- Use Event System to see what was clicked ---
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Assume we should close unless we hit something belonging to this popup
        bool clickedOnPopup = false;
        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                // Check if the hit object is this popup or one of its children
                if (result.gameObject == this.gameObject || result.gameObject.transform.IsChildOf(this.transform))
                {
                    clickedOnPopup = true;
                    break; // Found a hit on the popup, no need to check further
                }
            }
        }

        // --- Close only if the click was NOT on the popup UI ---
        if (!clickedOnPopup)
        {
            // Optional: Add the check to prevent closing if clicking the original villager again
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray); // Use 2D raycast
            if (hit.collider != null && hit.collider.gameObject == currentVillager.gameObject)
            {
                // Clicked the villager again, don't close the popup
            }
            else
            {
                // Clicked outside the popup AND outside the villager that opened it
                Debug.Log("Clicked outside popup UI, hiding."); // Add log for confirmation
                HidePopup();
            }
        }
        // else { Debug.Log("Clicked inside popup UI."); } // Optional log
    }
}