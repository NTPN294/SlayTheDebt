using Assets.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central run flow controller: 5 phases, each with 11 fixed steps.
/// Persists across scene loads via DontDestroyOnLoad + GameSession state.
///
/// UNITY SETUP (manual):
/// 1. In Phase1Scene (or StartScene), create empty GameObject named "PhaseFlowManager".
/// 2. Add this component. It uses DontDestroyOnLoad automatically.
/// 3. In Phase1Scene, disable or delete the ShopButton (was wired to OpenShopScene).
/// 4. In Phase1Scene, disable the Next Battle button (was wired to PlayerController.nextBattle).
/// 5. ShopScene: keep Back button on ShopUIController — it now continues the phase flow.
/// 6. Event/choice prefabs need no inspector changes; Continue/choice buttons already call EventController / ChoiseController.
/// </summary>
public class PhaseFlowManager : MonoBehaviour
{
    public static PhaseFlowManager Instance { get; private set; }

    const string Phase1SceneName = "Phase1Scene";
    const string ShopSceneName = "ShopScene";

    [Header("Battle")]
    [Tooltip("Debt amount the enemy starts with at the beginning of each battle.")]
    public int enemyStartingDebt = 500;

    bool _waitingForShopExit;
    bool _resumeOnNextPhase1Load = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != Phase1SceneName)
            return;

        if (!_resumeOnNextPhase1Load)
            return;

        StartCoroutine(ResumeFlowAfterSceneInit());
    }

    IEnumerator ResumeFlowAfterSceneInit()
    {
        // Wait one frame so PlayerController / WorldController Start() can finish.
        yield return null;
        ResumeFlow();
    }

    /// <summary>
    /// Called when Phase1Scene loads or when flow should continue in the current scene.
    /// </summary>
    public void ResumeFlow()
    {
        if (GameSession.Player == null)
            return;

        ExecuteCurrentStep();
    }

    /// <summary>
    /// Call after a battle ends and health/death checks pass.
    /// </summary>
    public void NotifyBattleComplete()
    {
        if (GameSession.Player == null)
            return;

        AdvanceStep();
        ExecuteCurrentStep();
    }

    /// <summary>
    /// Call after a random or scripted event popup is dismissed.
    /// </summary>
    public void NotifyEventComplete()
    {
        if (GameSession.Player == null)
            return;

        AdvanceStep();
        ExecuteCurrentStep();
    }

    /// <summary>
    /// Call when the player leaves ShopScene via the back button.
    /// </summary>
    public void NotifyShopComplete()
    {
        if (GameSession.Player == null)
            return;

        if (!_waitingForShopExit)
            return;

        _waitingForShopExit = false;
        AdvanceStep();

        if (GameSession.CurrentStep == PhaseStep.Complete)
        {
            HandlePhaseComplete();
            return;
        }

        _resumeOnNextPhase1Load = true;
        SceneManager.LoadScene(Phase1SceneName);
    }

    void ExecuteCurrentStep()
    {
        switch (GameSession.CurrentStep)
        {
            case PhaseStep.Battle1:
            case PhaseStep.Battle2:
            case PhaseStep.Battle3:
            case PhaseStep.Battle4:
                BeginBattle();
                break;

            case PhaseStep.RandomEvent1:
            case PhaseStep.RandomEvent2:
            case PhaseStep.RandomEvent3:
                BeginRandomEvent();
                break;

            case PhaseStep.ScriptedEvent:
                BeginScriptedEvent();
                break;

            case PhaseStep.Shop1:
            case PhaseStep.Shop2:
                BeginShop();
                break;

            case PhaseStep.Complete:
                HandlePhaseComplete();
                break;
        }
    }

    void BeginBattle()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
            playerController.PrepareNewBattle();

        var enemyController = FindFirstObjectByType<EnemyController>();
        if (enemyController != null)
            enemyController.ResetForNewBattle(enemyStartingDebt);
    }

    void BeginRandomEvent()
    {
        var worldController = FindFirstObjectByType<WorldController>();
        if (worldController == null)
        {
            Debug.LogWarning("PhaseFlowManager: WorldController not found. Skipping random event.");
            NotifyEventComplete();
            return;
        }

        if (!worldController.TrySpawnRandomEvent(NotifyEventComplete))
        {
            Debug.LogWarning("PhaseFlowManager: No random event available. Skipping step.");
            NotifyEventComplete();
        }
    }

    void BeginScriptedEvent()
    {
        var worldController = FindFirstObjectByType<WorldController>();
        if (worldController == null)
        {
            Debug.LogWarning("PhaseFlowManager: WorldController not found. Skipping scripted event.");
            NotifyEventComplete();
            return;
        }

        worldController.TrySpawnScriptedEvent(GameSession.CurrentPhase, NotifyEventComplete);
    }

    void BeginShop()
    {
        _waitingForShopExit = true;
        _resumeOnNextPhase1Load = false;
        ShopUIController.PersistEnergyFromPhase1();
        SceneManager.LoadScene(ShopSceneName);
    }

    void AdvanceStep()
    {
        if (GameSession.CurrentStep >= PhaseStep.Complete)
            return;

        GameSession.CurrentStep++;
    }

    void HandlePhaseComplete()
    {
        if (GameSession.CurrentPhase >= GameSession.MaxPhases)
        {
            GameSession.EndRunVictory();
            return;
        }

        GameSession.CurrentPhase++;
        GameSession.CurrentStep = PhaseStep.Battle1;
        _resumeOnNextPhase1Load = true;
        SceneManager.LoadScene(Phase1SceneName);
    }

    public static bool IsBattleStep(PhaseStep step)
    {
        return step == PhaseStep.Battle1
            || step == PhaseStep.Battle2
            || step == PhaseStep.Battle3
            || step == PhaseStep.Battle4;
    }
}
