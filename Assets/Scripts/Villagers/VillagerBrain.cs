using UnityEngine;

// Consolidates all dependencies
public class VillagerBrain : MonoBehaviour
{

    // Components and subsystems
    public Villager VillagerComponent { get; private set; }
    public VillagerMovement Movement { get; private set; }
    public LocationFinder LocationFinder { get; private set; }
    public VillagerProfession Profession { get; private set; }
    public INeedsManager NeedsManager { get; private set; }
    public VillagerPersonality Personality { get; private set; }
    public VillagerMood VillagerMood { get; private set; }

    // State management
    private IVillagerState currentState;
    private float stateStartTime;
    private float minimumStateDuration = 0.5f; // Minimum time before a state can change (in real seconds)

    // Behavior assessment
    private float behaviorCheckTimer = 0f;
    private float behaviorCheckInterval = 4f; // Check less frequently (every 4 seconds)

    public IVillagerState CurrentState => currentState;

    public void Initialize(Villager villager, ProfessionData professionData)
    {
        VillagerComponent = villager;

        // Initialize movement component
        Movement = GetComponent<VillagerMovement>();
        if (Movement == null)
        {
            Movement = gameObject.AddComponent<VillagerMovement>();
        }

        // Initialize location finder
        LocationFinder = GetComponent<LocationFinder>();
        if (LocationFinder == null)
        {
            LocationFinder = gameObject.AddComponent<LocationFinder>();
        }

        // Initialize profession manager
        Profession = GetComponent<VillagerProfession>();
        if (Profession == null)
        {
            Profession = gameObject.AddComponent<VillagerProfession>();
        }
        Profession.Initialize(VillagerComponent);

        // Initialize personality
        Personality = GetComponent<VillagerPersonality>();
        if (Personality == null)
        {
            Personality = gameObject.AddComponent<VillagerPersonality>();
        }
        Personality.InitializePersonality();

        VillagerMood = GetComponent<VillagerMood>();
        if (VillagerMood == null)
        {
            VillagerMood = gameObject.AddComponent<VillagerMood>();
        }
        VillagerMood.Initialize(villager, this);

        // Assign profession
        Profession.AssignProfession(professionData);

        // Initialize needs manager
        NeedsManager = new NeedsManager(VillagerComponent);

        // Start with Idle state
        TransitionTo(new IdleState(this));

        // Subscribe to time change events
        EventManager.Instance.AddListener<TimeEvents.TimeOfDayChangedEvent>(OnTimeOfDayChanged);
        EventManager.Instance.AddListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);

        // Broadcast initialization
        EventManager.Instance.TriggerEvent(new VillagerEvents.VillagerInitializedEvent
        {
            VillagerName = VillagerComponent.villagerName,
            ProfessionType = Profession.ProfessionType,
            VillagerObject = gameObject
        });
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventManager.Instance.RemoveListener<TimeEvents.TimeOfDayChangedEvent>(OnTimeOfDayChanged);
        EventManager.Instance.RemoveListener<VillagerEvents.NeedBecameCriticalEvent>(OnNeedBecameCritical);
    }

    public void TransitionTo(IVillagerState newState)
    {
        // Prevent transitioning to the same type of state
        if (currentState != null && currentState.GetType() == newState.GetType())
            return;

        // Enforce minimum state duration
        if (currentState != null && Time.time < stateStartTime + minimumStateDuration)
            return;

        currentState?.ExitState();
        currentState = newState;
        stateStartTime = Time.time;
        currentState.EnterState();

        // Broadcast state change
        EventManager.Instance.TriggerEvent(new VillagerEvents.StateChangeEvent
        {
            VillagerName = VillagerComponent.villagerName,
            ProfessionType = Profession.ProfessionType.ToString(),
            NewState = newState.GetType()
        });
    }

    void Update()
    {
        // Update needs with time-scaled values
        NeedsManager.UpdateNeeds();

        // Update Mood
        VillagerMood.UpdateHappiness();

        // Update current state
        currentState?.UpdateState();

        // Update movement
        Movement.MoveToTarget();

        // Periodically reassess behavior
        behaviorCheckTimer += Time.deltaTime;
        if (behaviorCheckTimer >= behaviorCheckInterval)
        {
            behaviorCheckTimer = 0f;

            // Randomness based on personality
            if (UnityEngine.Random.value < Personality.impulsivity * 0.3f || currentState is IdleState)
            {
                DetermineNextAction();
            }
        }
    }

    // Handle time changes
    private void OnTimeOfDayChanged(TimeEvents.TimeOfDayChangedEvent evt)
    {
        // Schedule-driven behavior
        if (Profession.IsWorkingHour() && !(currentState is NeedFulfillmentState))
        {
            // Consider transitioning to work if it's work time
            if (currentState is not WorkingState && Personality.workEthic > 0.3f &&
                UnityEngine.Random.value < Personality.workEthic * 0.8f)
            {
                TransitionTo(new WorkingState(this));
                return;
            }
        }

        if (Profession.IsRestingHour() && !(currentState is NeedFulfillmentState))
        {
            // Consider transitioning to sleep if it's sleep time
            if (currentState is not SleepingState && UnityEngine.Random.value < 0.7f)
            {
                TransitionTo(new SleepingState(this));
                return;
            }
        }

        // Otherwise do a general behavior assessment
        DetermineNextAction();
    }

    // Handle critical needs
    private void OnNeedBecameCritical(VillagerEvents.NeedBecameCriticalEvent evt)
    {
        // Only respond if it's our villager
        if (evt.VillagerName != VillagerComponent.villagerName)
            return;

        DetermineNextAction();
    }

    public void DetermineNextAction()
    {
        // Check for critical needs first
        Need urgentNeed = NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            TransitionTo(new NeedFulfillmentState(this, urgentNeed));
            return;
        }

        // Get schedule-appropriate behavior if no urgent needs
        TimeOfDay currentTime = TimeManager.Instance.CurrentTimeOfDay;

        bool isVeryUnhappy = VillagerMood != null && VillagerMood.Happiness < 30f;

        // Working hours - villager should work unless unhappy or personality dictates otherwise
        if (Profession.IsWorkingHour())
        {
            if (VillagerMood != null && VillagerMood.ShouldSkipWork())
            {
                // Unhappy villager might socialize instead of work
                if (Personality.sociability > 0.4f)
                {
                    TransitionTo(new SocializingState(this));
                    return;
                }
                // Or just idle
                TransitionTo(new IdleState(this));
                return;
            }
            else if (UnityEngine.Random.value < Personality.workEthic)
            {
                TransitionTo(new WorkingState(this));
                return;
            }
        }

        // Social hours - villager should socialize unless personality dictates otherwise
        if (Profession.IsSocialHour())
        {
            if (UnityEngine.Random.value < Personality.sociability)
            {
                TransitionTo(new SocializingState(this));
                return;
            }
        }

        // Rest hours - villager should rest
        if (Profession.IsRestingHour())
        {
            TransitionTo(new SleepingState(this));
            return;
        }

        // Default to idle if nothing else is appropriate
        if (!(currentState is IdleState))
        {
            TransitionTo(new IdleState(this));
        }
    }

}
