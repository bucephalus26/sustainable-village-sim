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
    [SerializeField] public float hunger = 100f;
    [SerializeField] public float rest = 100f;

    public Vector3 position; // Spawn position
    private VillagerMovement movement;

    void Start()
    {

        // Initialise profession
        AssignProfession(professionType);

        movement = gameObject.AddComponent<VillagerMovement>();
        SetRandomTargetPosition();
    }


    void Update() {
        movement.MoveToTarget();
        if (movement.HasReachedTarget())
        {
            SetRandomTargetPosition();
        }

        // Decrease needs over time
        hunger -= 0.1f * Time.deltaTime;
        rest -= 0.05f * Time.deltaTime;

        // Clamp needs to avoid negative values
        hunger = Mathf.Clamp(hunger, 0, 100);
        rest = Mathf.Clamp(rest, 0, 100);

        // Work
        if (profession != null)
        {
            profession.Work();
        } else
        {
            Debug.Log($"{name} is idle, no profession assigned.");
        }

    }

    private void SetRandomTargetPosition()
    {
        Vector3 newTarget = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
        movement.SetTargetPosition(newTarget);
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
