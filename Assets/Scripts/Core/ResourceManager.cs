using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Singleton instance
    public static ResourceManager Instance { get; private set; }

    // Village resources
    public float food = 100f;
    public float communalWealth = 100f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    // Deduct food when villagers eat
    public bool ConsumeFood(float amount)
    {
        if (food >= amount)
        {
            food -= amount;
            return true;
        }
        return false; // Not enough food
    }

    // Add food/wealth (used by professions)
    public void AddFood(float amount) => food += amount;
    public void AddCommunalWealth(float amount) => communalWealth += amount;
}