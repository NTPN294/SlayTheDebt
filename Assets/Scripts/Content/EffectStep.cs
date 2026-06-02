namespace Assets.Scripts.Content
{
    /// <summary>
    /// Discrete gameplay effects an event or choice can produce.
    /// Dispatched by ContentEffectApplier.
    /// </summary>
    public enum EffectKind
    {
        MoneyDelta,
        StatDelta,
        AddCard,
        RemoveCardByTag,
        RemoveCardByName,
        SetFlag
    }

    /// <summary>Player stat targeted by a StatDelta effect.</summary>
    public enum StatKind
    {
        Happiness,
        Health,
        Career,
        Relationships
    }

    /// <summary>
    /// Single effect "step" with only the fields relevant to its Kind populated.
    /// Use the static factory helpers (Money, Happiness, AddCard, ...) for readable content.
    /// </summary>
    public class EffectStep
    {
        public EffectKind Kind;
        public int Amount;
        public StatKind Stat;
        public string CardId;
        public string Tag;
        public string FlagKey;
        public bool FlagValue;

        public static EffectStep Money(int amount) =>
            new EffectStep { Kind = EffectKind.MoneyDelta, Amount = amount };

        public static EffectStep StatChange(StatKind stat, int amount) =>
            new EffectStep { Kind = EffectKind.StatDelta, Stat = stat, Amount = amount };

        public static EffectStep Happiness(int amount) => StatChange(StatKind.Happiness, amount);
        public static EffectStep Health(int amount) => StatChange(StatKind.Health, amount);
        public static EffectStep Career(int amount) => StatChange(StatKind.Career, amount);
        public static EffectStep Relationships(int amount) => StatChange(StatKind.Relationships, amount);

        public static EffectStep AddCard(string id) =>
            new EffectStep { Kind = EffectKind.AddCard, CardId = id };

        public static EffectStep RemoveByTag(string tag) =>
            new EffectStep { Kind = EffectKind.RemoveCardByTag, Tag = tag };

        public static EffectStep RemoveByName(string id) =>
            new EffectStep { Kind = EffectKind.RemoveCardByName, CardId = id };

        public static EffectStep SetFlag(string key, bool value = true) =>
            new EffectStep { Kind = EffectKind.SetFlag, FlagKey = key, FlagValue = value };
    }
}
