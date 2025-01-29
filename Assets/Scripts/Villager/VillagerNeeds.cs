using UnityEngine;

public class VillagerNeeds : MonoBehaviour
{
    // Needs thresholds
    [SerializeField] private float criticalHungerThreshold = 20f;
    [SerializeField] private float criticalRestThreshold = 20f;

    // Current needs
    public float Hunger { get; set; }
    public float Rest { get; set; }

    public void Initialise()
    {
        Hunger = 100f;
        Rest = 100f;
    }

    // Update needs over time
    public void UpdateNeeds()
    {
        Hunger -= 0.1f * Time.deltaTime;
        Rest -= 0.05f * Time.deltaTime;

        Hunger = Mathf.Clamp(Hunger, 0, 100);
        Rest = Mathf.Clamp(Rest, 0, 100);
    }

    // Check which need is most urgent
    public string GetMostUrgentNeed()
    {
        if (Hunger <= criticalHungerThreshold) return "Hunger";
        if (Rest <= criticalRestThreshold) return "Rest";
        return "None";
    }

}
