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
    [Header("Secondary Traits")]
    [Range(0f, 1f)] public float religiosity = 0.5f;    // Affects church attendance
    [Range(0f, 1f)] public float frugality = 0.5f;      // Affects spending/saving behavior
    [Range(0f, 1f)] public float altruism = 0.5f;       // Affects willingness to help others

    private Villager villager;

    private void Awake()
    {
        villager = GetComponent<Villager>();
    }

    // Initialize with random traits
    public void InitializePersonality(bool randomize = true)
    {
        if (randomize)
        {
            // Create somewhat balanced personalities with some variance
            sociability = Random.Range(0.3f, 0.8f);
            workEthic = Random.Range(0.4f, 0.9f);
            resilience = Random.Range(0.3f, 0.8f);
            impulsivity = Random.Range(0.1f, 0.7f);
            optimism = Random.Range(0.3f, 0.8f);

            // Secondary traits
            religiosity = Random.Range(0.2f, 0.9f);
            frugality = Random.Range(0.3f, 0.9f);
            altruism = Random.Range(0.2f, 0.8f);
        }

        // Log personality for debugging
        Debug.Log($"{GetComponent<Villager>()?.villagerName} personality: " +
                  $"Sociability={sociability:F2}, WorkEthic={workEthic:F2}, " +
                  $"Resilience={resilience:F2}, Impulsivity={impulsivity:F2}, " +
                  $"Optimism={optimism:F2}");
    }

    // Methods to influence state decisions based on personality
    public bool WillSkipWork()
    {
        // Higher work ethic and resilience makes skipping work less likely
        return Random.value > (workEthic * resilience);
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

    // Get desire to attend church based on religiosity
    public float GetChurchDesire()
    {
        return religiosity * 2.0f; // Double influence for religious activities
    }

    // Get chance of helping another villager based on altruism
    public float GetHelpingChance()
    {
        return altruism;
    }

    // Get tendency to save or spend resources based on frugality
    public float GetSavingTendency()
    {
        return frugality;
    }

    // Get a blended personality trait for specific decisions
    public float GetTraitBlend(float primaryTrait, float secondaryTrait, float primaryWeight = 0.7f)
    {
        return (primaryTrait * primaryWeight) + (secondaryTrait * (1 - primaryWeight));
    }

    // Print personality profile (for debugging/visualization)
    public string GetPersonalityProfile()
    {
        return $"{villager.villagerName} Personality Profile:\n" +
               $"Sociability: {GetTraitDescription(sociability)}\n" +
               $"Work Ethic: {GetTraitDescription(workEthic)}\n" +
               $"Resilience: {GetTraitDescription(resilience)}\n" +
               $"Impulsivity: {GetTraitDescription(impulsivity)}\n" +
               $"Religiosity: {GetTraitDescription(religiosity)}\n" +
               $"Frugality: {GetTraitDescription(frugality)}\n" +
               $"Altruism: {GetTraitDescription(altruism)}";
    }

    private string GetTraitDescription(float traitValue)
    {
        if (traitValue < 0.3f) return "Low";
        if (traitValue < 0.7f) return "Medium";
        return "High";
    }
}