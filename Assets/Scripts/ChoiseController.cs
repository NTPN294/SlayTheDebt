using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;

public class ChoiseController : MonoBehaviour
{
    public IChoise choiseInstance { set; private get; }

    Action _onComplete;

    //Unity variables
    [Header("Text")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI buttonOneText;
    public TextMeshProUGUI buttonTwoText;

    public void SetOnComplete(Action onComplete)
    {
        _onComplete = onComplete;
    }

    void CompleteAndDestroy()
    {
        _onComplete?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Runs the button one logic and destroys the choise
    /// </summary>
    public void OnButtonOneClick()
    {
        choiseInstance.ButtonOneAction();
        CompleteAndDestroy();
    }

    /// <summary>
    /// Runs the button two logic and destroys the choise
    /// </summary>
    public void OnButtonTwoClick()
    {
        choiseInstance.ButtonTwoAction();
        CompleteAndDestroy();
    }

    /// <summary>
    /// Set the text of the choise
    /// </summary>
    public void SetText(string title, string description, string buttonOne, string buttonTwo)
    {
        titleText.text = title;
        descriptionText.text = description;
        buttonOneText.text = buttonOne;
        buttonTwoText.text = buttonTwo;
    }
}
