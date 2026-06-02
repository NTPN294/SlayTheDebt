using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Content;

public class StartScript : MonoBehaviour
{
    public TMP_Text MoneyText;
    public Toggle Low;
    public Toggle Med;
    public Toggle High;
    public Toggle Home;
    public Toggle Studio;
    public Toggle Shared;
    public Toggle Job;
    public Toggle Loan;
    public Slider Happiness;
    public Slider Hunger;
    public Slider Career;
    public Slider Relationships;

    void Start()
    {

    }

    void Update()
    {

    }

    public void click()
    {
        List<Card> cards = GetCards();

        // Lees stat-waarden uit de StatusController die al correct berekend zijn via de toggles
        StatusController sc = FindFirstObjectByType<StatusController>();

        GameSession.ResetRunStats();
        // Clear per-run event flags (e.g. "had_pregnancy") so the new run starts fresh.
        RunFlags.Reset();

        int startingMoney = int.Parse(MoneyText.text.Replace("€", "").Trim());
        GameSession.Player = new Player
        {
            Money = startingMoney,
            Cards = cards,
            Happiness = sc != null ? (int)sc.sliderHappiness.value : 50,
            Health = sc != null ? (int)sc.sliderHealth.value : 50,
            Career = sc != null ? (int)sc.sliderCareer.value : 50,
            Relationships = sc != null ? (int)sc.sliderRelationship.value : 50,
            Energy = 5
        };
        GameSession.UpdateHighestMoney();

        SceneManager.LoadScene("PhaseTutorialScene");
    }

    /// <summary>
    /// Gives the staring cards
    /// </summary>
    /// <returns>REturns a list of starting cards</returns>
    public List<Card> GetCards()
    {
        List<Card> cards = new List<Card>();

        //Sets the starting cards based on the filled in form
        if (Low.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
            {
                Names.WorkCardLow,
                Names.Bike,
                Names.TutorJob,
                Names.Internship,
                Names.LateNightStudy,
                Names.Gamble,
                Names.Gamble,
                Names.Gift
            });

            cards.AddRange(newCards);
        }
        else if (Med.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
            {
                Names.WorkCardMid,
                Names.MotorCycle,
                Names.Scholarship,
                Names.Internship,
                Names.Textbooks,
                Names.Gamble,
                Names.Gamble,
                Names.Gift,
                Names.Gift
            });

            cards.AddRange(newCards);
        }
        else if (High.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
            {
                Names.WorkCardHigh,
                Names.Car,
                Names.Scholarship,
                Names.Festival,
                Names.Textbooks,
                Names.Gamble,
                Names.Gamble,
                Names.Gift,
                Names.Gift,
                Names.Gift
            });

            cards.AddRange(newCards);
        }

        if (Home.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
            {
                Names.Food,
                Names.Food,
                Names.Food
            });

            cards.AddRange(newCards);
        }
        else if (Studio.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
            {
                Names.Food,
                Names.Studio
            });

            cards.AddRange(newCards);
        }
        else if (Shared.isOn)
        {
            List<Card> newCards = Generator.GenerateCards(new[]
           {
                Names.Food,
                Names.SharedAppartment
            });

            cards.AddRange(newCards);
        }

        if (Job.isOn)
        {
            cards.Add(Generator.GenerateCard(Names.WorkCardLow));
        }

        if (Loan.isOn)
        {
            cards.Add(Generator.GenerateCard(Names.Loan));
        }

        return cards;
    }
}
