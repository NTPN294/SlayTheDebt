using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static session state: player data and run statistics persisted across scenes.
/// </summary>
public static class GameSession
{
    public const string CauseHealthDepleted = "Health Depleted";
    public const string CauseRunVictory = "Run Complete";

    public const int MaxPhases = 5;
    public const int DebtHealthPenalty = 20;
    public static Player Player;
    public static WorldStats WorldStats;

    public static int MaxBattles = 5;

    public static int BattlesCompleted;
    public static int HighestMoney;
    public static int TotalMoneySpent;
    public static int TurnsSurvived;
    public static int CardsPlayed;
    public static int ShopPurchases;
    public static int TotalEnergyUsed;
    public static int MostExpensivePurchase;

    public static string CauseOfDeath;

    public static int CurrentPhase = 1;
    public static PhaseStep CurrentStep = PhaseStep.Battle1;

    public static int FinalMoney => Player?.Money ?? 0;

    public static void ResetRunStats()
    {
        Player = null;
        BattlesCompleted = 0;
        HighestMoney = 0;
        TotalMoneySpent = 0;
        TurnsSurvived = 0;
        CardsPlayed = 0;
        ShopPurchases = 0;
        TotalEnergyUsed = 0;
        MostExpensivePurchase = 0;
        CauseOfDeath = string.Empty;
        ResetPhaseFlow();
    }

    public static void ResetPhaseFlow()
    {
        CurrentPhase = 1;
        CurrentStep = PhaseStep.Battle1;
    }

    public static void EndRunVictory()
    {
        EndRun(CauseRunVictory);
    }

    public static void UpdateHighestMoney()
    {
        if (Player == null)
            return;

        if (Player.Money > HighestMoney)
            HighestMoney = Player.Money;
    }

    public static void RecordCardPlayed(int energyCost)
    {
        CardsPlayed++;
        TotalEnergyUsed += energyCost;
        UpdateHighestMoney();
    }

    public static void RecordShopPurchase(int cost)
    {
        ShopPurchases++;
        TotalMoneySpent += cost;

        if (cost > MostExpensivePurchase)
            MostExpensivePurchase = cost;

        UpdateHighestMoney();
    }

    public static void RecordTurnStarted()
    {
        TurnsSurvived++;
    }

    /// <summary>
    /// Call when a battle finishes (turns exhausted). Applies debt penalty and may end the run if health is 0.
    /// </summary>
    /// <returns>True if the run ended and EndScene was loaded.</returns>
    public static bool OnBattleEnded()
    {
        UpdateHighestMoney();
        BattlesCompleted++;

        if (Player == null)
            return false;

        if (Player.Money < 0)
            Player.Health = Mathf.Clamp(Player.Health - DebtHealthPenalty, 0, 100);

        if (Player.Health <= 0)
        {
            EndRun(CauseHealthDepleted);
            return true;
        }

        return false;
    }

    public static void EndRun(string cause)
    {
        CauseOfDeath = cause;
        UpdateHighestMoney();
        SceneManager.LoadScene("EndScene");
    }
}
