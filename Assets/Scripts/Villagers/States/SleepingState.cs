using System.Linq;
using UnityEngine;

public class SleepingState : VillagerBaseState
{
    private bool hasReachedHome = false;
    private float sleepDuration;
    private float timer = 0f;

    public SleepingState(VillagerBrain brain) : base(brain)
    {
        sleepDuration = UnityEngine.Random.Range(6f, 8f) * 60f / TimeManager.Instance.TimeScaleFactor;
    }

    public override void EnterState()
    {
        Transform home = brain.LocationFinder.GetHomeLocation();
        brain.Movement.SetTargetPosition(home.position);
    }

    public override void UpdateState()
    {
        // Check if we've reached home
        if (!hasReachedHome && brain.Movement.HasReachedTarget())
        {
            hasReachedHome = true;
        }

        if (hasReachedHome)
        {
            timer += Time.deltaTime;

            // Recover rest need while sleeping
            var restNeed = brain.NeedsManager.GetAllNeeds().FirstOrDefault(n => n.Name == "Rest");
            if (restNeed != null)
            {
                // Rest recovers faster than other needs
                restNeed.FulfillGradually(Time.deltaTime, 15f);
            }

            // If it's no longer sleeping hours and we've slept enough, wake up
            if (!brain.Profession.IsRestingHour() && timer >= sleepDuration)
            {
                brain.DetermineNextAction();
            }
        }
        else
        {
            // If we can't reach home after some time, just idle
            timer += Time.deltaTime;
            if (timer > 10f) // 10 seconds of real time
            {
                brain.TransitionTo(new IdleState(brain));
            }
        }
    }
}
