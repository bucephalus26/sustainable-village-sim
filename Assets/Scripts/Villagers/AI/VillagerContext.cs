using UnityEngine;

// Main state manager
public class VillagerContext : MonoBehaviour
{
    private IVillagerState currentState;
    private VillagerServices services;

    public void Initialize(ProfessionData professionData)
    {
        services = gameObject.AddComponent<VillagerServices>();
        services.Initialize(professionData);

        TransitionTo(new WorkingState(this, services));
    }

    public void TransitionTo(IVillagerState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();

        Debug.Log($"Villager {services.VillagerComponent.villagerName} ({services.ProfessionManager.GetProfessionType()}) transitioning to {newState.GetType().Name}.");
    }

    void Update()
    {
        services.NeedsManager.UpdateNeeds();
        currentState?.UpdateState();
        services.Movement.MoveToTarget();
    }
}
