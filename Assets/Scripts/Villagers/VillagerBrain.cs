using System.Collections.Generic;
using System.Linq;
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
    public VillagerGoals Goals { get; private set; }

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

        // Initialise movement component
        Movement = GetComponent<VillagerMovement>();
        if (Movement == null)
        {
            Movement = gameObject.AddComponent<VillagerMovement>();
        }

        // Initialise location finder
        LocationFinder = GetComponent<LocationFinder>();
        if (LocationFinder == null)
        {
            LocationFinder = gameObject.AddComponent<LocationFinder>();
        }

        // Initialise profession manager
        Profession = GetComponent<VillagerProfession>();
        if (Profession == null)
        {
            Profession = gameObject.AddComponent<VillagerProfession>();
        }
        Profession.Initialize(VillagerComponent);

        // Initialise personality
        Personality = GetComponent<VillagerPersonality>();
        if (Personality == null)
        {
            Personality = gameObject.AddComponent<VillagerPersonality>();
        }
        Personality.Initialize();

        VillagerMood = GetComponent<VillagerMood>();
        if (VillagerMood == null)
        {
            VillagerMood = gameObject.AddComponent<VillagerMood>();
        }
        VillagerMood.Initialize(villager, this);

        Goals = GetComponent<VillagerGoals>();
        if (Goals == null)
        {
            Goals = gameObject.AddComponent<VillagerGoals>();
        }
        Goals.Initialize(villager, this);

        // Assign profession
        Profession.AssignProfession(professionData);

        // Initialise needs manager
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

        // Otherwise do general behaviour assessment
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
        // STEP 1: Critical Need Check 
        // Handle urgent needs before any other decision-making
        Need urgentNeed = NeedsManager.GetMostUrgentNeed();
        if (urgentNeed != null)
        {
            if (urgentNeed.Name == "Hunger" && VillagerComponent.personalWealth < EconomyManager.Instance.GetResourcePrice(ResourceType.Food) * urgentNeed.ResourceAmountNeeded)
            {
                // Can't afford food, go work
                if (Profession.ProfessionType != ProfessionType.Unemployed)
                {
                    Debug.Log($"{VillagerComponent.villagerName} is hungry but broke. Prioritising work.");
                    TransitionTo(new WorkingState(this));
                    return;
                }
            }

            TransitionTo(new NeedFulfillmentState(this, urgentNeed));
            return;
        }

        //  Decision making evaluation
        TimeOfDay currentTime = TimeManager.Instance.CurrentTimeOfDay;
        bool isVeryUnhappy = VillagerMood != null && VillagerMood.Happiness < 30f;
        bool isLeisureTime = currentTime == TimeOfDay.Noon || currentTime == TimeOfDay.Evening; // free time

        // STEP 2: Calculate Base Priorities
        Dictionary<System.Type, float> statePriorities = new();

        if (Profession.IsWorkingHour() && !isVeryUnhappy) statePriorities[typeof(WorkingState)] = Personality.workEthic * 10f;
        if (Profession.IsRestingHour()) statePriorities[typeof(SleepingState)] = 15f;

        if (isLeisureTime || isVeryUnhappy)
        {
            // Socialising: Base priority influenced by need and sociability
            Need socialNeed = NeedsManager?.GetAllNeeds().FirstOrDefault(n => n.Name == "Social");
            float socialNeedUrgencyFactor = (socialNeed != null && socialNeed.CurrentValue < 85f) ? ((100f - socialNeed.CurrentValue) / 100f) : 0f;
            statePriorities[typeof(SocializingState)] = (Personality.sociability * 5f) + (socialNeedUrgencyFactor * 3f);
            if (isVeryUnhappy) statePriorities[typeof(SocializingState)] += 3f; // Boost if unhappy

            // Relax at Home: Base priority, higher if less sociable or tired
            Need restNeed = NeedsManager?.GetAllNeeds().FirstOrDefault(n => n.Name == "Rest");
            float restNeedFactor = (restNeed != null && restNeed.CurrentValue < 90f) ? ((100f - restNeed.CurrentValue) / 100f) : 0f;
            statePriorities[typeof(RelaxAtHomeState)] = (1f - Personality.sociability) * 5f + restNeedFactor * 3f; // Inverse sociability + slight rest influence

            // Idle (Wandering/Visiting)
            float randomIdleChance = 0.15f;
            if (Random.value < randomIdleChance)
            {
                Debug.Log($"{VillagerComponent.villagerName} decided to randomly idle.");
                TransitionTo(new IdleState(this));
                return;
            }

            statePriorities[typeof(IdleState)] = 2.5f;
        }
        else // Not leisure time
        {
            // Idle has normal low priority outside leisure
            statePriorities[typeof(IdleState)] = 1f;
        }

        // STEP 3: Apply Goal Preferences
        if (Goals != null)
        {
            foreach (var stateEntry in statePriorities.Keys.ToList())
            {
                IVillagerState stateInstance = CreateStateInstance(stateEntry);
                if (stateInstance != null) statePriorities[stateEntry] += Goals.GetGoalPreference(stateInstance);
            }
        }

        // STEP 4: Apply Personality

        // 4.1: Impulsivity - Random behaviour
        if (Random.value < Personality.impulsivity * 0.15f) // 15% chance at max impulsivity
        {
            List<System.Type> impulsiveChoices = new() { typeof(SocializingState), typeof(IdleState), typeof(RelaxAtHomeState) };
            impulsiveChoices.RemoveAll(t => !statePriorities.ContainsKey(t));

            if (impulsiveChoices.Count > 0)
            {
                System.Type impulsiveChoice = impulsiveChoices[Random.Range(0, impulsiveChoices.Count)];
                Debug.Log($"{VillagerComponent.villagerName} acting impulsively, choosing {impulsiveChoice.Name}");
                TransitionTo(CreateStateInstance(impulsiveChoice));
                return;
            }
        }

        // 4.2: Work Ethic - Low work ethic villagers might skip work
        if (Profession.IsWorkingHour() && statePriorities.ContainsKey(typeof(WorkingState)) && Personality.workEthic < 0.4f)
        {
            if (Random.value < (0.4f - Personality.workEthic) * 0.5f)
            {
                statePriorities[typeof(WorkingState)] *= 0.1f; // Reduce work priority
                Debug.Log($"{VillagerComponent.villagerName} feeling lazy, might skip work.");
            }
        }

        // 4.3: Unhappiness - Avoid work when very unhappy
        if (isVeryUnhappy && statePriorities.ContainsKey(typeof(WorkingState)))
        {
            statePriorities[typeof(WorkingState)] *= 0.2f; // Strongly prefer not working
            if (statePriorities.ContainsKey(typeof(SocializingState))) statePriorities[typeof(SocializingState)] += 5f;
            if (statePriorities.ContainsKey(typeof(RelaxAtHomeState))) statePriorities[typeof(RelaxAtHomeState)] += 3f; // Boost Relax too
            if (statePriorities.ContainsKey(typeof(IdleState))) statePriorities[typeof(IdleState)] += 2f;
            Debug.Log($"{VillagerComponent.villagerName} is unhappy, avoiding work.");
        }

        // STEP 5: Select Highest Priority State
        System.Type selectedStateType = typeof(IdleState);

        if (statePriorities.Count > 0) { selectedStateType = statePriorities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key; }
        else { selectedStateType = typeof(IdleState); }

        // STEP 6: Transition to Selected State
        IVillagerState newState = CreateStateInstance(selectedStateType);
        if (newState != null) TransitionTo(newState);
        else TransitionTo(new IdleState(this));
    }

    private IVillagerState CreateStateInstance(System.Type stateType)
    {
        if (stateType == typeof(WorkingState)) return new WorkingState(this);
        if (stateType == typeof(SocializingState)) return new SocializingState(this);
        if (stateType == typeof(SleepingState)) return new SleepingState(this);
        if (stateType == typeof(IdleState)) return new IdleState(this);
        if (stateType == typeof(RelaxAtHomeState)) return new RelaxAtHomeState(this);

        return null;
    }
}
