using System;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Data row describing a single card type. Build() spawns a runtime Card.
    /// The Id doubles as the display name shown on the card UI; it must be unique
    /// across the whole CardDatabase so a runtime Card.Name can be mapped back
    /// to its definition (used for tag-based removal effects).
    /// </summary>
    public class CardDefinition
    {
        public string Id;
        public string Description;
        public CardType Type;
        public int Value;
        public int EnergyCost;

        // Stat deltas applied when the card is played (CardController.ApplyStatChanges).
        public int Happiness;
        public int Health;
        public int Career;
        public int Relationships;

        // Phase availability (inclusive). Defaults span the whole run.
        public int MinPhase = 1;
        public int MaxPhase = 5;

        // Free-form tags ("work", "asset", "luxury", ...) for pool filters and tag-based removal.
        public string[] Tags = Array.Empty<string>();

        /// <summary>Whether this card is allowed in the given phase.</summary>
        public bool IsInPhase(int phase) => phase >= MinPhase && phase <= MaxPhase;

        /// <summary>Whether this card carries the given tag.</summary>
        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || Tags == null) return false;
            for (int i = 0; i < Tags.Length; i++)
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Construct a runtime Card from this definition.</summary>
        public Card Build()
        {
            return new Card(Id, Type, Value, EnergyCost, Description,
                            Happiness, Health, Career, Relationships);
        }
    }
}
