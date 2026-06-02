using Assets.Scripts;
using Assets.Scripts.Content;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    #region Varibles

    //Variables imported from Unity
    [Header("World stats")]
    [SerializeField]
    private GameObject cardContainerObj;
    [SerializeField]
    private GameObject eventObj;
    [SerializeField]
    private GameObject choiseObj;
    [SerializeField]
    private int wealth;
    [SerializeField]
    private int innovation;
    [SerializeField]
    private bool war;
    [SerializeField]
    private bool crisis;

    [Header("End turn variations")]
    [SerializeField]
    private int inovationChange;
    [SerializeField]
    private int wealthChange;
    [SerializeField]
    private float warChance;

    //Local variables
    private Player player;
    private CardContainer cardContainer;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cardContainer = cardContainerObj.GetComponent<CardContainer>();
        player = GameSession.Player;

        // Sweep cards that belong to an earlier life stage out of the deck
        // (e.g. Scholarship / Internship / Maxloan don't follow you into your 60s).
        // Runs before any UI populates from player.Cards.
        DeckMaintenance.ExpireOutOfPhase(player, GameSession.CurrentPhase);

        ClampStats();
        SetWorldStats();
    }

    // Update is called once per frame
    void Update()
    {
        //Set the current player
        player = GameSession.Player;
    }

    #region Stat changes 

    /// <summary>
    /// Sets the world stats at the end of a turn
    /// </summary>
    public void EndTurn()
    {
        // Random stat changes
        wealth += Random.value < 0.5f ? -wealthChange : wealthChange;
        innovation += Random.value < 0.5f ? -inovationChange : inovationChange;

        // Clamp stats between 0 and 100
        wealth = Mathf.Clamp(wealth, 0, 100);
        innovation = Mathf.Clamp(innovation, 0, 100);

        // Set wether there is war based on the war chance
        war = Random.value < warChance / 100;

        // Set wether there is a crisis. Crisis chance increases as wealth decreases
        float crisisChance = (101 - wealth) / 200f;
        crisis = Random.value < crisisChance;

        ClampStats();
        SetWorldStats();

        //Add a turn count to the game session world states
        GameSession.WorldStats.turnCount++;
    }

    /// <summary>
    /// Sets the stats to the maximum values based on the current world state
    /// </summary>
    private void ClampStats()
    {
        if (war && wealth > 40)
            wealth = 40;

        if (crisis && wealth > 25)
            wealth = 25;

        if (war && innovation > 70)
            innovation = 70;

        if (crisis && innovation > 10)
            innovation = 10;

        if (wealth < 0)
            wealth = 0;

        if (innovation < 0)
            innovation = 0;

        if (wealth > 100)
            wealth = 100;

        if (innovation > 100)
            innovation = 100;
    }

    #endregion

    #region Event methods

    /// <summary>
    /// Start an event
    /// </summary>
    public void StartEvent()
    {
        //Spawn an event or a choise every 2 turns
        if (GameSession.WorldStats.turnCount % 2 == 0)
        {
            TrySpawnRandomEvent(null);
        }
    }

    /// <summary>
    /// Spawns a random event or choice popup pulled from EventDatabase, filtered by
    /// current phase and per-row conditions. Returns false if nothing in either pool
    /// is currently eligible.
    /// </summary>
    public bool TrySpawnRandomEvent(System.Action onComplete)
    {
        EventContext ctx = BuildEventContext();

        // 50/50 try-Event-first / try-Choice-first, with cross-kind fallback so
        // we still spawn something if one bucket happens to be empty this phase.
        bool eventFirst = Random.value < 0.5f;
        EventKind primary = eventFirst ? EventKind.Event : EventKind.Choice;
        EventKind fallback = eventFirst ? EventKind.Choice : EventKind.Event;

        if (TrySpawnKind(primary, ctx, onComplete))
            return true;

        return TrySpawnKind(fallback, ctx, onComplete);
    }

    /// <summary>
    /// Spawns the guaranteed scripted event for the current phase, applying its effects
    /// (if any) when the player clicks Continue / a choice button.
    /// </summary>
    public void TrySpawnScriptedEvent(int phase, System.Action onComplete)
    {
        EventContext ctx = BuildEventContext();
        EventDefinition def = ScriptedEventDatabase.Get(phase);

        if (def == null)
        {
            // Defensive fallback: legacy text-only path.
            var (title, description) = ScriptedEvents.Get(phase);
            SpawnEventPopup(title, description, null, ctx, onComplete);
            return;
        }

        SpawnFromDefinition(def, ctx, onComplete);
    }

    bool TrySpawnKind(EventKind kind, EventContext ctx, System.Action onComplete)
    {
        EventDefinition def = EventDatabase.GetWeightedRandom(ctx.Phase, kind, ctx);
        if (def == null)
            return false;

        SpawnFromDefinition(def, ctx, onComplete);
        return true;
    }

    void SpawnFromDefinition(EventDefinition def, EventContext ctx, System.Action onComplete)
    {
        // Register before spawn so the next pool draw already knows it's on cooldown.
        RunFlags.RecordEvent(def.Id);

        System.Action wrappedComplete = () =>
        {
            if (def.OncePerRun)
                RunFlags.Set(RunFlags.OnceKey(def.Id), true);
            onComplete?.Invoke();
        };

        if (def.Kind == EventKind.Choice)
        {
            SpawnChoicePopup(def, ctx, wrappedComplete);
        }
        else
        {
            SpawnEventPopup(def.Title, def.Description, def.OnContinueEffects, ctx, wrappedComplete);
        }
    }

    void SpawnEventPopup(string title, string description, EffectStep[] onContinueEffects,
                         EventContext ctx, System.Action onComplete)
    {
        GameObject spawnedEvent = Instantiate(eventObj, transform, false);
        spawnedEvent.transform.localPosition += new Vector3(0, 500f, 0);

        EventController eventController = spawnedEvent.GetComponent<EventController>();
        eventController.SetText(title, description);
        eventController.SetOnComplete(() =>
        {
            // Effects fire AFTER the player reads & clicks Continue, never before.
            if (onContinueEffects != null && onContinueEffects.Length > 0)
                ContentEffectApplier.Apply(onContinueEffects, ctx);
            onComplete?.Invoke();
        });
    }

    void SpawnChoicePopup(EventDefinition def, EventContext ctx, System.Action onComplete)
    {
        GameObject spawnedChoise = Instantiate(choiseObj, transform, false);
        spawnedChoise.transform.localPosition += new Vector3(0, 500f, 0);

        ChoiseController choiseController = spawnedChoise.GetComponent<ChoiseController>();
        // DataChoise applies the per-button EffectSteps in ButtonOneAction / ButtonTwoAction.
        choiseController.choiseInstance = new DataChoise(def, ctx);
        choiseController.SetText(def.Title, def.Description, def.ButtonOneText, def.ButtonTwoText);
        choiseController.SetOnComplete(onComplete);
    }

    EventContext BuildEventContext()
    {
        return new EventContext
        {
            Player = GameSession.Player,
            World = GameSession.WorldStats,
            Deck = cardContainer,
            Phase = GameSession.CurrentPhase
        };
    }

    #endregion

    #region World session

    /// <summary>
    /// Get the game session world stats so you can get the stats when a new scene is created
    /// </summary>
    private WorldStats GetWorldStats()
    {
        //Check if there is an active world stats
        if (GameSession.WorldStats != null)
        {
            //Create a new world stats
            WorldStats ws = new();

            //Set new world stats to current controller stats
            ws.war = this.war;
            ws.wealth = this.wealth;
            ws.warChance = this.warChance;
            ws.innovation = this.innovation;
            ws.crisis = this.crisis;
            ws.inovationChange = this.inovationChange;
            ws.wealthChange = this.wealthChange;
            ws.turnCount = 0;

            //Set the game session world stats to the new world stats
            ws = GameSession.WorldStats;
        }

        return GameSession.WorldStats;
    }

    /// <summary>
    /// Set the world stats to the game session world stats so they stay when a new scene is created
    /// </summary>
    private void SetWorldStats()
    {
        //Check if there is an active world stats
        if (GameSession.WorldStats == null){
            //Create a new world stats
            WorldStats ws = new();
            GameSession.WorldStats = ws;
        }

        //Set the stats of the game session world stats to the stats of the current controller
        GameSession.WorldStats.war = this.war;
        GameSession.WorldStats.wealth = this.wealth;
        GameSession.WorldStats.warChance = this.warChance;
        GameSession.WorldStats.innovation = this.innovation;
        GameSession.WorldStats.crisis = this.crisis;
        GameSession.WorldStats.inovationChange = this.inovationChange;
        GameSession.WorldStats.wealthChange = this.wealthChange;
    }

    #endregion
}
