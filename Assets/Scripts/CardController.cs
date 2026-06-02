using Assets.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    #region Variables

    //Variables imported from Unity
    public TMP_Text EnergyText { set; private get; }

    //Local variables
    public Card Card { get; set; }
    public CardContainer cardContainer { set; private get; }

    private RectTransform rectTransform;
    private Canvas canvas;

    #endregion

    #region Unity functions

    /// <summary>
    /// Runs when the game starts
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// This is run when the object is created
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Is run at the start of a drag
    /// </summary>
    /// <param name="eventData">The drag event data</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Optional
    }

    /// <summary>
    /// This run during the drag
    /// </summary>
    /// <param name="eventData">The drag event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    /// <summary>
    /// This run at the end of a drag
    /// </summary>
    /// <param name="eventData">The drag event data</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (EventSystem.current == null)
            return;

        // Check if Card and EnergyText are assigned
        if (Card == null || EnergyText == null)
        {
            Debug.LogError("Card or EnergyText not assigned on CardController");
            return;
        }

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            //Get the enemy controller from the hit object if there is one
            EnemyController enemy = result.gameObject.GetComponentInParent<EnemyController>();
            if (enemy == null)
                continue;

            //Checks if the player has enough energy to play the card
            if (Card.EnergyCost > int.Parse(EnergyText.text))
            {
                Debug.Log("Not enough energy to play this card.");
                return;
            }
            else
            {
                //Removes energy from the player
                int newEnergy = int.Parse(EnergyText.text) - Card.EnergyCost;
                EnergyText.text = newEnergy.ToString();
            }

            //Deal damage to the enemy
            if (Card.Type == CardType.Chance)
            {
                if (Random.value < 0.5f)
                {
                    enemy.ApplyCardValue(Card.Value);
                }

            }
            else
            {
                enemy.ApplyCardValue(Card.Value);
            }

            // Pas stat-wijzigingen toe en ververs de HUD
            ApplyStatChanges();

            // If card is consumable, remove it from the deck
            if (Card.Type == CardType.Consumable)
                cardContainer.RemoveCardFromDeck(Card);

            //Destroy the card and remove it from the hand
            GameObject fx = new GameObject("HitEffect");

            fx.transform.SetParent(enemy.transform, false);

            fx.AddComponent<HitEffect>();
            // --- END ---
            Destroy(gameObject);
            cardContainer.RemoveCardFromHand(Card);
            return;
        }
    }

    private void ApplyStatChanges()
    {
        if (GameSession.Player == null) return;

        GameSession.Player.Happiness = Mathf.Clamp(GameSession.Player.Happiness + Card.HappinessChange, 0, 100);
        GameSession.Player.Health = Mathf.Clamp(GameSession.Player.Health + Card.HealthChange, 0, 100);
        GameSession.Player.Career = Mathf.Clamp(GameSession.Player.Career + Card.CareerChange, 0, 100);
        GameSession.Player.Relationships = Mathf.Clamp(GameSession.Player.Relationships + Card.RelationshipChange, 0, 100);

        // Ververs de stats-balk in de scene
        StatusController hud = FindAnyObjectByType<StatusController>();
        if (hud != null) hud.LoadFromGameSession();
    }

    #endregion
}