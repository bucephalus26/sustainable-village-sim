using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    // Singleton
    public static EconomyManager Instance { get; private set; }

    [Header("Economy Settings")]
    [SerializeField] private float initialFoodAmount = 100f;
    [SerializeField] private float initialWealthAmount = 100f;
    [SerializeField] private float initialGoodsAmount = 50f;
    [SerializeField] private float initialStoneAmount = 20f;

    [Header("Price Settings")]
    [SerializeField] private float foodBasePrice = 1.0f;
    [SerializeField] private float goodsBasePrice = 2.0f;
    [SerializeField] private float stoneBasePrice = 3.0f;

    // Resource tracking
    private Dictionary<ResourceType, float> resources = new();

    // Price tracking (affected by supply and demand)
    private Dictionary<ResourceType, float> prices = new();

    // History for economy stats
    private Dictionary<ResourceType, List<float>> resourceHistory = new();
    private List<float> wealthHistory = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEconomy();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEconomy()
    {
        // Initialize resources
        resources[ResourceType.Food] = initialFoodAmount;
        resources[ResourceType.Wealth] = initialWealthAmount;
        resources[ResourceType.Goods] = initialGoodsAmount;
        resources[ResourceType.Stone] = initialStoneAmount;

        // Initialize prices
        prices[ResourceType.Food] = foodBasePrice;
        prices[ResourceType.Goods] = goodsBasePrice;
        prices[ResourceType.Stone] = stoneBasePrice;
        prices[ResourceType.Wealth] = 1.0f;

        // Initialize history tracking
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resourceHistory[type] = new List<float>();
        }
    }

    private void Update()
    {
        // Record history every game day
        if (TimeManager.Instance != null &&
            TimeManager.Instance.CurrentTimeOfDay == TimeOfDay.Morning &&
            TimeManager.Instance.CurrentHour < 7f)
        {
            RecordEconomySnapshot();
        }
    }

    private void RecordEconomySnapshot()
    {
        foreach (var resource in resources)
        {
            resourceHistory[resource.Key].Add(resource.Value);

            // Limit history size
            if (resourceHistory[resource.Key].Count > 30)
            {
                resourceHistory[resource.Key].RemoveAt(0);
            }
        }
    }


    // Deducts resource when consumed
    public bool ConsumeResource(ResourceType type, float amount)
    {
        if (type == ResourceType.None) return true;

        if (!resources.ContainsKey(type) || resources[type] < amount)
        {
            EventManager.Instance.TriggerEvent(new EconomyEvents.ResourceCriticalEvent
            {
                ResourceType = type,
                CurrentAmount = resources.GetValueOrDefault(type)
            });
            return false;
        }

        resources[type] -= amount;

        EventManager.Instance.TriggerEvent(new EconomyEvents.ResourceChangeEvent
        {
            ResourceType = type,
            Amount = -amount,
            NewTotal = resources[type],
            Source = "Consumption"
        });

        // Check if resource is critically low (e.g., below 20%)
        if (resources[type] < 20f)
        {
            EventManager.Instance.TriggerEvent(new EconomyEvents.ResourceCriticalEvent
            {
                ResourceType = type,
                CurrentAmount = resources[type]
            });
        }

        // Update prices based on demand
        UpdateResourcePrice(type);

        return true;
    }

    // Add resource
    public void AddResource(ResourceType type, float amount)
    {
        if (type == ResourceType.None) return;

        resources[type] = resources.GetValueOrDefault(type) + amount;

        EventManager.Instance.TriggerEvent(new EconomyEvents.ResourceChangeEvent
        {
            ResourceType = type,
            Amount = amount,
            NewTotal = resources[type],
            Source = "Production"
        });

        // Update prices based on supply
        UpdateResourcePrice(type);
    }

    // Price update based on supply and demand
    private void UpdateResourcePrice(ResourceType type)
    {
        if (type == ResourceType.None || type == ResourceType.Wealth) return;

        float basePrice = type switch
        {
            ResourceType.Food => foodBasePrice,
            ResourceType.Goods => goodsBasePrice,
            ResourceType.Stone => stoneBasePrice,
            _ => 1.0f
        };

        // Calculate new price based on scarcity - more supply = lower price
        float availableAmount = resources[type];
        float oldPrice = prices[type];

        // Scale price inversely with quantity (more quantity = lower price)
        float scarcityFactor = 100f / (availableAmount + 50f);
        float newPrice = basePrice * scarcityFactor;

        // Limit price changes to avoid wild swings
        newPrice = Mathf.Clamp(newPrice, basePrice * 0.5f, basePrice * 3f);

        // Only update if there's a significant change
        if (Mathf.Abs(newPrice - oldPrice) > 0.1f)
        {
            prices[type] = newPrice;

            EventManager.Instance.TriggerEvent(new EconomyEvents.PriceChangeEvent
            {
                ResourceType = type,
                OldPrice = oldPrice,
                NewPrice = newPrice
            });
        }
    }

    public float GetResourceAmount(ResourceType type)
        => resources.GetValueOrDefault(type);

    public float GetResourcePrice(ResourceType type)
        => prices.GetValueOrDefault(type, 1.0f);

    // Get value for a resource (amount * price)
    public float GetResourceValue(ResourceType type, float amount)
        => amount * GetResourcePrice(type);
}