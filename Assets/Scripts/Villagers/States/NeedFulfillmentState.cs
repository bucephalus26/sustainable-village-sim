using UnityEngine;

public class NeedFulfillmentState : VillagerBaseState
{
    private readonly Need currentNeed;
    private float fulfillmentTimer = 0f;
    private float timeToFulfill = 3f;
    private bool hasReachedDestination = false;

    public NeedFulfillmentState(VillagerBrain brain, Need need) : base(brain)
    {
        currentNeed = need;
    }

    public override void EnterState()
    {
        Transform location = brain.LocationFinder.GetNeedLocation(currentNeed);
        brain.Movement.SetTargetPosition(location.position);
    }

    public override void UpdateState()
    {
        // Check if we've reached the destination
        if (!hasReachedDestination && brain.Movement.HasReachedTarget())
        {
            hasReachedDestination = true;
        }

        if (hasReachedDestination)
        {
            fulfillmentTimer += Time.deltaTime;

            if (fulfillmentTimer >= timeToFulfill)
            {
                brain.NeedsManager.FulfillNeed(currentNeed);
                brain.DetermineNextAction();
            }
        }
        else
        {
            // If we can't reach the need location after some time, try a different approach
            fulfillmentTimer += Time.deltaTime;
            if (fulfillmentTimer > 10f) // 10 seconds of real time
            {
                brain.TransitionTo(new IdleState(brain));
            }
        }
    }
}
