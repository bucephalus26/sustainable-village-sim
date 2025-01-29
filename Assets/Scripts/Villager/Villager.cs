using UnityEngine;

public class Villager : MonoBehaviour {

    // Basic Attributes
    public string villagerName;
    public int age;
    public float health;
    public float wealth;

    // Profession Attributes
    public IProfession profession;
    public enum ProfessionType {Farmer, Shopkeeper, Priest, Unemployed}
    [Header("Profession Settings")]
    public ProfessionType professionType;

    public string trait;  // Traits (e.g., hardworking, lazy, social, loner)

    // Needs
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

        // Initialise profession
        AssignProfession(professionType);
        workplaceFinder = gameObject.AddComponent<WorkplaceFinder>();
        movement = gameObject.AddComponent<VillagerMovement>();

        // Initialise needs
        needs = gameObject.AddComponent<VillagerNeeds>();
        needs.Initialise();

        // Find home (all villagers go home when not working)
        home = GameObject.FindGameObjectWithTag("Home").transform;
        workplace = workplaceFinder.FindWorkplace(professionType);

        // Start by moving to workplace
        if (workplace != null)
        {
            movement.SetTargetPosition(workplace.position);
        }
    }

    void Update() {
        needs.UpdateNeeds();

        string urgentNeed = needs.GetMostUrgentNeed();
        if (urgentNeed != "None" && !isFulfillingNeed)
        {
            HandleNeed(urgentNeed);
        }
        else if (!isFulfillingNeed)
        {
            HandleWork(); // Default to work if no urgent needs
        }

        movement.MoveToTarget();
        if (movement.HasReachedTarget())
        {
            Debug.Log($"{name} reached target.");
            if (isFulfillingNeed)
            {
                FulfillNeed(urgentNeed); // Replenish needs when target is reached
            }
            isFulfillingNeed = false; // Reset need fulfillment flag
        }


        // Work
        if (profession != null && !isFulfillingNeed)
        { // Only work if not fulfilling a need
            profession.Work();
        }
        else if (profession == null)
        {
            Debug.Log($"{name} is idle, no profession assigned.");
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
                movement.SetTargetPosition(restaurant.position);
                break;
            case "Rest":
                Transform home = workplaceFinder.FindHome();
                movement.SetTargetPosition(home.position);
                break;
        }
    }

    private void FulfillNeed(string need)
    {
        switch (need)
        {
            case "Hunger":
                needs.Hunger = 100f; // Replenish hunger
                Debug.Log($"{name} ate at the restaurant. Hunger replenished.");
                break;
            case "Rest":
                needs.Rest = 100f; // Replenish rest
                Debug.Log($"{name} rested at home. Rest replenished.");
                break;
        }

        // Return to work after fulfilling the need
        HandleWork();
    }

    private void HandleWork()
    {
        Transform workplace = workplaceFinder.FindWorkplace(professionType);
        movement.SetTargetPosition(workplace.position);
    }

    private void AssignProfession(ProfessionType type)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        switch (type)
        {
            case ProfessionType.Farmer:
                profession = gameObject.AddComponent<Farmer>();
                spriteRenderer.color = Color.green;
                break;
            case ProfessionType.Shopkeeper:
                profession = gameObject.AddComponent<Shopkeeper>();
                spriteRenderer.color = Color.red;
                break;
            case ProfessionType.Priest:
                profession = gameObject.AddComponent<Priest>();
                spriteRenderer.color = Color.blue;
                break;
            default:
                profession = null;
                spriteRenderer.color = Color.gray;
                break;
        }
    }

    public string GetProfession()
    {
        return profession?.GetProfessionName() ?? "Unemployed";
    }


    //private void PerformDailyActions()
    //{
    //    // Perform profession-specific actions
    //    switch (profession)
    //    {
    //        case "Farmer":
    //            Farm();
    //            break;
    //        case "Blacksmith":
    //            Forge();
    //            break;
    //        case "Shopkeeper":
    //            RunShop();
    //            break;
    //        default:
    //            Debug.Log($"{villagerName} is idle, no profession assigned.");
    //            break;
    //    }

    //    // Update needs
    //    ConsumeFood();
    //    Rest();

    //    Debug.Log($"{villagerName} ({profession}) is working. Needs: Food={foodNeed}, Rest={restNeed}");
    //}

    //private void ConsumeFood()
    //{
    //    // Simulate food consumption
    //    foodNeed -= 10f; // Reduce need
    //    if (foodNeed <= 0f)
    //    {
    //        health -= 5f; // Penalty for starvation
    //        Debug.LogWarning($"{villagerName} is starving!");
    //    }
    //}

    //private void Rest() {
    //    // Simulate resting
    //    restNeed -= 5f; // Reduce need
    //    if (restNeed <= 0f)
    //    {
    //        health -= 2f; // Penalty for exhaustion
    //        Debug.LogWarning($"{villagerName} is exhausted!");
    //    }
    //}

    //private void Farm()
    //{
    //    Debug.Log($"{villagerName} is farming and earning wealth.");
    //    wealth += 10f; // Increase wealth
    //}

    //private void Forge()
    //{
    //    Debug.Log($"{villagerName} is forging tools.");
    //    wealth += 15f; // Increase wealth slightly faster
    //}

    //private void RunShop()
    //{
    //    Debug.Log($"{villagerName} is selling goods.");
    //    wealth += 20f; // Shopkeepers earn the most wealth
    //}
}
