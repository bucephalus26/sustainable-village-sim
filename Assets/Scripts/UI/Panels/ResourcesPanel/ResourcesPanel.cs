using System.Collections.Generic;
using UnityEngine;

// <summary>
/// Controller for the village resources panel
/// </summary>
public class ResourcesPanel : UIPanel
{
    [System.Serializable]
    public class ResourceDisplayMapping
    {
        public ResourceType resourceType;
        public ResourceDisplay display;
    }

    [SerializeField] private List<ResourceDisplayMapping> resourceDisplays = new List<ResourceDisplayMapping>();
    private Dictionary<ResourceType, ResourceDisplay> displayLookup = new Dictionary<ResourceType, ResourceDisplay>();

    public override void Initialize()
    {
        base.Initialize();

        // Create lookup dictionary for quick access
        foreach (var mapping in resourceDisplays)
        {
            displayLookup[mapping.resourceType] = mapping.display;
        }

        // Subscribe to resource change events
        EventManager.Instance.AddListener<EconomyEvents.ResourceChangeEvent>(OnResourceChanged);
        EventManager.Instance.AddListener<EconomyEvents.ResourceCriticalEvent>(OnResourceCritical);

        // Initial update
        RefreshAllDisplays();
    }

    protected override void OnShow()
    {
        base.OnShow();
        RefreshAllDisplays();
    }

    private void RefreshAllDisplays()
    {
        if (EconomyManager.Instance == null) return;

        foreach (var mapping in resourceDisplays)
        {
            float amount = EconomyManager.Instance.GetResourceAmount(mapping.resourceType);
            mapping.display.UpdateDisplay(amount);
        }
    }

    private void OnResourceChanged(EconomyEvents.ResourceChangeEvent evt)
    {
        if (displayLookup.TryGetValue(evt.ResourceType, out ResourceDisplay display))
        {
            display.UpdateDisplay(evt.NewTotal);
        }
    }

    private void OnResourceCritical(EconomyEvents.ResourceCriticalEvent evt)
    {
        if (displayLookup.TryGetValue(evt.ResourceType, out ResourceDisplay display))
        {
            display.SetCriticalState(true);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener<EconomyEvents.ResourceChangeEvent>(OnResourceChanged);
            EventManager.Instance.RemoveListener<EconomyEvents.ResourceCriticalEvent>(OnResourceCritical);
        }
    }

}
