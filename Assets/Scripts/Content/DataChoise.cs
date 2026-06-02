namespace Assets.Scripts.Content
{
    /// <summary>
    /// Lightweight IChoise adapter for a data-driven EventDefinition. The button
    /// actions apply per-branch EffectSteps via ContentEffectApplier using the
    /// EventContext captured at spawn time.
    /// </summary>
    public class DataChoise : IChoise
    {
        public EventDefinition Definition { get; }
        readonly EventContext _context;

        public DataChoise(EventDefinition definition, EventContext context)
        {
            Definition = definition;
            _context = context;
        }

        public string Title => Definition?.Title ?? string.Empty;
        public string Description => Definition?.Description ?? string.Empty;
        public string ButtonOneText => Definition?.ButtonOneText ?? "OK";
        public string ButtonTwoText => Definition?.ButtonTwoText ?? "Cancel";

        public void ButtonOneAction()
        {
            if (Definition == null) return;
            ContentEffectApplier.Apply(Definition.ButtonOneEffects, _context);
        }

        public void ButtonTwoAction()
        {
            if (Definition == null) return;
            ContentEffectApplier.Apply(Definition.ButtonTwoEffects, _context);
        }
    }
}
