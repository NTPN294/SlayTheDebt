namespace Assets.Scripts
{
    public interface IEvent
    {
        public string Title { get; }
        public string Description { get; }
    }

    public interface IChoise
    {
        public string Title { get; }
        public string Description { get; }
        public string ButtonOneText { get; }
        public string ButtonTwoText { get; }

        /// <summary>
        /// Preform the action of button one
        /// </summary>
        public void ButtonOneAction();

        /// <summary>
        /// Preform the action of button two
        /// </summary>
        public void ButtonTwoAction();
    }
}
