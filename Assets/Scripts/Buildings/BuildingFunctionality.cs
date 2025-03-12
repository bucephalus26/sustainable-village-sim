using UnityEngine;

public interface IBuildingFunctionality
{
    void OnTimeUpdate();
}

// Basic functionality for all buildings
public class BasicFunctionality : IBuildingFunctionality
{
    protected Building building;

    public BasicFunctionality(Building building)
    {
        this.building = building;
    }

    public virtual void OnTimeUpdate() { }
}

// Home functionality
public class HomeFunctionality : BasicFunctionality
{
    public HomeFunctionality(Building building) : base(building) { }

    public override void OnTimeUpdate()
    {
        // Homes might have special time-based functions
        // e.g., automatic rest fulfillment for residents at night
    }
}

// Social building functionality (tavern, church)
public class SocialFunctionality : BasicFunctionality
{
    private float needFulfillmentInterval = 2.0f;
    private float timeUntilNextFulfillment = 0f;

    public SocialFunctionality(Building building) : base(building) { }

    public override void OnTimeUpdate()
    {
        // Handle need fulfillment timing
        timeUntilNextFulfillment -= Time.deltaTime * TimeManager.Instance.TimeScaleFactor;

        if (timeUntilNextFulfillment <= 0)
        {
            // Reset timer
            timeUntilNextFulfillment = needFulfillmentInterval;
        }
    }
}

// Workplace functionality (farms, shops, etc.)
public class WorkplaceFunctionality : BasicFunctionality
{
    private float productionInterval = 1.0f;
    private float timeUntilNextProduction = 0f;

    public WorkplaceFunctionality(Building building) : base(building) { }

    public override void OnTimeUpdate()
    {
        // Handle production timing
        timeUntilNextProduction -= Time.deltaTime * TimeManager.Instance.TimeScaleFactor;

        if (timeUntilNextProduction <= 0)
        {
            // Reset timer
            timeUntilNextProduction = productionInterval;
        }
    }
}