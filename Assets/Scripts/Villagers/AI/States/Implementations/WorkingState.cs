using UnityEngine;

public class WorkingState : VillagerBaseState
{
    public WorkingState(VillagerContext context, VillagerServices services)
        : base(context, services) { }

    public override void EnterState()
    {
        services.Movement.SetTargetPosition(services.WorkplaceFinder.GetCurrentWorkplace().position);
        services.ProfessionManager.HandleWork(true);
    }

    public override void UpdateState()
    {
        var urgentNeed = services.NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            context.TransitionTo(new NeedFulfillmentState(context, services, urgentNeed));
        }
    }

    public override void ExitState()
    {
        services.ProfessionManager.HandleWork(false);
    }

}
