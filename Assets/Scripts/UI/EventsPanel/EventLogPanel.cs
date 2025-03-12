using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the event log panel
/// </summary>
public class EventLogPanel : UIPanel
{
    [Header("Content References")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private TMP_InputField filterField;
    [SerializeField] private TMP_Dropdown categoryFilter;
    [SerializeField] private ScrollRect scrollRect; // Add this reference
    [SerializeField] private int maxEntries = 100;

    private Queue<GameObject> logEntries = new();
    private List<GameObject> instantiatedObjects = new();
    private bool shouldAutoScroll = true;
    private bool isInitialized = false;

    public override void Initialize()
    {
        base.Initialize();
        
        // Set up filtering
        if (filterField != null)
        {
            filterField.onValueChanged.AddListener(FilterEntries);
        }

        // Setup category filter
        if (categoryFilter != null)
        {
            categoryFilter.ClearOptions();
            categoryFilter.AddOptions(new List<string> {
                "All",
                "State Changes",
                "Resources",
                "Needs",
                "Work"
            });
            categoryFilter.onValueChanged.AddListener(FilterByCategory);
        }

        // Setup scroll rect
        if (scrollRect != null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>();
        }

        // Add drag detector to manage auto-scroll behavior
        scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

        isInitialized = true;
        // Subscribe to events
        SubscribeToEvents();
    }

    private void OnScrollRectValueChanged(Vector2 normalizedPosition)
    {
        // Only check if user is manually scrolling
        if (isInitialized)
        {
            // If we're very close to the bottom (0.1 or less), consider it "at bottom"
            // and enable auto-scrolling
            shouldAutoScroll = normalizedPosition.y <= 0.1f;
        }
    }

    private void AddLogEntry(string message, string category, LogEntryType type)
    {
        if (!gameObject.activeSelf) return;

        GameObject entryObj = Instantiate(logEntryPrefab, contentContainer);
        EventLogEntry entry = entryObj.GetComponent<EventLogEntry>();

        if (entry != null)
        {
            entry.Initialize($"[{System.DateTime.Now:HH:mm:ss}] {message}", category, type);
        }

        // Add to lists and manage maximum entries
        logEntries.Enqueue(entryObj);
        instantiatedObjects.Add(entryObj);

        if (logEntries.Count > maxEntries)
        {
            GameObject oldestEntry = logEntries.Dequeue();
            instantiatedObjects.Remove(oldestEntry);
            Destroy(oldestEntry);
        }

        // Force layout update and scroll to bottom
        if (shouldAutoScroll)
        {
            ForceScrollToBottom();
        }

        // Apply filters
        if (filterField != null)
        {
            FilterEntries(filterField.text);
        }
    }

    public void ForceScrollToBottom()
    {
        if (scrollRect == null) return;

        // Use coroutine to ensure UI updates before scrolling
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        // Wait for end of frame to ensure content size is updated
        yield return new WaitForEndOfFrame();

        // Force canvas update to ensure layout is current
        Canvas.ForceUpdateCanvases();

        // Set scroll position to bottom (0 = bottom, 1 = top)
        scrollRect.verticalNormalizedPosition = 0f;

        // Wait another frame and force it again to be sure
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void FilterEntries(string filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            foreach (var obj in instantiatedObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            return;
        }

        filterText = filterText.ToLowerInvariant();

        foreach (var obj in instantiatedObjects)
        {
            if (obj == null) continue;

            EventLogEntry entry = obj.GetComponent<EventLogEntry>();
            if (entry != null)
            {
                bool matchesFilter = entry.MessageText.ToLowerInvariant().Contains(filterText);
                obj.SetActive(matchesFilter);
            }
        }
    }

    private void FilterByCategory(int categoryIndex)
    {
        string selectedCategory = categoryFilter.options[categoryIndex].text;

        foreach (var obj in instantiatedObjects)
        {
            if (obj == null) continue;

            EventLogEntry entry = obj.GetComponent<EventLogEntry>();
            if (entry != null)
            {
                bool shouldShow = selectedCategory == "All" || entry.Category == selectedCategory;
                obj.SetActive(shouldShow);
            }
        }
    }

    private void SubscribeToEvents()
    {
        EventManager.Instance.AddListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);
        EventManager.Instance.AddListener<VillagerEvents.NeedFulfilledEvent>(OnNeedFulfilled);
        EventManager.Instance.AddListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);
        EventManager.Instance.AddListener<VillagerEvents.ProfessionWorkCompletedEvent>(OnWorkCompleted);
        EventManager.Instance.AddListener<VillagerEvents.WealthChangedEvent>(OnWealthChanged);
        EventManager.Instance.AddListener<EconomyEvents.ResourceChangeEvent>(OnResourceChanged);
        EventManager.Instance.AddListener<EconomyEvents.ResourceCriticalEvent>(OnResourceCritical);
    }

