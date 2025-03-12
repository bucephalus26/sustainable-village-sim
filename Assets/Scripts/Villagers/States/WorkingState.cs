using UnityEngine;

public class WorkingState : VillagerBaseState
{
    private bool hasReachedWorkplace = false;
    private float workDuration;
    private float timer = 0f;

    public WorkingState(VillagerBrain brain) : base(brain)
    {
        // Work for a significant amount of game time
        workDuration = UnityEngine.Random.Range(3f, 5f) * 60f / TimeManager.Instance.TimeScaleFactor;
    }

    public override void EnterState()
    {
        brain.Movement.SetTargetPosition(brain.LocationFinder.GetCurrentWorkplace().position);
    }

    public override void UpdateState()
    {
        // Check if we've reached workplace
        if (!hasReachedWorkplace && brain.Movement.HasReachedTarget())
        {
            hasReachedWorkplace = true;
            brain.Profession.HandleWork(true);
        }

        if (hasReachedWorkplace)
        {
            timer += Time.deltaTime;

            // Check for urgent needs while working
            var urgentNeed = brain.NeedsManager.GetMostUrgentNeed();
            if (urgentNeed != null && urgentNeed.GetUrgency() > 0.7f)
            {
                brain.TransitionTo(new NeedFulfillmentState(brain, urgentNeed));
                return;
            }

            // If it's no longer working hours or we've worked enough, stop working
            if (!brain.Profession.IsWorkingHour() || timer >= workDuration)
            {
                brain.DetermineNextAction();
            }
        }
        else
        {
            // If we can't reach workplace after some time, just idle
            timer += Time.deltaTime;
            if (timer > 10f) // 10 seconds of real time
            {
                brain.TransitionTo(new IdleState(brain));
            }
        }
    }

    public override void ExitState()
    {
        brain.Profession.HandleWork(false);
    }
}
