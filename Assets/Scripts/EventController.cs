using System;
using TMPro;
using UnityEngine;

public class EventController : MonoBehaviour
{
    Action _onComplete;

    //Unity variables
    [Header("Text")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public void SetOnComplete(Action onComplete)
    {
        _onComplete = onComplete;
    }

    /// <summary>
    /// Destroy the event when clicked on continu
    /// </summary>
    public void OnButtonClick()
    {
        _onComplete?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Set the text of the event
    /// </summary>
    public void SetText(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
    }
}