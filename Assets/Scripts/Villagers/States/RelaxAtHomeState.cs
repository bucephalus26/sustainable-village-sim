using UnityEngine;
using System.Linq;

public class RelaxAtHomeState : VillagerBaseState
{
    private float relaxDuration;
    private float timer;
    private bool hasReachedHome = false;

    public RelaxAtHomeState(VillagerBrain brain) : base(brain)
    {
        // Relax for 1-2 game hours
        relaxDuration = UnityEngine.Random.Range(1f, 2f) * 60f / TimeManager.Instance.TimeScaleFactor;
        timer = 0f;
    }

    public override void EnterState()
    {
        Debug.Log($"{brain.VillagerComponent.villagerName} deciding to relax at home.");
        Transform homeLocation = brain.LocationFinder.GetHomeLocation();
        if (homeLocation != brain.transform) // Check if already home
        {
            brain.Movement.SetTargetPosition(homeLocation.position);
            hasReachedHome = false;
        }
        else
        {
            hasReachedHome = true;
            brain.Movement.ClearTarget();
        }
    }

    public override void UpdateState()
    {
        // Check critical needs first
        var urgentNeed = brain.NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            brain.TransitionTo(new NeedFulfillmentState(brain, urgentNeed));
            return;
        }

        // Check arrival if moving
        if (!hasReachedHome && brain.Movement.HasReachedTarget())
        {
            hasReachedHome = true;
            brain.Movement.ClearTarget();
            Debug.Log($"{brain.VillagerComponent.villagerName} arrived home to relax.");
        }

        // If home, relax and recover slight rest
        if (hasReachedHome)
        {
            timer += Time.deltaTime;

            var restNeed = brain.NeedsManager?.GetAllNeeds().FirstOrDefault(n => n.Name == "Rest");
            if (restNeed != null)
            {
                //  slower than sleeping
                restNeed.FulfillGradually(Time.deltaTime, 5f);
            }

            // Check if relaxation duration is over or if no longer leisure time
            // DetermineNextAction pulls villager out if schedule changes
            bool isStillLeisure = brain.Profession.IsSocialHour() || TimeManager.Instance.CurrentTimeOfDay == TimeOfDay.Noon;

            if (timer >= relaxDuration || !isStillLeisure)
            {
                Debug.Log($"{brain.VillagerComponent.villagerName} finished relaxing at home.");
                brain.DetermineNextAction();
            }
        }
        else
        {
            timer += Time.deltaTime; 
            if (timer > 15f)
            {
                Debug.LogWarning($"{brain.VillagerComponent.villagerName} couldn't reach home to relax. Idling instead.");
                brain.TransitionTo(new IdleState(brain));
            }
        }
    }

    public override void ExitState() {}
}