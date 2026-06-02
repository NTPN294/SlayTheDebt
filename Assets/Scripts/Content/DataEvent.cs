namespace Assets.Scripts.Content
{
    /// <summary>
    /// Lightweight IEvent adapter for a data-driven EventDefinition. WorldController
    /// applies the event's effects in the wrapped onComplete callback, so this type
    /// only needs to expose Title / Description.
    /// </summary>
    public class DataEvent : IEvent
    {
        public EventDefinition Definition { get; }

        public DataEvent(EventDefinition definition)
        {
            Definition = definition;
        }

        public string Title => Definition?.Title ?? string.Empty;
        public string Description => Definition?.Description ?? string.Empty;
    }
}
