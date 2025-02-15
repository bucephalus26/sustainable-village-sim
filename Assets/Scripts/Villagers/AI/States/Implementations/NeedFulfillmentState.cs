using UnityEngine;

public class NeedFulfillmentState : VillagerBaseState
{
    private readonly Need currentNeed;

    public NeedFulfillmentState(VillagerContext context, VillagerServices services, Need need)
        : base(context, services)
    {
        currentNeed = need;
    }

    public override void EnterState()
    {
        Transform location = services.WorkplaceFinder.GetNeedLocation(currentNeed);
        services.Movement.SetTargetPosition(location.position);
    }

    public override void UpdateState()
    {
        if (services.Movement.HasReachedTarget())
        {
            services.NeedsManager.FulfillNeed(currentNeed);
            context.TransitionTo(new WorkingState(context, services));
        }
    }

    public override void ExitState() { }
}
