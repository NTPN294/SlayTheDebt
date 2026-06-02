using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Resolves EffectStep lists against an EventContext: mutates Player stats,
    /// money, deck contents and RunFlags, then refreshes any visible HUD.
    ///
    /// Effects are applied in the order they appear in the list.
    /// </summary>
    public static class ContentEffectApplier
    {
        public static void Apply(IEnumerable<EffectStep> steps, EventContext ctx)
        {
            if (steps == null || ctx == null) return;

            bool playerTouched = false;

            foreach (EffectStep step in steps)
            {
                if (step == null) continue;

                switch (step.Kind)
                {
                    case EffectKind.MoneyDelta:
                        if (ApplyMoney(ctx, step.Amount)) playerTouched = true;
                        break;

                    case EffectKind.StatDelta:
                        if (ApplyStat(ctx, step.Stat, step.Amount)) playerTouched = true;
                        break;

                    case EffectKind.AddCard:
                        ApplyAddCard(ctx, step.CardId);
                        break;

                    case EffectKind.RemoveCardByTag:
                        ApplyRemoveByTag(ctx, step.Tag);
                        break;

                    case EffectKind.RemoveCardByName:
                        ApplyRemoveByName(ctx, step.CardId);
                        break;

                    case EffectKind.SetFlag:
                        RunFlags.Set(step.FlagKey, step.FlagValue);
                        break;
                }
            }

            if (playerTouched)
                RefreshHud(ctx);
        }

        static bool ApplyMoney(EventContext ctx, int amount)
        {
            if (ctx.Player == null) return false;
            ctx.Player.Money += amount;
            GameSession.UpdateHighestMoney();

            // Keep the in-battle money label in sync so PlayerController.noTurnsLeft
            // sees the correct base when the next turn ends.
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null && pc.playerMoneyText != null)
                pc.playerMoneyText.text = ctx.Player.Money.ToString();

            return true;
        }

        static bool ApplyStat(EventContext ctx, StatKind stat, int amount)
        {
            if (ctx.Player == null) return false;

            switch (stat)
            {
                case StatKind.Happiness:
                    ctx.Player.Happiness = Mathf.Clamp(ctx.Player.Happiness + amount, 0, 100);
                    break;
                case StatKind.Health:
                    ctx.Player.Health = Mathf.Clamp(ctx.Player.Health + amount, 0, 100);
                    break;
                case StatKind.Career:
                    ctx.Player.Career = Mathf.Clamp(ctx.Player.Career + amount, 0, 100);
                    break;
                case StatKind.Relationships:
                    ctx.Player.Relationships = Mathf.Clamp(ctx.Player.Relationships + amount, 0, 100);
                    break;
            }
            return true;
        }

        static void ApplyAddCard(EventContext ctx, string cardId)
        {
            if (ctx.Deck == null || string.IsNullOrEmpty(cardId)) return;

            // CardContainer.AddCard goes through Generator.GenerateCard -> CardDatabase.
            // We pass the id directly; CardDatabase.GetById is the source of truth.
            if (CardDatabase.GetById(cardId) == null)
            {
                Debug.LogWarning($"ContentEffectApplier: AddCard skipped, unknown card id '{cardId}'.");
                return;
            }

            ctx.Deck.AddCard(cardId);
        }

        static void ApplyRemoveByTag(EventContext ctx, string tag)
        {
            if (ctx.Deck == null || string.IsNullOrEmpty(tag)) return;

            Card target = null;
            foreach (Card c in ctx.Deck.GetPlayerDeck())
            {
                CardDefinition def = CardDatabase.GetById(c.Name);
                if (def != null && def.HasTag(tag))
                {
                    target = c;
                    break;
                }
            }

            if (target == null) return;

            ctx.Deck.RemoveCardFromHand(target);
            ctx.Deck.RemoveCardFromDeck(target);
        }

        static void ApplyRemoveByName(EventContext ctx, string id)
        {
            if (ctx.Deck == null || string.IsNullOrEmpty(id)) return;

            Card target = null;
            foreach (Card c in ctx.Deck.GetPlayerDeck())
            {
                if (string.Equals(c.Name, id, System.StringComparison.OrdinalIgnoreCase))
                {
                    target = c;
                    break;
                }
            }

            if (target == null) return;

            ctx.Deck.RemoveCardFromHand(target);
            ctx.Deck.RemoveCardFromDeck(target);
        }

        static void RefreshHud(EventContext ctx)
        {
            StatusController hud = Object.FindAnyObjectByType<StatusController>();
            if (hud != null) hud.LoadFromGameSession();
        }
    }
}
