using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Singleton instance
    public static ResourceManager Instance { get; private set; }
    private Dictionary<ResourceType, float> resources = new Dictionary<ResourceType, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeResources();
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void InitializeResources()
    {
        resources[ResourceType.Food] = 100f;
        resources[ResourceType.Wealth] = 100f;
    }

    // Deducts resource when consumed
    public bool ConsumeResource(ResourceType type, float amount)
    {
        if (!resources.ContainsKey(type) || resources[type] < amount)
        {
            EventManager.Instance.TriggerEvent(new ResourceEvents.ResourceCriticalEvent
            {
                ResourceType = type,
                CurrentAmount = resources.GetValueOrDefault(type)
            });
            return false;
        }

        resources[type] -= amount;

        EventManager.Instance.TriggerEvent(new ResourceEvents.ResourceChangeEvent
        {
           ResourceType = type,
           Amount = -amount,
           NewTotal =resources[type],
           Source = "Consumption"
        });

        // Check if resource is critically low (e.g., below 20%)
        if (resources[type] < 20f)
        {
            EventManager.Instance.TriggerEvent(new ResourceEvents.ResourceCriticalEvent
            {
                ResourceType = type,
                CurrentAmount = resources[type]
            });
        }

        return true;
    }

    // Add resource
    public void AddResource(ResourceType type, float amount)
    {
        resources[type] = resources.GetValueOrDefault(type) + amount;

        EventManager.Instance.TriggerEvent(new ResourceEvents.ResourceChangeEvent
        {
            ResourceType = type,
            Amount = amount,
            NewTotal = resources[type],
            Source = "Production"
        });
    }
        

    public float GetResourceAmount(ResourceType type)
        => resources.GetValueOrDefault(type);
}