    private void UnsubscribeFromEvents()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.RemoveListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);
        EventManager.Instance.RemoveListener<VillagerEvents.NeedFulfilledEvent>(OnNeedFulfilled);
        EventManager.Instance.RemoveListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);
        EventManager.Instance.RemoveListener<VillagerEvents.ProfessionWorkCompletedEvent>(OnWorkCompleted);
        EventManager.Instance.RemoveListener<VillagerEvents.WealthChangedEvent>(OnWealthChanged);
        EventManager.Instance.RemoveListener<EconomyEvents.ResourceChangeEvent>(OnResourceChanged);
        EventManager.Instance.RemoveListener<EconomyEvents.ResourceCriticalEvent>(OnResourceCritical);
    }

    #region Event Handlers
    private void OnVillagerStateChanged(VillagerEvents.StateChangeEvent evt)
    {
        AddLogEntry($"{evt.VillagerName} ({evt.ProfessionType}) changed to {evt.NewState.Name}", "State Changes", LogEntryType.Normal);
    }

    private void OnNeedFulfilled(VillagerEvents.NeedFulfilledEvent evt)
    {
        AddLogEntry($"{evt.VillagerName}'s {evt.NeedType} fulfilled. Value: {evt.NewValue:F1}", "Needs", LogEntryType.Normal);
    }

    private void OnNeedBecameCritical(VillagerEvents.NeedBecameCriticalEvent evt)
    {
        AddLogEntry($"{evt.VillagerName}'s {evt.NeedType} is critical! Value: {evt.CurrentValue:F1}", "Needs", LogEntryType.Warning);
    }

    private void OnWorkCompleted(VillagerEvents.ProfessionWorkCompletedEvent evt)
    {
        AddLogEntry($"{evt.VillagerName} ({evt.ProfessionType}) produced {evt.ResourcesProduced:F1} resources", "Work", LogEntryType.Normal);
    }

    private void OnWealthChanged(VillagerEvents.WealthChangedEvent evt)
    {
        AddLogEntry($"{evt.VillagerName} earned {evt.Amount}. Total wealth: {evt.NewTotal:F1}", "Work", LogEntryType.Normal);
    }

    private void OnResourceChanged(EconomyEvents.ResourceChangeEvent evt)
    {
        string changeText = evt.Amount >= 0 ? $"+{evt.Amount:F1}" : $"{evt.Amount:F1}";
        AddLogEntry($"{evt.ResourceType}: {changeText} = {evt.NewTotal:F1} ({evt.Source})", "Resources", LogEntryType.Normal);
    }

    private void OnResourceCritical(EconomyEvents.ResourceCriticalEvent evt)
    {
        AddLogEntry($"{evt.ResourceType} is critically low! Amount: {evt.CurrentAmount:F1}", "Resources", LogEntryType.Warning);
    }
    #endregion

    protected override void Reset()
    {
        base.Reset();
        if (filterField != null)
        {
            filterField.text = string.Empty;
        }
        if (categoryFilter != null)
        {
            categoryFilter.value = 0;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEvents();

        if (filterField != null)
        {
            filterField.onValueChanged.RemoveListener(FilterEntries);
        }
        if (categoryFilter != null)
        {
            categoryFilter.onValueChanged.RemoveListener(FilterByCategory);
        }
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        }
    }
}
