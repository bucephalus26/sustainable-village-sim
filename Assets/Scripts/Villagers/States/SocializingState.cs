using System.Linq;
using UnityEngine;

public class SocializingState : VillagerBaseState
{
    private float timer;
    private float socialDuration;
    private bool hasReachedDestination = false;

    public SocializingState(VillagerBrain brain) : base(brain)
    {
        // Longer duration for socializing - between 1-3 hours of game time
        socialDuration = UnityEngine.Random.Range(1f, 3f) * 60f / TimeManager.Instance.TimeScaleFactor;
        timer = 0f;
    }

    public override void EnterState()
    {
        // Go to tavern, church, or public space
        Transform leisureLocation = brain.LocationFinder.GetLeisureLocation();
        brain.Movement.SetTargetPosition(leisureLocation.position);
    }

    public override void UpdateState()
    {
        // Check if we reached our destination
        if (!hasReachedDestination && brain.Movement.HasReachedTarget())
        {
            hasReachedDestination = true;
        }

        if (hasReachedDestination)
        {
            timer += Time.deltaTime;

            // Recover social need while socializing
            var socialNeed = brain.NeedsManager.GetAllNeeds().FirstOrDefault(n => n.Name == "Social");
            if (socialNeed != null)
            {
                socialNeed.FulfillGradually(Time.deltaTime);
            }

            // After socializing for a while, let the behavior system decide what to do next
            if (timer >= socialDuration)
            {
                brain.DetermineNextAction();
            }
        }

        // If we can't reach the destination after some time, try a different behavior
        else if (timer > 10f) // 10 seconds of real time
        {
            brain.TransitionTo(new IdleState(brain));
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
