using UnityEngine;

public class VillagerPersonality : MonoBehaviour
{
    // Core personality traits (values between 0-1)
    [Header("Personality Traits")]
    [Range(0f, 1f)] public float sociability = 0.5f;    // Affects social needs decay and fulfillment
    [Range(0f, 1f)] public float workEthic = 0.5f;      // Affects productivity and work satisfaction
    [Range(0f, 1f)] public float resilience = 0.5f;     // Affects how quickly needs become urgent
    [Range(0f, 1f)] public float impulsivity = 0.5f;    // Affects decision making and routine adherence
    [Range(0f, 1f)] public float optimism = 0.5f;       // Affects baseline happiness

    // Additional personality factors
    [Header("Goal-Related Traits")]
    [Range(0f, 1f)] public float ambition = 0.5f;       // Affects number and difficulty of goals
    [Range(0f, 1f)] public float altruism = 0.5f;       // Affects preference for village-helping goals

    private Villager villager;

    private void Awake()
    {
        villager = GetComponent<Villager>();
    }

    // Initialise with random traits
    public void Initialize(bool randomize = true)
    {
        if (randomize)
        {
            // Create balanced personalities with some variance
            sociability = Random.Range(0.3f, 0.8f);
            workEthic = Random.Range(0.4f, 0.9f);
            resilience = Random.Range(0.3f, 0.8f);
            impulsivity = Random.Range(0.1f, 0.7f);
            optimism = Random.Range(0.3f, 0.8f);

            // Goal traits
            ambition = Random.Range(0.2f, 0.9f);
            altruism = Random.Range(0.2f, 0.8f);
        }

        // Log personality for debugging
        Debug.Log($"{GetComponent<Villager>()?.villagerName} personality: " +
                  $"Sociability={sociability:F2}, WorkEthic={workEthic:F2}, " +
                  $"Resilience={resilience:F2}, Impulsivity={impulsivity:F2}, " +
                  $"Optimism={optimism:F2}, Ambition={ambition:F2}, Altruism={altruism:F2}");
    }

    public float GetNeedDecayMultiplier(Need need)
    {
        switch (need.Name)
        {
            case "Social":
                // Less sociable people get lonely faster
                return 1.0f + (0.5f - sociability) * 0.5f;

            case "Rest":
                // Less resilient people tire faster
                return 1.0f + (0.5f - resilience) * 0.5f;

            case "Hunger":
                // More impulsive people get hungry faster
                return 1.0f + (impulsivity - 0.5f) * 0.3f;

            default:
                return 1.0f;
        }
    }

    // Get a blended personality trait for specific decisions
    public float GetTraitBlend(float primaryTrait, float secondaryTrait, float primaryWeight = 0.7f)
    {
        return (primaryTrait * primaryWeight) + (secondaryTrait * (1 - primaryWeight));
    }

    // Print personality profile (debugging/visualization)
    public string GetPersonalityProfile()
    {
        return $"{villager.villagerName} Personality Profile:\n" +
               $"Sociability: {GetTraitDescription(sociability)}\n" +
               $"Work Ethic: {GetTraitDescription(workEthic)}\n" +
               $"Resilience: {GetTraitDescription(resilience)}\n" +
               $"Impulsivity: {GetTraitDescription(impulsivity)}\n" +
               $"Ambition: {GetTraitDescription(ambition)}\n" +
               $"Altruism: {GetTraitDescription(altruism)}";
    }

    private string GetTraitDescription(float traitValue)
    {
        if (traitValue < 0.3f) return "Low";
        if (traitValue < 0.7f) return "Medium";
        return "High";
    }
}