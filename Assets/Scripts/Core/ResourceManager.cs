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
        resources[ResourceType.CommunalWealth] = 100f;
    }

    // Deducts resource when consumed
    public bool ConsumeResource(ResourceType type, float amount)
    {
        if (!resources.ContainsKey(type) || resources[type] < amount) return false;
        resources[type] -= amount;
        return true;
    }

    // Add resource
    public void AddResource(ResourceType type, float amount) 
        => resources[type] = resources.GetValueOrDefault(type) + amount;

    public float GetResourceAmount(ResourceType type)
        => resources.GetValueOrDefault(type);
}