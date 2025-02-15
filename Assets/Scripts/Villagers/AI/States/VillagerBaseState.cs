using UnityEngine;

public abstract class VillagerBaseState : IVillagerState
{
    protected readonly VillagerContext context;
    protected readonly VillagerServices services;

    protected VillagerBaseState(VillagerContext context, VillagerServices services)
    {
        this.context = context;
        this.services = services;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}
