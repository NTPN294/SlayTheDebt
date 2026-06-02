using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class FindWorkChoise : IChoise
    {
        private string title = Names.FindWork;
        private string description = "There is an oppertunity to get a job!";
        private string buttonOneText = "Take the job";
        private string buttonTwoText = "Refuse the job";
        private CardContainer cardContainer;

        public string Title => title;

        public string Description => description;

        public string ButtonOneText => buttonOneText;

        public string ButtonTwoText => buttonTwoText;

        public FindWorkChoise(CardContainer cardContainer)
            => this.cardContainer = cardContainer;

        /// <summary>
        /// Preform the action for button one
        /// </summary>
        public void ButtonOneAction()
            //Add a low work card to the card container
            => cardContainer.AddCard(Names.WorkCardLow);

        /// <summary>
        /// Preform the action for button two
        /// </summary>
        public void ButtonTwoAction()
        {
            //Do nothing
        }
    }
}
