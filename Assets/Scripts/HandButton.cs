using UnityEngine;
using Assets.Scripts;

public class HandButton : MonoBehaviour
{
    public HandButtonType Buttontype;
    public CardContainer cardContainer { private get;  set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The action performed when the button is clicked
    /// </summary>
    public void OnClick()
    {
        switch (Buttontype)
        {
            case HandButtonType.Next:
                cardContainer.ShowNextHand();
                break;

            case HandButtonType.Previous:
                cardContainer.ShowPreviousHand();
                break;
        }
    }
}
