using UnityEngine;

public class VillagerNeeds : MonoBehaviour
{
    [Header("Thresholds")]
    [SerializeField] private float criticalHungerThreshold = 20f;
    [SerializeField] private float criticalRestThreshold = 20f;

    [Header("Current Needs")]
    [SerializeField] private float hunger;
    [SerializeField] private float rest;

    public void Initialize()
    {
        hunger = 100f;
        rest = 100f;
    }

    // Update needs over time
    public void UpdateNeeds()
    {
        hunger -= 0.1f * Time.deltaTime;
        rest -= 0.05f * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0, 100);
        rest = Mathf.Clamp(rest, 0, 100);
    }

    // Check which need is most urgent
    public string GetMostUrgentNeed()
    {
        if (hunger <= criticalHungerThreshold) return "Hunger";
        if (rest <= criticalRestThreshold) return "Rest";
        return "None";
    }

    public float Hunger { get => hunger; set => hunger = value; }
    public float Rest { get => rest; set => rest = value; }
    
}
