using UnityEngine;

public interface IVillagerState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public abstract class VillagerBaseState : IVillagerState
{
    protected VillagerBrain brain;

    public VillagerBaseState(VillagerBrain brain)
    {
        this.brain = brain;
    }

    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void ExitState() { }
}
