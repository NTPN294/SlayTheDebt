using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts;

public class PlayerController : MonoBehaviour
{
    //Unity variables
    [Header("References")]
    public TMP_Text playerMoneyText;
    public TMP_Text enemyMoneyText;
    public TMP_Text turnsText;
    public int initialTurns = 3;
    public TMP_Text EnergyText;
    public int initialEnergy = 5;
    public Button nextBattleButton;
    [SerializeField]
    private GameObject cardContainerObj;
    [SerializeField]
    private GameObject world;
    [Header("Layout")]
    public int maxCardsToShow = 5;
    public float horizontalPadding = 50;
    public float leftX = 50;
    public float bottomY = 10;

    //Local variables
    private CardContainer cardContainer;
    private WorldController worldController;

    void Start()
    {
        cardContainer = cardContainerObj.GetComponent<CardContainer>();
        worldController = world.GetComponent<WorldController>();
        getPlayer();
        cardContainer.AddStartingCards();
        cardContainer.SetUpHands();
        cardContainer.ShowCards();

        if (nextBattleButton != null)
            nextBattleButton.gameObject.SetActive(false);
    }

    private Player getPlayer()
    {
        if (GameSession.Player == null)
        {
            //Create new player
            Player player = new Player();

            //Set the player stats
            player.Money = 1000;
            player.Happiness = 50;
            player.Health = 20;
            player.Career = 30;
            player.Relationships = 40;

            //Add player cards
            player.Cards = Generator.GenerateCards(new[]
            {
                Names.WorkCardLow,
                Names.TutorJob,
                Names.Internship,
                Names.Bike,
                Names.LateNightStudy,
                Names.Gamble,
                Names.Gamble,
                Names.Gift
            });

            //Set the game session player to the new player
            GameSession.Player = player;
        }

        //Set the UI text
        playerMoneyText.text = GameSession.Player.Money.ToString();
        EnergyText.text = initialEnergy.ToString();
        turnsText.text = initialTurns.ToString();

        return GameSession.Player;
    }

    public void newTurn()
    {
        if (turnsText.text == "None")
        {
            return;
        }

        int turn = int.Parse(turnsText.text);

        if (turn > 0)
        {
            GameSession.RecordTurnStarted();
            turnsText.text = (turn - 1).ToString();

            //Reset the hand for the new turn
            cardContainer.ShowCards();

            EnergyText.text = initialEnergy.ToString();

        }
        else
        {
            noTurnsLeft();
            cardContainer.ShowCards();
            turnsText.text = "None";
            Debug.Log("No Turns Left");

            //Make the deck ready for the next round
            cardContainer.SetUpForNextRound();
            cardContainer.ShowCards();

            //Run the end turn function of the world controller to update the world stats for the next round
            worldController.EndTurn();

            if (GameSession.OnBattleEnded())
                return;

            PhaseFlowManager.Instance?.NotifyBattleComplete();
        }

    }

    public void noTurnsLeft()
    {
        int newMoney = int.Parse(playerMoneyText.text) - int.Parse(enemyMoneyText.text);
        playerMoneyText.text = newMoney.ToString();
        GameSession.Player.Money = newMoney;
        GameSession.UpdateHighestMoney();

        StatusController hud = FindAnyObjectByType<StatusController>();
        if (hud != null)
            hud.LoadFromGameSession();
    }

    /// <summary>
    /// Resets battle UI for the next battle step in the current phase.
    /// Called by PhaseFlowManager.
    /// </summary>
    public void PrepareNewBattle()
    {
        if (GameSession.Player == null)
            return;

        playerMoneyText.text = GameSession.Player.Money.ToString();
        turnsText.text = initialTurns.ToString();
        EnergyText.text = initialEnergy.ToString();
        GameSession.Player.Energy = initialEnergy;

        cardContainer.SetUpForNextRound();
        cardContainer.ShowCards();

        if (nextBattleButton != null)
            nextBattleButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Deprecated: phase flow now handles advancing between battles automatically.
    /// </summary>
    public void nextBattle()
    {
        // Intentionally empty — disable the Next Battle button in the inspector.
    }
}
