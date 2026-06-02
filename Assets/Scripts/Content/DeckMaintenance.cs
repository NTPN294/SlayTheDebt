using Assets.Scripts;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Deck cleanup that runs at scene-load time.
    ///
    /// Why this exists: every Income / Asset / Chance card stays in the deck after
    /// being played, so without expiration the player would still be holding their
    /// Phase-1 Scholarship or student Loan at age 60. ExpireOutOfPhase removes any
    /// card whose CardDefinition has MaxPhase &lt; currentPhase so each life stage
    /// only carries the content that actually belongs to that stage.
    ///
    /// Intentional limitations:
    ///  - Only acts on cards that have a registered CardDefinition. Cards with no
    ///    definition (defensive: shouldn't exist) are left alone.
    ///  - Does nothing within a phase. A Scholarship in Phase 1 is still replayable
    ///    until the phase ends — recurring income is meant to recur per phase.
    ///    Cards that thematically should be one-shot are marked Consumable in
    ///    CardDatabase instead, so the existing CardController removal handles
    ///    them after a single play.
    /// </summary>
    public static class DeckMaintenance
    {
        public static int ExpireOutOfPhase(Player player, int currentPhase)
        {
            if (player == null || player.Cards == null) return 0;

            int removed = 0;
            for (int i = player.Cards.Count - 1; i >= 0; i--)
            {
                Card card = player.Cards[i];
                if (card == null) continue;

                CardDefinition def = CardDatabase.GetById(card.Name);
                if (def == null) continue;

                if (def.MaxPhase < currentPhase)
                {
                    player.Cards.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }
    }
}
