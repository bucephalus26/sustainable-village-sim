using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillagerManager : MonoBehaviour
{
    public static VillagerManager Instance { get; private set; }
    private Transform villagerContainer;

    [Header("Villager Configuration")]
    [SerializeField] private GameObject villagerPrefab;
    [SerializeField] private int initialVillagerCount = 15;
    [SerializeField] private List<ProfessionData> availableProfessions;

    [Header("Village Demographics")]
    [SerializeField] [Range(16, 30)] private int youngAdultMinAge = 18;
    [SerializeField] [Range(30, 45)] private int middleAgeMinAge = 30;
    [SerializeField] [Range(45, 65)] private int olderAdultMinAge = 50;
    [SerializeField] [Range(65, 90)] private int elderlyMinAge = 65;

    [Header("Profession Distribution")]
    [SerializeField] private int farmerCount = 8;
    [SerializeField] private int shopkeeperCount = 2;
    [SerializeField] private int priestCount = 1;
    [SerializeField] private int craftsmanCount = 2;
    [SerializeField] private int cookCount = 2;

    private List<Villager> villagers = new();
    private Dictionary<string, List<string>> familyRelationships = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Give time for buildings to register first
        Invoke("InitializeVillagers", 0.5f);
    }

    private void InitializeVillagers()
    {
        if (villagerContainer == null)
        {
            GameObject containerObj = new("Villagers");
            villagerContainer = containerObj.transform;
        }


        List<Building> homes = BuildingManager.Instance.GetBuildingsByType(BuildingType.Home);
        if (homes.Count == 0)
        {
            Debug.LogError("No homes found in the scene. Cannot initialize villagers.");
            return;
        }

        // Create distribution list for professions
        List<ProfessionType> professionDistribution = GetProfessionDistribution();

        // Create family units
        CreateFamilyRelationships(initialVillagerCount);

        // Spawn villagers
        for (int i = 0; i < initialVillagerCount; i++)
        {
            if (i >= professionDistribution.Count) break;

            // Find profession data
            ProfessionType type = professionDistribution[i];
            ProfessionData profData = availableProfessions.Find(p => p.type == type);

            if (profData == null)
            {
                Debug.LogWarning($"No profession data found for {type}. Skipping villager.");
                continue;
            }

            // Find home for villager
            Building homeBuilding = homes[i % homes.Count];
            if (homeBuilding == null)
            {
                Debug.LogWarning("Building is not a Home. Skipping villager.");
                continue;
            }

            // Generate villager info
            string villagerName = GenerateRandomName();
            int villagerAge = GenerateAge();

            // Spawn the villager at the home entrance
            Vector3 spawnPosition = homeBuilding.GetEntrancePoint().position;
            GameObject villagerObj = Instantiate(villagerPrefab, spawnPosition, Quaternion.identity);
            villagerObj.transform.SetParent(villagerContainer);

            // Configure the villager
            Villager villagerComponent = villagerObj.GetComponent<Villager>();
            if (villagerComponent != null)
            {
                // Set basic attributes
                villagerComponent.villagerName = villagerName;
                villagerComponent.age = villagerAge;

                // Initialize with profession
                villagerComponent.Initialize(profData);

                // Assign to home
                homeBuilding.AddResident(villagerComponent);

                // Find and assign workplace
                BuildingType workplaceType = GetWorkplaceTypeForProfession(type);
                List<Building> workplaces = BuildingManager.Instance.GetBuildingsByType(workplaceType);

                if (workplaces.Count > 0)
                {
                    Building workplace = workplaces[i % workplaces.Count];
                    workplace.AssignWorker(villagerComponent);
                }

                // Add to list
                villagers.Add(villagerComponent);
            }
        }

        EventManager.Instance.TriggerEvent(new VillagerEvents.AllVillagersInitializedEvent { VillagerCount = villagers.Count });

        Debug.Log($"Initialized {villagers.Count} villagers in the village.");
    }

    private List<ProfessionType> GetProfessionDistribution()
    {
        List<ProfessionType> professionDistribution = new();

        for (int i = 0; i < farmerCount; i++) professionDistribution.Add(ProfessionType.Farmer);
        for (int i = 0; i < shopkeeperCount; i++) professionDistribution.Add(ProfessionType.Shopkeeper);
        for (int i = 0; i < priestCount; i++) professionDistribution.Add(ProfessionType.Priest);
        for (int i = 0; i < craftsmanCount; i++) professionDistribution.Add(ProfessionType.Craftsman);
        for (int i = 0; i < cookCount; i++) professionDistribution.Add(ProfessionType.Cook);

        // Ensure we have enough professions in our distribution
        if (professionDistribution.Count < initialVillagerCount)
        {
            // Add more farmers to fill in the gaps
            int remaining = initialVillagerCount - professionDistribution.Count;
            for (int i = 0; i < remaining; i++) professionDistribution.Add(ProfessionType.Farmer);
        }

        // Shuffle the distribution
        ShuffleList(professionDistribution);
        return professionDistribution;
    }

    private BuildingType GetWorkplaceTypeForProfession(ProfessionType type)
    {
        return type switch
        {
            ProfessionType.Farmer => BuildingType.Farm,
            ProfessionType.Shopkeeper => BuildingType.Shop,
            ProfessionType.Priest => BuildingType.Church,
            ProfessionType.Craftsman => BuildingType.Workshop,
            ProfessionType.Cook => BuildingType.Restaurant,
            _ => BuildingType.Home
        };
    }

    private string GenerateRandomName()
    {
        string[] firstNames = { "John", "Mary", "Robert", "Emma", "William", "Olivia", "James", "Sophia", "Thomas", "Isabella" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Miller", "Davis", "Wilson", "Taylor", "Clark" };

        return $"{firstNames[Random.Range(0, firstNames.Length)]} {lastNames[Random.Range(0, lastNames.Length)]}";
    }

    private int GenerateAge()
    {
        // Generate age based on demographic distribution
        float randomValue = Random.value;

        if (randomValue < 0.4f)
        {
            // Young adults (40% of population)
            return Random.Range(youngAdultMinAge, middleAgeMinAge);
        }
        else if (randomValue < 0.75f)
        {
            // Middle aged (35% of population)
            return Random.Range(middleAgeMinAge, olderAdultMinAge);
        }
        else if (randomValue < 0.95f)
        {
            // Older adults (20% of population)
            return Random.Range(olderAdultMinAge, elderlyMinAge);
        }
        else
        {
            // Elderly (5% of population)
            return Random.Range(elderlyMinAge, 90);
        }
    }

    private void CreateFamilyRelationships(int villagerCount)
    {
        // For future expansion: create family structures
        // Currently this just collects names, but could be enhanced to create
        // family units with specific relationships
        familyRelationships.Clear();
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public List<Villager> GetVillagers()
    {
        // Filters out any destroyed villagers
        villagers.RemoveAll(item => item == null);
        return villagers;
    }

    public float GetAverageHappiness()
    {
        var validVillagers = GetVillagers();
        if (!validVillagers.Any()) return 50f;

        return validVillagers.Average(v => v.happiness);
    }

    public ProfessionData GetProfessionData(ProfessionType type)
    {
        if (availableProfessions == null || type == ProfessionType.Unemployed)
        {
            return null;
        }

        return availableProfessions.FirstOrDefault(p => p.type == type);
    }

}
