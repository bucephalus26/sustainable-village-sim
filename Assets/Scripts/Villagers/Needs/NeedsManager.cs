using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeedsManager : INeedsManager
{
    private List<Need> needs = new List<Need>();
    private ResourceManager resourceManager;
    private VillagerServices services;

    public NeedsManager(VillagerServices services)
    {
        this.services = services;
        resourceManager = ResourceManager.Instance;

        needs.Add(new HungerNeed(services));
        needs.Add(new RestNeed(services));
    }

    public void UpdateNeeds()
    {
        foreach (var need in needs)
        {
            need.Decay(Time.deltaTime);
        }
    }

    public void FulfillNeed(Need need)
    {
        need.Fulfill(resourceManager);
    }

    public Transform GetNeedLocation(Need need)
    {
        return services.WorkplaceFinder.GetNeedLocation(need);
    }

    public Need GetMostUrgentNeed()
    {
        return needs.Where(need => need.IsCritical()).OrderBy(n => n.CurrentValue).FirstOrDefault();
    }

    public IReadOnlyList<Need> GetAllNeeds()
    {
        return needs;
    }

    public bool HasUrgentNeeds => needs.Any(need => need.IsCritical());
}
