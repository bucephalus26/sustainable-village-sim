using UnityEngine;

public class Villager : MonoBehaviour {

    // Basic Attributes
    public string villagerName;
    public int age;
    public float health;
    public float personalWealth = 0f; // Inheritable wealth

    // Profession
    [Header("Profession Settings")]
    [SerializeField] private ProfessionData professionData;
    private ProfessionManager professionManager;

    [Header("Trait Settings")]
    [SerializeField] private TraitType trait;
    public enum TraitType { Hardworking, Lazy, Social, Loner }

    // Needs
    private ResourceManager resourceManager;
    private VillagerNeeds needs;
    private bool isFulfillingNeed = false;

    // Movement
    public Vector3 position;
    private WorkplaceFinder workplaceFinder;
    private Transform workplace;
    private Transform home;
    private VillagerMovement movement;

    void Start()
    {
        // Find ResourceManager instance
        resourceManager = ResourceManager.Instance;
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager not found in scene!");
        }

        // Initialise profession
        professionManager = gameObject.AddComponent<ProfessionManager>();
        professionManager.Initialize(this);
        professionManager.AssignProfession(professionData);

        workplaceFinder = gameObject.AddComponent<WorkplaceFinder>();
        movement = gameObject.AddComponent<VillagerMovement>();

        // Initialise needs
        needs = gameObject.AddComponent<VillagerNeeds>();
        needs.Initialize();

        // Find home (all villagers go home when not working)
        home = GameObject.FindGameObjectWithTag("Home").transform;
        workplace = workplaceFinder.FindWorkplace(professionData.type);

        // Start by moving to workplace
        if (workplace != null)
        {
            movement.SetTargetPosition(workplace.position);
        }
    }

    void Update() {
        // Update needs first
        needs.UpdateNeeds();

        // Check for urgent needs
        string urgentNeed = needs.GetMostUrgentNeed();
        if (urgentNeed != "None" && !isFulfillingNeed)
        {
            // Stop working and handle the need
            professionManager.HandleWork(false);
            HandleNeed(urgentNeed);
        }
        else if (!isFulfillingNeed)
        {
            // No urgent needs, go to work
            professionManager.HandleWork(true);
            if (!movement.TargetIsWorkplace)
            {
                HandleWork();
            }
        }

        // Handle movement and reaching destinations
        movement.MoveToTarget();
        if (movement.HasReachedTarget())
        {
            Debug.Log($"{name} reached target.");
            if (isFulfillingNeed)
            {
                FulfillNeed(urgentNeed); // Replenish needs when target is reached
                isFulfillingNeed = false;
                HandleWork();
            }
            else
            {
                Debug.Log($"{name} reached workplace.");
            }
        }
    }

    private void HandleNeed(string need)
    {
        isFulfillingNeed = true;
        movement.TargetIsWorkplace = false;

        switch (need)
        {
            case "Hunger":
                Transform restaurant = workplaceFinder.FindRestaurant();
                if (restaurant != null)
                {
                    movement.SetTargetPosition(restaurant.position);
                    Debug.Log($"{villagerName} is heading to restaurant.");
                }
                break;

            case "Rest":
                Transform home = workplaceFinder.FindHome();
                if (home != null)
                {
                    movement.SetTargetPosition(home.position);
                    Debug.Log($"{villagerName} is heading home to rest.");
                }
                break;
        }
    }

    private void FulfillNeed(string need)
    {
        switch (need)
        {
            case "Hunger":
                if (resourceManager.ConsumeFood(10f))
                { // Deduct 10 food
                    needs.Hunger = 100f; // Replenish hunger
                    Debug.Log($"{name} ate at the restaurant. Hunger replenished.");
                } 
                else 
                {
                    Debug.Log($"{name} couldn't eat. Not enough food in the village.");
                }
                break;
            case "Rest":
                needs.Rest = 100f; // Replenish rest
                Debug.Log($"{name} rested at home. Rest replenished.");
                break;
        }

        // Return to work after fulfilling need
        HandleWork();
    }

    private void HandleWork()
    {
        Transform workplace = workplaceFinder.FindWorkplace(professionData.type);
        if (workplace != null)
        {
            movement.SetTargetPosition(workplace.position);
            movement.TargetIsWorkplace = true;
            Debug.Log($"{name} is heading to work as {professionData.professionName}.");
        }
        else
        {
            Debug.LogWarning($"No workplace found for {name} ({professionData.professionName})");
        }
    }

    // Called when the villager works
    public void EarnWealth(float amount)
    {
        personalWealth += amount;
    }

}
