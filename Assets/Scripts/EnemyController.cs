using UnityEngine;
using TMPro;

public class EnemyController : MonoBehaviour
{
    public TMP_Text moneyText;
    public int InitialMoney = 500;

    void Start()
    {
        moneyText.text = InitialMoney.ToString();
    }

    /// <summary>
    /// Has the enemy take damage based on the card value
    /// </summary>
    /// <param name="cardValue">The amount of damage delt by the card</param>
    public void ApplyCardValue(int cardValue)
    {
        int newMoney = InitialMoney - cardValue;
        moneyText.text = newMoney.ToString();
        InitialMoney = newMoney;
    }

    /// <summary>
    /// Resets enemy debt for a new battle step.
    /// </summary>
    public void ResetForNewBattle(int startingDebt = 500)
    {
        InitialMoney = startingDebt;
        moneyText.text = InitialMoney.ToString();
    }
}