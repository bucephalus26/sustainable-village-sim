using UnityEngine;

// Consolidates all dependencies
public class VillagerServices : MonoBehaviour
{
    public Villager VillagerComponent { get; private set; }
    public VillagerMovement Movement { get; private set; }
    public WorkplaceFinder WorkplaceFinder { get; private set; }
    public ProfessionManager ProfessionManager { get; private set; }
    public INeedsManager NeedsManager { get; private set; }

    public void Initialize(ProfessionData professionData)
    {
        VillagerComponent = GetComponent<Villager>();

        // Add components first
        Movement = gameObject.AddComponent<VillagerMovement>();
        WorkplaceFinder = gameObject.AddComponent<WorkplaceFinder>();
        ProfessionManager = gameObject.AddComponent<ProfessionManager>();

        // Then initialize them
        ProfessionManager.Initialize(VillagerComponent);
        ProfessionManager.AssignProfession(professionData);

        NeedsManager = new NeedsManager(this);
    }
}
