using UnityEngine;

public class VillagerProfession : MonoBehaviour
{
    private ProfessionData professionData;
    private Villager villager;
    private VillagerBrain brain;
    private SpriteRenderer spriteRenderer;
    private float workTimer;
    private bool isWorking;

    [SerializeField] private string currentProfession;
    [SerializeField] private float workProgress;
    [SerializeField] private float workEfficiency;

    public ProfessionType ProfessionType => professionData?.type ?? ProfessionType.Unemployed;
    public ProfessionData ProfessionData => professionData;

    public void Initialize(Villager villager)
    {
        this.villager = villager;
        brain = GetComponent<VillagerBrain>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void AssignProfession(ProfessionData data)
    {
        professionData = data;
        workTimer = 0f;
        isWorking = false;

        currentProfession = data != null ? data.professionName : "Unemployed";

        // Update appearance
        if (spriteRenderer != null && data != null)
        {
            spriteRenderer.color = data.spriteColor;
        }
    }

    public void Update()
    {
        if (isWorking && professionData != null)
        {
            // Get the work efficiency multiplier from mood system
            workEfficiency = 1.0f;
            if (brain?.VillagerMood != null)
            {
                workEfficiency = brain.VillagerMood.GetWorkEfficiencyMultiplier();
            }

            // Convert real time to game time for work timer, affected by efficiency
            workTimer += Time.deltaTime * TimeManager.Instance.TimeScaleFactor * workEfficiency;

            // Update inspector display
            workProgress = workTimer / professionData.workInterval;

            if (workTimer >= professionData.workInterval)
            {
                PerformWork();
                workTimer = 0f;
            }
        }
    }

    public void HandleWork(bool canWork)
    {
        isWorking = canWork;
    }

    private void PerformWork()
    {
        if (professionData == null) return;

        // Get current work efficiency
        float efficiency = 1.0f;
        if (brain?.VillagerMood != null)
        {
            efficiency = brain.VillagerMood.GetWorkEfficiencyMultiplier();
        }

        // Add resources to economy - adjusted based on work efficiency
        float adjustedOutput = professionData.resourceOutput;
        if (professionData.primaryResourceType == ResourceType.Food)
        {
            adjustedOutput *= 0.5f; // Reduce food production by 50%
        }

        // Apply work efficiency (happiness effect)
        adjustedOutput *= efficiency;

        if (professionData.primaryResourceType != ResourceType.None)
        {
            EconomyManager.Instance.AddResource(
                professionData.primaryResourceType,
                adjustedOutput);
        }

        // Add wealth to villager
        if (professionData.wealthGeneration > 0)
        {
            float adjustedWealth = professionData.wealthGeneration * efficiency;
            villager.EarnWealth(adjustedWealth);
        }

        // Trigger event
        EventManager.Instance.TriggerEvent(new VillagerEvents.ProfessionWorkCompletedEvent
        {
            VillagerName = villager.villagerName,
            ProfessionType = professionData.type,
            ResourcesProduced = adjustedOutput
        });
    }

    public bool IsWorkingHour()
    {
        if (professionData == null) return false;

        TimeOfDay currentTime = TimeManager.Instance.CurrentTimeOfDay;

        foreach (var workHour in professionData.workingHours)
        {
            if (workHour == currentTime)
                return true;
        }

        return false;
    }

    public bool IsSocialHour()
    {
        if (professionData == null) return false;

        TimeOfDay currentTime = TimeManager.Instance.CurrentTimeOfDay;

        foreach (var socialHour in professionData.socialHours)
        {
            if (socialHour == currentTime)
                return true;
        }

        return false;
    }

    public bool IsRestingHour()
    {
        if (professionData == null) return false;

        TimeOfDay currentTime = TimeManager.Instance.CurrentTimeOfDay;

        foreach (var restingHour in professionData.restingHours)
        {
            if (restingHour == currentTime)
                return true;
        }

        return false;
    }
}