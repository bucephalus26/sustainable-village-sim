using UnityEngine;

public class Villager : MonoBehaviour {

    // Basic Attributes
    public string villagerName;
    public int age;
    public float health;
    public float wealth;

    // Social Attributes
    public string profession;
    public string familyID;
    public bool isHardworking;
    public bool isSocial;

    // Needs
    public float foodNeed = 100f;
    public float restNeed = 100f;
    public float socialNeed = 100f;

    // Goals
    public enum Goal { AccumulateWealth, MakeFriends, RaiseFamily }
    public Goal currentGoal;

    // Update interval for actions
    private float actionInterval = 1f; // Villagers act every 1 second
    private float nextActionTime = 0f;

    void Start() {
        // Initialise default values
        if (string.IsNullOrEmpty(villagerName))
            villagerName = "Villager_" + Random.Range(1, 1000);
        if (profession == null)
            profession = "Unemployed";
        if (currentGoal == 0)
            currentGoal = Goal.AccumulateWealth;

        Debug.Log($"{villagerName} has spawned with profession: {profession} and goal: {currentGoal}");
    }

    void Update() {
        // Check if it's time to perform an action
        if (Time.time >= nextActionTime)
        {
            PerformDailyActions();
            nextActionTime = Time.time + actionInterval;
        }
    }

    private void PerformDailyActions() {
        ConsumeFood();
        Rest();
        Socialise();

        Debug.Log($"{villagerName} is working as a {profession}. Current needs: Food={foodNeed}, Rest={restNeed}, Social={socialNeed}");
    }

    private void ConsumeFood()
    {
        // Simulate food consumption
        foodNeed -= 10f; // Reduce need
        if (foodNeed <= 0f)
        {
            health -= 5f; // Penalty for starvation
            Debug.LogWarning($"{villagerName} is starving!");
        }
    }

    private void Rest() {
        // Simulate resting
        restNeed -= 5f; // Reduce need
        if (restNeed <= 0f)
        {
            health -= 2f; // Penalty for exhaustion
            Debug.LogWarning($"{villagerName} is exhausted!");
        }
    }

    private void Socialise() {
        // Simulate socialising (for now, just decrement)
        socialNeed -= 3f;
        if (socialNeed <= 0f)
        {
            health -= 1f; // Penalty for loneliness
            Debug.LogWarning($"{villagerName} is feeling lonely!");
        }
    }

}
