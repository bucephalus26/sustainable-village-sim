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

        EventManager.Instance.TriggerEvent(new VillagerEvents.StateChangeEvent
        {
            VillagerName = services.VillagerComponent.villagerName,
            ProfessionType = services.ProfessionManager.GetProfessionType().ToString(),
            NewState = newState.GetType()
        }); 
    }

    void Update()
    {
        services.NeedsManager.UpdateNeeds();
        currentState?.UpdateState();
        services.Movement.MoveToTarget();
    }
}
