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
            fulfillmentTimer = 0f;
        }

        if (hasReachedDestination)
        {
            fulfillmentTimer += Time.deltaTime;

            if (fulfillmentTimer >= timeToFulfill)
            {

                bool success = brain.NeedsManager.FulfillNeed(currentNeed);

                if (success)
                {
                    brain.DetermineNextAction(); // Success, decide next action
                }
                else
                {
                    Debug.LogWarning($"{brain.VillagerComponent.villagerName} failed to fulfill {currentNeed.Name}. Transitioning to Idling.");
                    brain.TransitionTo(new IdleState(brain));
                }
            }
        }
        else
        {
            // If we can't reach the need location after some time, try diff approach
            fulfillmentTimer += Time.deltaTime;
            if (fulfillmentTimer > 10f) // 10 seconds of real time
            {
                Debug.LogWarning($"{brain.VillagerComponent.villagerName} couldn't reach {currentNeed.Name} location. Idling.");
                brain.TransitionTo(new IdleState(brain));
            }
        }
    }
}
