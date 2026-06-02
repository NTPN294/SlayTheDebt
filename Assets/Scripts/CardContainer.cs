using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardContainer : MonoBehaviour
{
    #region Variables

    //Variables imported from Unity
    [SerializeField]
    private TMP_Text EnergyText;
    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private GameObject handButtonPrefab;
    [SerializeField]
    private int maxCardsShown;
    [SerializeField]
    private float spaceBetweenCards;
    [SerializeField]
    private float cardWidth;

    //Local variables
    public Card Card { get; set; }

    private List<Card> currentRoundCards = new List<Card>();
    private List<Card> cards = new List<Card>();
    private List<List<Card>> hands = new List<List<Card>>();
    private List<CardController> shownCards = new List<CardController>();
    private int currentHand = 0;
    private RectTransform cardParent;
    private HandButton nextHandButton;
    private HandButton previousHandButton;

    #endregion

    #region Unity functions

    /// <summary>
    /// Runs when the game starts
    /// </summary>
    private void Start()
    {
        //Set the variabels correct
        cardParent = (RectTransform)transform;
    }

    #endregion

    #region Get/Set functions

    public List<Card> GetPlayerDeck() 
        => cards;

    #endregion

    #region setup hand

    public void SetUpHands()
    {
        //Clears the current hands to make new ones
        hands.Clear();

        //Splits the cards the player has into multiple hands that can be shown one at a time
        hands.Add(new List<Card>());
        int index = 0;

        for (int i = 0; i < currentRoundCards.Count; i++)
        {
            if (i % maxCardsShown == 0 && i != 0)
            {
                hands.Add(new List<Card>());
                index++;
            }

            hands[index].Add(currentRoundCards[i]);
        }

        //Set the current hand to the last hand if the current hand is to high
        if (currentHand >= hands.Count)
            currentHand--;
    }

    /// <summary>
    /// Show the next hand
    /// </summary>
    public void ShowNextHand()
    {
        //Show the next hand if there is one
        if (currentHand < hands.Count - 1)
        {
            currentHand++;
            ShowCards();
        }
    }

    /// <summary>
    /// Show the previous hand
    /// </summary>
    public void ShowPreviousHand()
    {
        //Show the previous hand if there is one
        if (currentHand > 0)
        {
            currentHand--;
            ShowCards();
        }
    }

    /// <summary>
    /// Set up the cards for the next round
    /// </summary>
    public void SetUpForNextRound()
    {
        //Clear the current round cards and fill it up with the cards from the deck
        currentRoundCards.Clear();
        foreach (Card card in cards)
        {
            currentRoundCards.Add(card);
        }

        SetUpHands();
    }

    #endregion

    #region Add/Remove cards

    /// <summary>
    /// Make starting cards based on the filled in sheet
    /// </summary>
    public void AddStartingCards()
    {
        //Get the player cards
        cards = GameSession.Player.Cards;

        //Add the player cards to the cards for the first round
        if (currentRoundCards.Count <= 0)
        {
            foreach (Card card in cards)
            {
                currentRoundCards.Add(card);
            }
        }
    }

    /// <summary>
    /// Add a card to the players deck
    /// </summary>
    public void AddCard(string name)
    {
        //Check if the current hand is full, if it is add a new hand to the list of hands
        if (hands.Last().Count >= maxCardsShown)
        {
            hands.Add(new List<Card>());
        }

        //Add card to the hand and the deck
        Card newCard = Generator.GenerateCard(name);
        hands.Last().Add(newCard);
        currentRoundCards.Add(newCard);
        cards.Add(newCard);

        //Show the current hand again to update the shown cards
        ShowCards();
    }

    /// <summary>
    /// Removes a card
    /// </summary>
    public void RemoveCardFromHand(Card card)
    {
        //Remove card from the current round deck
        for (int i = 0; i < currentRoundCards.Count(); i++)
        {
            if (card.IsSameCard(currentRoundCards[i].Name, currentRoundCards[i].Type, currentRoundCards[i].Value))
            {
                currentRoundCards.RemoveAt(i);
                break;
            }
        }

        //Set the hands up again to make sure the cards are in the correct hands
        SetUpHands();

        //Show the current hand again to update the shown cards
        ShowCards();
    }

    /// <summary>
    /// Removes a card
    /// </summary>
    public void RemoveCardFromDeck(Card card)
    {
        //Remove card from the current round deck
        for (int i = 0; i < cards.Count(); i++)
        {
            if (card.IsSameCard(cards[i].Name, cards[i].Type, cards[i].Value))
            {
                cards.RemoveAt(i);
                break;
            }
        }

        //Set the hands up again and show the card
        SetUpHands();
        ShowCards();
    }

    #endregion

    #region Create UI

    /// <summary>
    /// Spawn a new hand of cards
    /// </summary>
    public void ShowCards()
    {
        //Destroy the current hand UI
        DestroyHandButtons();
        DestroyCurrectCards();

        int amountOfCardsToShow = maxCardsShown;

        //Check if the player has enough cards to fill the max amount of cards shown, if not only show the amount of cards the player has
        if (amountOfCardsToShow > hands[currentHand].Count)
            amountOfCardsToShow = hands[currentHand].Count;

        //Spawn the cards for the hand to show
        for (int i = 0; i < amountOfCardsToShow; i++)
        {
            Card data = currentRoundCards[((currentHand) * maxCardsShown) + i];
            CardController cardUI = Instantiate(cardPrefab, cardParent, false).GetComponent<CardController>();

            //Calculate the horizontal position for the card
            float xPosition = cardParent.anchoredPosition.x;

            if (i < (amountOfCardsToShow + 1) / 2)
            {
                float cardsInBetween = Mathf.Floor((amountOfCardsToShow + 1) / 2f) - i;
                xPosition = cardParent.anchoredPosition.x - (cardsInBetween * cardWidth + cardsInBetween * spaceBetweenCards);
            }
            else if (i > (amountOfCardsToShow + 1) / 2)
            {
                float cardsInBetween = i - Mathf.Floor((amountOfCardsToShow + 1) / 2);
                xPosition = cardParent.anchoredPosition.x + (cardsInBetween * cardWidth + cardsInBetween * spaceBetweenCards);
            }

            //Set the correct position for the card
            Vector2 position = new(xPosition, cardParent.anchoredPosition.y);
            cardUI.transform.position = position;

            //Create the buttons to switch between hands if needed
            CreateHandButtons(i, xPosition, amountOfCardsToShow);

            cardUI.Card = data;
            cardUI.cardContainer = this;
            cardUI.EnergyText = EnergyText;
            SetCardText(cardUI.gameObject, data);
            ApplyCardBackgroundColor(cardUI.gameObject, data.Type);

            shownCards.Add(cardUI);
        }
    }

    /// <summary>
    /// Create buttons to switch between hands if the player has multiple hands
    /// </summary>
    private void CreateHandButtons(int index, float xPosition, float amountOfCardsToShow)
    {
        if (currentRoundCards.Count > maxCardsShown)
        {
            //Creates the button to go to the previous hand if this is the first card in the hand
            if (index == 0 && currentHand != 0)
            {
                //Create the button and calculate the position for the button
                previousHandButton = Instantiate(handButtonPrefab, cardParent, false).GetComponent<HandButton>();
                Vector2 position = new(xPosition - (cardWidth / 2) - previousHandButton.transform.localScale.x, cardParent.anchoredPosition.y);

                //Set position and rotation for the button
                previousHandButton.transform.position = position;
                previousHandButton.transform.rotation = Quaternion.Euler(0, 0, 90);

                //Set the button variables
                previousHandButton.Buttontype = HandButtonType.Previous;
                previousHandButton.cardContainer = this;
            }

            //Creates the button to go to the next hand if this is the last card in the hand
            if (index == amountOfCardsToShow - 1 && currentHand != hands.Count - 1)
            {
                //Create the button and calculate the position for the button
                nextHandButton = Instantiate(handButtonPrefab, cardParent, false).GetComponent<HandButton>();
                Vector2 position = new(xPosition + (cardWidth / 2) + nextHandButton.transform.localScale.x, cardParent.anchoredPosition.y);

                //Set position and rotation for the button
                nextHandButton.transform.position = position;
                nextHandButton.transform.rotation = Quaternion.Euler(0, 0, -90);

                //Set the button variables
                nextHandButton.Buttontype = HandButtonType.Next;
                nextHandButton.cardContainer = this;
            }
        }
    }

    #endregion

    #region Destroy UI

    /// <summary>
    /// Destroys the currently shown cards
    /// </summary>
    public void DestroyCurrectCards()
    {
        foreach (CardController cardUI in shownCards)
        {
            Destroy(cardUI.gameObject);
        }

        //Clear the list of shown cards
        shownCards.Clear();
    }

    /// <summary>
    /// Destroys the hand buttons if they exist
    /// </summary>
    public void DestroyHandButtons()
    {
        if (nextHandButton != null)
        {
            Destroy(nextHandButton.gameObject);
            nextHandButton = null;
        }
        if (previousHandButton != null)
        {
            Destroy(previousHandButton.gameObject);
            previousHandButton = null;
        }
    }

    #endregion

    #region Card seting functions

    /// <summary>
    /// Set the card variable for the card text correct
    /// </summary>
    /// <param name="cardRoot">The card UI root object</param>
    /// <param name="data">The card data to display</param>
    private void SetCardText(GameObject cardRoot, Card data)
    {
        //Get the text components from the card
        TextMeshProUGUI[] texts = cardRoot.GetComponentsInChildren<TextMeshProUGUI>(true);

        //Set the text components of the card
        foreach (var t in texts)
        {
            string lname = t.gameObject.name.ToLower();
            if (lname.Contains("name"))
            {
                t.text = data.Name;
            }
            else if (lname.Contains("type"))
            {
                t.text = data.Type.ToString();
            }
            else if (lname.Contains("value"))
            {
                t.text = data.Value.ToString();
            }
            else if (lname.Contains("energy"))
            {
                t.text = data.EnergyCost.ToString();
            }
            else if (lname.Contains("description"))
            {
                // Educational text: short gameplay line + financial lesson.
                t.text = data.Description;
            }
        }
    }

    /// <summary>
    /// Apply a background to the card based on the type
    /// </summary>
    /// <param name="cardRoot">The card UI root object</param>
    /// <param name="cardType">The card type</param>
    private void ApplyCardBackgroundColor(GameObject cardRoot, CardType cardType)
    {
        Image backgroundImage = null;
        Image[] images = cardRoot.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image != null && image.gameObject.name.ToLower() == "background")
            {
                backgroundImage = image;
                break;
            }
        }

        if (backgroundImage != null)
        {
            if (cardType == CardType.Income)
            {
                backgroundImage.color = new Color(0.96f, 0.92f, 0.65f, 1f);
            }
            else if (cardType == CardType.Consumable)
            {
                backgroundImage.color = new Color(0.65f, 0.8f, 0.95f, 1f);
            }
            else if (cardType == CardType.Chance)
            {
                backgroundImage.color = new Color(0.95f, 0.7f, 0.7f, 1f);
            }
        }
    }

    #endregion
}