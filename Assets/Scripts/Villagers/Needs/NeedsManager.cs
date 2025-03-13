using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface INeedsManager
{
    void UpdateNeeds();
    void FulfillNeed(Need need);
    Need GetMostUrgentNeed();
    IReadOnlyList<Need> GetAllNeeds();
    bool HasUrgentNeeds { get; }
}

public class NeedsManager : INeedsManager
{
    private List<Need> needs = new();
    private EconomyManager economyManager;
    private Villager villager;
    private VillagerPersonality personality;

    public NeedsManager(Villager villager)
    {
        this.villager = villager;
        economyManager = EconomyManager.Instance;
        personality = villager.GetComponent<VillagerPersonality>();

        // Create needs for this villager
        needs.Add(new HungerNeed(villager));
        needs.Add(new RestNeed(villager));
        needs.Add(new SocialNeed(villager));
    }


    public void UpdateNeeds()
    {
        // Get time-scaled delta time for proper decay rates
        float scaledDeltaTime = Time.deltaTime * TimeManager.Instance.TimeScaleFactor;

        foreach (var need in needs)
        {
            // Decay needs based on scaled time
            need.Decay(scaledDeltaTime);
        }
    }

    public void FulfillNeed(Need need)
    {
        float fulfillmentAmount = 30f;

        if (personality != null)
        {
            // Resilient villagers get more benefit from need fulfillment
            fulfillmentAmount *= (1f + (personality.resilience * 0.2f));
        }

        need.Fulfill(fulfillmentAmount);
    }

    public Need GetMostUrgentNeed()
    {
        // Calculate modified critical threshold based on personality
        float resilience = personality != null ? personality.resilience : 0.5f;
        float criticalThresholdModifier = 1.0f + ((0.5f - resilience) * 0.4f);

        // Check for any needs below critical threshold first
        var criticalNeeds = needs
            .Where(need => need.CurrentValue < need.CriticalThreshold * criticalThresholdModifier)
            .ToList();

        if (criticalNeeds.Count > 0)
        {
            // Pick the most urgent critical need
            return criticalNeeds.OrderByDescending(n => n.GetUrgency()).FirstOrDefault();
        }

        // If no critical needs, use weighted urgency calculation
        var mostUrgentNeed = needs
            .OrderByDescending(n => n.GetUrgency())
            .FirstOrDefault();

        // Only return if it's somewhat urgent (> 0.4 urgency)
        if (mostUrgentNeed != null && mostUrgentNeed.GetUrgency() > 0.4f)
        {
            return mostUrgentNeed;
        }

        return null;
    }

    public IReadOnlyList<Need> GetAllNeeds()
    {
        return needs;
    }

    public bool HasUrgentNeeds => needs.Any(need => need.IsCritical());
}
