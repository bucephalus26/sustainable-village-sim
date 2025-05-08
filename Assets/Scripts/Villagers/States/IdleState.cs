using UnityEngine;

public class IdleState : VillagerBaseState
{
    public string DetailedIdleActivity { get; private set; } = "Thinking...";
    float decisionTimer;
    float timeUntilNextDecision = 5f;
    bool isWanderingToLocation = false;
    private Building currentWanderDestinationBuilding = null;

    public IdleState(VillagerBrain brain) : base(brain) { }

    public override void EnterState()
    {
        currentWanderDestinationBuilding = null;
        DetailedIdleActivity = "Deciding what to do";
        MakeIdleDecision();
    }

    public override void UpdateState()
    {
        // Check for urgent needs
        var urgentNeed = brain.NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            brain.TransitionTo(new NeedFulfillmentState(brain, urgentNeed));
            return;
        }

        decisionTimer += Time.deltaTime;

        if (isWanderingToLocation)
        {
            if (currentWanderDestinationBuilding != null)
            {
                DetailedIdleActivity = $"At the {currentWanderDestinationBuilding.BuildingName}";
            }
            else
            {
                DetailedIdleActivity = "Taking a break"; // random spot
            }

            currentWanderDestinationBuilding = null; 
            isWanderingToLocation = false;
            decisionTimer = 0f;
            timeUntilNextDecision = UnityEngine.Random.Range(3f, 7f);
        }

        // if time is up and not moving, decide again
        if (decisionTimer >= timeUntilNextDecision && !isWanderingToLocation)
        {
            MakeIdleDecision();
        }
    }

    void MakeIdleDecision()
    {
        decisionTimer = 0f;
        timeUntilNextDecision = UnityEngine.Random.Range(5f, 15f);
        currentWanderDestinationBuilding = null;

        float choice = UnityEngine.Random.value;

        if (choice < 0.4f)
        {
            currentWanderDestinationBuilding = brain.LocationFinder.GetLeisureBuilding();
            if (currentWanderDestinationBuilding != null)
            {
                Transform targetLocation = currentWanderDestinationBuilding.GetEntrancePoint();
                if (targetLocation != null && Vector3.Distance(brain.transform.position, targetLocation.position) > 1f)
                {
                    brain.Movement.SetTargetPosition(targetLocation.position);
                    isWanderingToLocation = true;
                    DetailedIdleActivity = $"Walking to {currentWanderDestinationBuilding.BuildingName}";
                    return;
                }
                else // already there
                { 
                    currentWanderDestinationBuilding = null;
                }
            }
            WanderLocally();
        }
        else if (choice < 0.8f)
        {
            WanderLocally();
        }
        else
        {
            StandStill();
        }
    }

    void WanderLocally()
    {
        currentWanderDestinationBuilding = null;
        Vector3 targetPos = brain.LocationFinder.GetRandomNearbyPosition();
        brain.Movement.SetTargetPosition(targetPos);
        isWanderingToLocation = true;
        DetailedIdleActivity = "Wandering Nearby";
    }

    void StandStill()
    {
        currentWanderDestinationBuilding = null;
        brain.Movement.ClearTarget();
        isWanderingToLocation = false;
        DetailedIdleActivity = "Standing Still";
        timeUntilNextDecision = UnityEngine.Random.Range(2f, 5f);
    }


    public override void ExitState()
    {
        isWanderingToLocation = false;
        currentWanderDestinationBuilding = null;
        DetailedIdleActivity = string.Empty;
    }

}
