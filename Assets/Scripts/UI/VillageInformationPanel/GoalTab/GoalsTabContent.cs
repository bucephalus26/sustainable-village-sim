using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GoalsTabContent : MonoBehaviour
{
    [Header("Prefabs - Assign from Project Assets")]
    [SerializeField] private GameObject villagerSectionPrefab;
    [SerializeField] private GameObject villagerNameHeaderPrefab;
    [SerializeField] private GameObject goalItemPrefab;

    [Header("Goal Type Colors")]
    [SerializeField] private Color wealthGoalColor = new(0.945f, 0.769f, 0.059f);
    [SerializeField] private Color socialGoalColor = new(0.608f, 0.349f, 0.714f);
    [SerializeField] private Color masteryGoalColor = new(0.180f, 0.800f, 0.443f);
    [SerializeField] private Color contributorGoalColor = new(0.902f, 0.494f, 0.133f);
    [SerializeField] private Color defaultGoalColor = Color.grey;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 1.5f;
    private float updateTimer = 0f;

    private VillagerManager villagerManager;
    private List<GameObject> currentVillagerSections = new();
    private Dictionary<GoalType, Color> goalTypeColors;


    void Start()
    {
        villagerManager = VillagerManager.Instance;
        InitializeColorMap();

        if (gameObject.activeInHierarchy) UpdateGoalDisplay();
    }

    void OnEnable()
    {
        if (villagerManager != null) UpdateGoalDisplay();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateGoalDisplay();
        }
    }

    void InitializeColorMap()
    {
        goalTypeColors = new Dictionary<GoalType, Color>
        {
            { GoalType.AccumulateWealth, wealthGoalColor },
            { GoalType.SocialProminence, socialGoalColor },
            { GoalType.WorkMastery, masteryGoalColor },
            { GoalType.VillageContributor, contributorGoalColor }
        };
    }

    void UpdateGoalDisplay()
    {
        // Clear existing villager elements
        foreach (GameObject sectionGO in currentVillagerSections)
        {
            Destroy(sectionGO);
        }
        currentVillagerSections.Clear();

        // Get current villagers
        List<Villager> villagers = villagerManager.GetVillagers();
        if (villagers == null || villagers.Count == 0) return;

        // Iterate and create UI sections
        foreach (Villager villager in villagers.OrderBy(v => v.villagerName))
        {
            if (villager?.Brain?.Goals == null) continue;

            List<Goal> activeGoals = villager.Brain.Goals.GetActiveGoals();
            if (activeGoals == null || activeGoals.Count == 0) continue; 

            // Create the Parent Section for Villager
            GameObject sectionGO = Instantiate(villagerSectionPrefab, this.transform);
            Transform sectionTransform = sectionGO.transform; 
            sectionGO.name = $"Section_{villager.villagerName}";
            sectionGO.SetActive(true);
            currentVillagerSections.Add(sectionGO);


            // Add Villager Header
            GameObject headerGO = Instantiate(villagerNameHeaderPrefab, sectionTransform);
            TextMeshProUGUI headerText = headerGO.GetComponent<TextMeshProUGUI>();
            if (headerText != null)
            {
                string professionName = villager.Brain?.Profession?.ProfessionData?.professionName;
                headerText.text = $"{villager.villagerName} - {professionName}";
            }
            headerGO.SetActive(true);


            // Instantiate Goal Items
            foreach (Goal goal in activeGoals)
            {
                GameObject goalItemGO = Instantiate(goalItemPrefab, sectionTransform);
                GoalItemUI goalItemUI = goalItemGO.GetComponent<GoalItemUI>();
                if (goalItemUI != null)
                {
                    goalItemUI.Setup(goal, goalTypeColors.GetValueOrDefault(goal.type, defaultGoalColor));
                }
                goalItemGO.SetActive(true);
            }
        }
    }
}