using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class VillagerDetailPanelController : MonoBehaviour
{
    [Header("Header References")]
    [SerializeField] private TextMeshProUGUI villagerNameTitle;
    [SerializeField] private Button closeButton;

    [Header("Section Containers")]
    [SerializeField] private Transform statsGridContainer;
    [SerializeField] private Transform needsGridContainer;
    [SerializeField] private Transform traitsGridContainer;
    [SerializeField] private Transform goalsListContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject statItemPrefab;
    [SerializeField] private GameObject needItemPrefab;
    [SerializeField] private GameObject traitItemPrefab;
    [SerializeField] private GameObject goalItemPrefab;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    private float updateTimer = 0f;

    private Villager currentVillager;
    private CanvasGroup canvasGroup;
    private List<GameObject> instantiatedGoalItems = new();
    private List<GameObject> instantiatedStatItems = new();
    private List<GameObject> instantiatedNeedItems = new();
    private List<GameObject> instantiatedTraitItems = new();

    // Need/Trait Colors
    private Color hungerColor = new(0.902f, 0.494f, 0.133f);
    private Color restColor = new(0.608f, 0.349f, 0.714f);
    private Color socialColor = new(0.204f, 0.596f, 0.859f);
    private Color traitColor = new(0.204f, 0.596f, 0.859f);

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        closeButton?.onClick.AddListener(OnCloseButtonClicked);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || currentVillager == null)
        {
            if (!gameObject.activeInHierarchy) updateTimer = updateInterval;
            return;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            PopulateAllSections();
        }
    }

    public void ShowPanel(Villager villager)
    {
        if (villager == null) return;
        currentVillager = villager;

        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        PopulateAllSections();
    }

    public void HidePanel()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f; // Optional fade out
        gameObject.SetActive(false);
        currentVillager = null;
    }

    void OnCloseButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideVillagerDetailPanel();
        }
        else
        {
            HidePanel(); // Fallback if UIManager missing
        }
    }

    void PopulateAllSections()
    {
        if (currentVillager == null) return;

        villagerNameTitle.text = currentVillager.villagerName;

        PopulateBasicInfo();
        PopulateNeeds();
        PopulatePersonality();
        PopulateGoals();
    }

    void ClearSection(List<GameObject> itemList, Transform container)
    {
        if (container == null) return;
        foreach (GameObject item in itemList) Destroy(item);
        itemList.Clear();
    }

    // Population Methods

    void PopulateBasicInfo()
    {
        ClearSection(instantiatedStatItems, statsGridContainer);
        if (currentVillager == null || statsGridContainer == null || statItemPrefab == null) return;

        // Create stat items
        CreateStatItem("Age", currentVillager.age.ToString());
        string profName = currentVillager.Brain?.Profession?.ProfessionData?.professionName;
        CreateStatItem("Profession", profName);
        CreateStatItem("Health", $"{currentVillager.health:F0}%");
        CreateStatItem("Happiness", $"{currentVillager.happiness:F0}%");
        CreateStatItem("Personal Wealth", currentVillager.personalWealth.ToString("F1"));
        string activity = currentVillager.Brain?.CurrentState?.GetType().Name.Replace("State", "") ?? "Idle";
        CreateStatItem("Current Activity", currentVillager.CurrentActivityDescription);
    }

    void CreateStatItem(string label, string value)
    {
        GameObject itemGO = Instantiate(statItemPrefab, statsGridContainer);
        DetailStatItemUI ui = itemGO.GetComponent<DetailStatItemUI>();
        if (ui != null) ui.Setup(label, value);
        itemGO.SetActive(true);
        instantiatedStatItems.Add(itemGO);
    }


    void PopulateNeeds()
    {
        ClearSection(instantiatedNeedItems, needsGridContainer);
        if (currentVillager?.Brain?.NeedsManager == null || needsGridContainer == null || needItemPrefab == null) return;

        var needs = currentVillager.Brain.NeedsManager.GetAllNeeds();

        Need hunger = needs.FirstOrDefault(n => n.Name == "Hunger");
        Need rest = needs.FirstOrDefault(n => n.Name == "Rest");
        Need social = needs.FirstOrDefault(n => n.Name == "Social");

        if (hunger != null) CreateNeedItem("Hunger", hunger.CurrentValue, hungerColor);
        if (rest != null) CreateNeedItem("Rest", rest.CurrentValue, restColor);
        if (social != null) CreateNeedItem("Social", social.CurrentValue, socialColor);
    }

    void CreateNeedItem(string label, float value, Color barColor)
    {
        GameObject itemGO = Instantiate(needItemPrefab, needsGridContainer);
        DetailNeedItemUI ui = itemGO.GetComponent<DetailNeedItemUI>();
        if (ui != null) ui.Setup(label, value / 100f, barColor); // Pass value as 0-1 ratio
        itemGO.SetActive(true);
        instantiatedNeedItems.Add(itemGO);
    }


    void PopulatePersonality()
    {
        ClearSection(instantiatedTraitItems, traitsGridContainer);
        if (currentVillager?.Brain?.Personality == null || traitsGridContainer == null || traitItemPrefab == null) return;

        var p = currentVillager.Brain.Personality;

        CreateTraitItem("Sociability", p.sociability, socialColor); 
        CreateTraitItem("Work Ethic", p.workEthic, new(0.180f, 0.800f, 0.443f));
        CreateTraitItem("Resilience", p.resilience, new(0.945f, 0.769f, 0.059f));
        CreateTraitItem("Impulsivity", p.impulsivity, new(0.906f, 0.298f, 0.235f));
        CreateTraitItem("Optimism", p.optimism, restColor);
        CreateTraitItem("Ambition", p.ambition, hungerColor);
        CreateTraitItem("Altruism", p.altruism, new(1.0f, 0.992f, 0.816f, 0.784f));
    }

    void CreateTraitItem(string label, float value, Color barColor)
    {
        GameObject itemGO = Instantiate(traitItemPrefab, traitsGridContainer);
        PersonalityTraitItemUI ui = itemGO.GetComponent<PersonalityTraitItemUI>();
        if (ui != null) ui.Setup(label, value, barColor); // Pass value as 0-1 ratio
        itemGO.SetActive(true);
        instantiatedTraitItems.Add(itemGO);
    }


    void PopulateGoals()
    {
        ClearSection(instantiatedGoalItems, goalsListContainer);
        if (currentVillager?.Brain?.Goals == null || goalsListContainer == null || goalItemPrefab == null) return;

        List<Goal> activeGoals = currentVillager.Brain.Goals.GetActiveGoals();
        if (activeGoals == null || activeGoals.Count == 0) return;

        Color defaultColor = Color.grey;
        Dictionary<GoalType, Color> goalTypeColors = new() {};
        goalTypeColors[GoalType.AccumulateWealth] = new(0.945f, 0.769f, 0.059f);
        goalTypeColors[GoalType.SocialProminence] = new(0.608f, 0.349f, 0.714f);
        goalTypeColors[GoalType.WorkMastery] = new(0.180f, 0.800f, 0.443f);
        goalTypeColors[GoalType.VillageContributor] = new(0.902f, 0.494f, 0.133f);


        foreach (Goal goal in activeGoals)
        {
            GameObject goalItemGO = Instantiate(goalItemPrefab, goalsListContainer);
            GoalItemUI goalItemUI = goalItemGO.GetComponent<GoalItemUI>();
            if (goalItemUI != null)
            {
                Color goalColor = goalTypeColors.GetValueOrDefault(goal.type, defaultColor);
                goalItemUI.Setup(goal, goalColor);
            }
            goalItemGO.SetActive(true);
            instantiatedGoalItems.Add(goalItemGO);
        }
    }
}