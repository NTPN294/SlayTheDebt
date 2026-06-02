namespace Assets.Scripts.Content
{
    /// <summary>
    /// Snapshot of the things events / effects need to reason about and mutate.
    /// Built fresh by WorldController each time a popup is spawned.
    /// </summary>
    public class EventContext
    {
        public Player Player;
        public WorldStats World;
        public CardContainer Deck;
        public int Phase;
    }
}
