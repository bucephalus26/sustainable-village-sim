using UnityEngine;

public class IdleState : VillagerBaseState
{
    private float idleDuration;
    private float timer;

    public IdleState(VillagerBrain brain) : base(brain)
    {
        idleDuration = UnityEngine.Random.Range(2f, 5f);
        timer = 0f;
    }

    public override void EnterState()
    {
        // Just stay in place or wander slightly
        brain.Movement.SetTargetPosition(brain.LocationFinder.GetRandomNearbyPosition());
    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;

        // Check for urgent needs every frame
        var urgentNeed = brain.NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            brain.TransitionTo(new NeedFulfillmentState(brain, urgentNeed));
            return;
        }

        // If we've been idle long enough, determine what to do next
        if (timer >= idleDuration)
        {
            brain.DetermineNextAction();
        }
    }

}
