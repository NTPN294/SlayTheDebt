namespace Assets.Scripts
{
    /// <summary>
    /// A enum for the card types
    /// </summary>
    public enum CardType
    {
        Income,
        Asset,
        Consumable,
        Chance
    }

    /// <summary>
    /// A enum for the card types
    /// </summary>
    public enum PlayerStartState
    {
        One,
        Two,
        Three,
        Four
    }

    /// <summary>
    /// A enum for the hand button types
    /// </summary>
    public enum  HandButtonType
    {
        Next,
        Previous
    }

    /// <summary>
    /// Steps within a single phase of the run.
    /// </summary>
    public enum PhaseStep
    {
        Battle1,
        RandomEvent1,
        Battle2,
        RandomEvent2,
        Shop1,
        Battle3,
        RandomEvent3,
        Battle4,
        ScriptedEvent,
        Shop2,
        Complete
    }
}
