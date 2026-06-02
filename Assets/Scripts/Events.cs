using System.Collections.Generic;

namespace Assets.Scripts
{
    public class FiredEvent : IEvent
    {
        private string title = Names.Fired;
        private string description = "You got fired from your job and lost that income.";

        public string Title => title;
        public string Description => description;

        public FiredEvent(CardContainer cardContainer, List<Card> playerCards)
        {
            Card card = null;

            foreach(Card c in playerCards)
            {
                if (c.Name.Contains("Work"))
                    card =  c;
            }

            cardContainer.RemoveCardFromHand(card);
            cardContainer.RemoveCardFromDeck(card);
        }
    }

    public class BreakUpEvent : IEvent
    {
        private string title = Names.BreakUp;
        private string description = "You broke up with your partner.";

        public string Title => title;
        public string Description => description;

        public BreakUpEvent(CardContainer cardContainer, List<Card> playerCards)
        {
            //TODO: Implement the break up event, I dont know what the effect should be yet
        }
    }
}
