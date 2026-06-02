using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// End-of-run summary screen. Reads stats from GameSession and populates UI.
/// </summary>
public class EndScreenController : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text titleText;

    [Header("Run statistics")]
    public TMP_Text finalMoneyText;
    public TMP_Text highestMoneyText;
    public TMP_Text totalMoneySpentText;
    public TMP_Text turnsSurvivedText;
    public TMP_Text cardsPlayedText;
    public TMP_Text shopPurchasesText;
    public TMP_Text totalEnergyUsedText;
    public TMP_Text mostExpensivePurchaseText;
    public TMP_Text causeOfDeathText;

    [Header("Actions")]
    public Button restartButton;
    public Button quitButton;

    [Header("Copy")]
    public string titleFormat = "Run Complete";
    public string subtitleFormat = "Battles fought: {0} / {1}";

    void Awake()
    {
        WireReferencesIfMissing();
    }

    void Start()
    {
        PopulateStats();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRun);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void WireReferencesIfMissing()
    {
        if (titleText != null && finalMoneyText != null)
            return;

        titleText ??= FindChildText("TitleText");
        finalMoneyText ??= FindChildText("FinalMoneyText");
        highestMoneyText ??= FindChildText("HighestMoneyText");
        totalMoneySpentText ??= FindChildText("TotalMoneySpentText");
        turnsSurvivedText ??= FindChildText("TurnsSurvivedText");
        cardsPlayedText ??= FindChildText("CardsPlayedText");
        shopPurchasesText ??= FindChildText("ShopPurchasesText");
        totalEnergyUsedText ??= FindChildText("TotalEnergyUsedText");
        mostExpensivePurchaseText ??= FindChildText("MostExpensivePurchaseText");
        causeOfDeathText ??= FindChildText("CauseOfDeathText");

        if (restartButton == null)
        {
            var restart = FindChild("RestartButton");
            if (restart != null)
                restartButton = restart.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            var quit = FindChild("QuitButton");
            if (quit != null)
                quitButton = quit.GetComponent<Button>();
        }
    }

    TMP_Text FindChildText(string objectName)
    {
        var t = FindChild(objectName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    Transform FindChild(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == objectName)
                return t;
        }

        return null;
    }

    public void PopulateStats()
    {
        bool isVictory = GameSession.CauseOfDeath == GameSession.CauseRunVictory;

        if (titleText != null)
        {
            string title = isVictory ? "Victory!" : titleFormat;
            string subtitle = isVictory
                ? $"Phases completed: {GameSession.CurrentPhase} / {GameSession.MaxPhases}"
                : string.Format(subtitleFormat, GameSession.BattlesCompleted, GameSession.MaxBattles);
            titleText.text = $"{title}\n<size=70%>{subtitle}</size>";
        }

        SetStat(finalMoneyText, "Final money", $"€{GameSession.FinalMoney}");
        SetStat(highestMoneyText, "Highest money", $"€{GameSession.HighestMoney}");
        SetStat(totalMoneySpentText, "Total money spent", $"€{GameSession.TotalMoneySpent}");
        SetStat(turnsSurvivedText, "Turns survived", GameSession.TurnsSurvived.ToString());
        SetStat(cardsPlayedText, "Cards played", GameSession.CardsPlayed.ToString());
        SetStat(shopPurchasesText, "Shop purchases", GameSession.ShopPurchases.ToString());
        SetStat(totalEnergyUsedText, "Total energy used", GameSession.TotalEnergyUsed.ToString());
        SetStat(mostExpensivePurchaseText, "Most expensive purchase", $"€{GameSession.MostExpensivePurchase}");

        string cause = string.IsNullOrEmpty(GameSession.CauseOfDeath)
            ? "Unknown"
            : GameSession.CauseOfDeath;
        string causeLabel = isVictory ? "Result" : "Cause of end";
        SetStat(causeOfDeathText, causeLabel, cause);
    }

    static void SetStat(TMP_Text field, string label, string value)
    {
        if (field == null)
            return;

        field.text = $"{label}: {value}";
    }

    public void RestartRun()
    {
        GameSession.ResetRunStats();
        SceneManager.LoadScene("StartScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
