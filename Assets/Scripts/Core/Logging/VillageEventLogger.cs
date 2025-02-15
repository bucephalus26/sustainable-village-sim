using UnityEngine;

public class VillageEventLogger : MonoBehaviour
{
    private bool isInitialized = false;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            SubscribeToEvents();
            isInitialized = true;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        isInitialized = false;
    }

    private void SubscribeToEvents()
    {
        EventManager.Instance.AddListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);
        EventManager.Instance.AddListener<VillagerEvents.NeedFulfilledEvent>(OnNeedFulfilled);
        EventManager.Instance.AddListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);
        EventManager.Instance.AddListener<VillagerEvents.ProfessionWorkCompletedEvent>(OnWorkCompleted);
        EventManager.Instance.AddListener<VillagerEvents.WealthChangedEvent>(OnWealthChanged);
        EventManager.Instance.AddListener<ResourceEvents.ResourceChangeEvent>(OnResourceChanged);
        EventManager.Instance.AddListener<ResourceEvents.ResourceCriticalEvent>(OnResourceCritical);
    }

    private void UnsubscribeFromEvents()
    {
        if (EventManager.Instance == null) return; // Safety check

        EventManager.Instance.RemoveListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);
        EventManager.Instance.RemoveListener<VillagerEvents.NeedFulfilledEvent>(OnNeedFulfilled);
        EventManager.Instance.RemoveListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);
        EventManager.Instance.RemoveListener<VillagerEvents.ProfessionWorkCompletedEvent>(OnWorkCompleted);
        EventManager.Instance.RemoveListener<VillagerEvents.WealthChangedEvent>(OnWealthChanged);
        EventManager.Instance.RemoveListener<ResourceEvents.ResourceChangeEvent>(OnResourceChanged);
        EventManager.Instance.RemoveListener<ResourceEvents.ResourceCriticalEvent>(OnResourceCritical);
    }

    private void OnVillagerStateChanged(VillagerEvents.StateChangeEvent evt)
    {
        Debug.Log($"[State Change] Villager {evt.VillagerName} ({evt.ProfessionType}) changed to {evt.NewState.Name}");
    }

    private void OnNeedFulfilled(VillagerEvents.NeedFulfilledEvent evt)
    {
        Debug.Log($"[Need Fulfilled] {evt.VillagerName}'s {evt.NeedType} fulfilled. New value: {evt.NewValue}");
    }

    private void OnNeedBecameCritical(VillagerEvents.NeedBecameCriticalEvent evt)
    {
        Debug.LogWarning($"[Need Critical] {evt.VillagerName}'s {evt.NeedType} is critical! Value: {evt.CurrentValue}");
    }

    private void OnWorkCompleted(VillagerEvents.ProfessionWorkCompletedEvent evt)
    {
        Debug.Log($"[Work Completed] {evt.VillagerName} ({evt.ProfessionType}) produced {evt.ResourcesProduced} resources");
    }

    private void OnWealthChanged(VillagerEvents.WealthChangedEvent evt)
    {
        Debug.Log($"[Wealth Change] {evt.VillagerName} earned {evt.Amount}. New total: {evt.NewTotal}");
    }

    private void OnResourceChanged(ResourceEvents.ResourceChangeEvent evt)
    {
        Debug.Log($"[Resource Change] {evt.ResourceType}: {(evt.Amount >= 0 ? "+" : "")}{evt.Amount} = {evt.NewTotal} ({evt.Source})");
    }

    private void OnResourceCritical(ResourceEvents.ResourceCriticalEvent evt)
    {
        Debug.LogWarning($"[Resource Critical] {evt.ResourceType} is low! Amount: {evt.CurrentAmount}");
    }
}
