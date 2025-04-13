using UnityEngine;

public class Villager : MonoBehaviour
{
    [Header("Basic Attributes")]
    public string villagerName;
    public int age;
    public float health = 100f;
    public float personalWealth = 0f;

    [Header("Happiness")]
    [SerializeField] public float happiness = 50f;

    [Header("Debug Info")]
    [SerializeField] private string professionName = "Unassigned";
    [SerializeField] private string currentState = "None";
    [SerializeField] private string currentActivity = "None";
    [SerializeField] private float hungerNeed = 100f;
    [SerializeField] private float restNeed = 100f;
    [SerializeField] private float socialNeed = 100f;

    public VillagerBrain Brain { get; private set; }
    private SpriteRenderer villagerSpriteRenderer;
    private bool isInitialized = false;

    public void Initialize(ProfessionData profData)
    {
        if (isInitialized) return;

        // Initialize the brain, which handles all villager behavior
        Brain = GetComponent<VillagerBrain>();
        if (Brain == null)
        {
            Brain = gameObject.AddComponent<VillagerBrain>();
        }

        villagerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (villagerSpriteRenderer == null)
        {
            Debug.LogError($"Villager '{this.name}' could not find a Renderer component in itself or its children. Visibility toggling will fail.", this.gameObject);
        }

        Brain.Initialize(this, profData);

        // Set profession name for inspector
        if (profData != null)
        {
            professionName = profData.professionName;
        }

        isInitialized = true;
    }

    private void Update()
    {
        // Update inspector debug info if brain is initialized
        if (Brain != null && Brain.NeedsManager != null)
        {
            // Update current state
            if (Brain.CurrentState != null)
            {
                currentState = Brain.CurrentState.GetType().Name.Replace("State", "");
            }

            // Update needs
            foreach (var need in Brain.NeedsManager.GetAllNeeds())
            {
                if (need.Name == "Hunger")
                    hungerNeed = need.CurrentValue;
                else if (need.Name == "Rest")
                    restNeed = need.CurrentValue;
                else if (need.Name == "Social")
                    socialNeed = need.CurrentValue;
            }

            // Update current activity
            if (Brain.CurrentState is WorkingState)
            {
                currentActivity = $"Working as {professionName}";
            }
            else if (Brain.CurrentState is SocializingState)
            {
                currentActivity = "Socializing";
            }
            else if (Brain.CurrentState is SleepingState)
            {
                currentActivity = "Sleeping";
            }
            else if (Brain.CurrentState is NeedFulfillmentState)
            {
                currentActivity = $"Satisfying need";
            }
            else if (Brain.CurrentState is IdleState)
            {
                currentActivity = "Idle";
            }
        }
    }

    public void EarnWealth(float amount)
    {
        personalWealth += amount;
    }

    public void SpendWealth(float amount)
    {
        personalWealth = Mathf.Max(0, personalWealth - amount);
    }

    public void ChangeHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, 100);
    }

    public void SetVisibility(bool isVisible)
    {
        if (villagerSpriteRenderer != null && villagerSpriteRenderer.enabled != isVisible)
        {
            villagerSpriteRenderer.enabled = isVisible;
        }
    }

    // For debugging - display villager status as gizmo text
    private void OnDrawGizmos()
    {
        Vector3 textPos = transform.position + Vector3.up * 1.5f;

        if (Application.isPlaying && Brain != null && Brain.CurrentState != null)
        {
            string stateLabel = Brain.CurrentState.GetType().Name.Replace("State", "");
            GUIStyle style = new();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            // Only display in play mode to avoid cluttering the scene view during editing
            UnityEditor.Handles.Label(textPos, $"{villagerName}\n{professionName}\n{stateLabel}", style);
        }
    }
}

