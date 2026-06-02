using System;

namespace Assets.Scripts.Content
{
    /// <summary>Whether an EventDefinition is shown as a one-button popup or a two-button choice.</summary>
    public enum EventKind
    {
        Event,
        Choice
    }

    /// <summary>
    /// Data row describing a random or scripted event/choice. Effects fire on the
    /// Continue / button click, not when the popup appears, so the player reads the
    /// flavor text before its consequences land.
    /// </summary>
    public class EventDefinition
    {
        public string Id;
        public string Title;
        public string Description;
        public EventKind Kind = EventKind.Event;

        // Used when Kind == Event.
        public EffectStep[] OnContinueEffects = Array.Empty<EffectStep>();

        // Used when Kind == Choice.
        public string ButtonOneText = "OK";
        public string ButtonTwoText = "Cancel";
        public EffectStep[] ButtonOneEffects = Array.Empty<EffectStep>();
        public EffectStep[] ButtonTwoEffects = Array.Empty<EffectStep>();

        // Weighted random-pool selection.
        public float Weight = 1f;

        // Phase availability (inclusive).
        public int MinPhase = 1;
        public int MaxPhase = 5;

        /// <summary>
        /// If true, this event can only fire at most once per run. WorldController
        /// auto-sets RunFlags["once_<Id>"] after the popup is dismissed; the same
        /// flag is checked by MatchesContext so the row drops out of the pool after.
        /// </summary>
        public bool OncePerRun;

        /// <summary>Optional predicate gating whether this event can fire for a given context.</summary>
        public Func<EventContext, bool> Condition;

        /// <summary>True if this event is valid for the given context (phase + once-flag + predicate).</summary>
        public bool MatchesContext(EventContext ctx)
        {
            if (ctx == null) return false;
            if (ctx.Phase < MinPhase || ctx.Phase > MaxPhase) return false;
            if (OncePerRun && RunFlags.Get(RunFlags.OnceKey(Id))) return false;
            return Condition == null || Condition(ctx);
        }
    }
}
