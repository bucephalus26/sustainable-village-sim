using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Simulation Settings")]
    [SerializeField] private float simulationSpeed = 1.0f;
    [SerializeField] private bool pauseSimulation = false;

    [Header("Simulation Parameters")]
    [SerializeField] private bool enableWeather = false;
    [SerializeField] private bool enableDisease = false;
    [SerializeField] private bool enableAging = false;

    // Managers
    private TimeManager timeManager;
    private EconomyManager economyManager;
    private BuildingManager buildingManager;
    private VillagerManager villagerManager;
    private EventManager eventManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        // Create EventManager first since other systems depend on it
        if (FindAnyObjectByType<EventManager>() == null)
        {
            GameObject eventManagerObj = new GameObject("EventManager");
            eventManager = eventManagerObj.AddComponent<EventManager>();
        }
        else
        {
            eventManager = FindAnyObjectByType<EventManager>();
        }

        // Create TimeManager
        if (FindAnyObjectByType<TimeManager>() == null)
        {
            GameObject timeManagerObj = new GameObject("TimeManager");
            timeManager = timeManagerObj.AddComponent<TimeManager>();
        }
        else
        {
            timeManager = FindAnyObjectByType<TimeManager>();
        }

        // Create EconomyManager
        if (FindAnyObjectByType<EconomyManager>() == null)
        {
            GameObject economyManagerObj = new GameObject("EconomyManager");
            economyManager = economyManagerObj.AddComponent<EconomyManager>();
        }
        else
        {
            economyManager = FindAnyObjectByType<EconomyManager>();
        }

        // Create BuildingManager
        if (FindAnyObjectByType<BuildingManager>() == null)
        {
            GameObject buildingManagerObj = new GameObject("BuildingManager");
            buildingManager = buildingManagerObj.AddComponent<BuildingManager>();
        }
        else
        {
            buildingManager = FindAnyObjectByType<BuildingManager>();
        }

        // Create VillagerManager
        if (FindAnyObjectByType<VillagerManager>() == null)
        {
            GameObject villagerManagerObj = new GameObject("VillagerManager");
            villagerManager = villagerManagerObj.AddComponent<VillagerManager>();
        }
        else
        {
            villagerManager = FindAnyObjectByType<VillagerManager>();
        }
    }

    private void Update()
    {
        // Adjust simulation speed
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
        {
            IncreaseSimulationSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            DecreaseSimulationSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }

        // Apply time scaling
        Time.timeScale = pauseSimulation ? 0 : simulationSpeed;
    }

    public void IncreaseSimulationSpeed()
    {
        simulationSpeed = Mathf.Min(simulationSpeed + 0.5f, 10f);
        Debug.Log($"Simulation speed: {simulationSpeed}x");
    }

    public void DecreaseSimulationSpeed()
    {
        simulationSpeed = Mathf.Max(simulationSpeed - 0.5f, 0.5f);
        Debug.Log($"Simulation speed: {simulationSpeed}x");
    }

    public void TogglePause()
    {
        pauseSimulation = !pauseSimulation;
        Debug.Log(pauseSimulation ? "Simulation paused" : "Simulation resumed");
    }

    public float GetSimulationSpeed()
    {
        return simulationSpeed;
    }

    // Methods for checking global simulation settings
    public bool IsWeatherEnabled() => enableWeather;
    public bool IsDiseaseEnabled() => enableDisease;
    public bool IsAgingEnabled() => enableAging;
}