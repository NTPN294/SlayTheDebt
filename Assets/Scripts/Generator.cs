using System.Collections.Generic;
using Assets.Scripts.Content;

namespace Assets.Scripts
{
    /// <summary>
    /// Thin factory facade kept for backward compatibility.
    /// All card lookups go through CardDatabase; all event/choice lookups go
    /// through EventDatabase. To add content, edit those databases instead.
    /// </summary>
    static class Generator
    {
        /// <summary>
        /// Generate multiple cards from a list of card ids (or display names — same thing).
        /// Unknown ids are skipped with a warning so a typo never breaks the start deck.
        /// </summary>
        public static List<Card> GenerateCards(string[] cardNames)
        {
            List<Card> cards = new();
            if (cardNames == null) return cards;

            foreach (string cardName in cardNames)
            {
                Card card = GenerateCard(cardName);
                if (card != null)
                    cards.Add(card);
                else
                    UnityEngine.Debug.LogWarning($"Generator.GenerateCards: unknown card id '{cardName}'.");
            }

            return cards;
        }

        /// <summary>
        /// Generate one card by id. Returns null when the id is unknown.
        /// </summary>
        public static Card GenerateCard(string cardName)
        {
            return CardDatabase.BuildCard(cardName);
        }

        /// <summary>
        /// Generate an event by id. Returns null if the id is unknown or refers to a choice.
        /// The cardContainer / playerCards parameters are kept for API compatibility but
        /// no longer participate — effects are applied via ContentEffectApplier when the
        /// player clicks Continue.
        /// </summary>
        public static IEvent GenerateEvent(string eventName, CardContainer cardContainer, List<Card> playerCards)
        {
            EventDefinition def = EventDatabase.GetById(eventName);
            if (def == null || def.Kind != EventKind.Event)
                return null;
            return new DataEvent(def);
        }

        /// <summary>
        /// Generate a choice by id. Returns null if the id is unknown or refers to a
        /// single-button event. The context is built from the current GameSession.
        /// </summary>
        public static IChoise GenerateChoise(string choiseName, CardContainer cardContainer)
        {
            EventDefinition def = EventDatabase.GetById(choiseName);
            if (def == null || def.Kind != EventKind.Choice)
                return null;

            EventContext ctx = new EventContext
            {
                Player = GameSession.Player,
                World = GameSession.WorldStats,
                Deck = cardContainer,
                Phase = GameSession.CurrentPhase
            };

            return new DataChoise(def, ctx);
        }
    }
}
