using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// One scripted event per phase, each carrying real gameplay effects. Called from
    /// WorldController.TrySpawnScriptedEvent. Phases are 1-based to match
    /// GameSession.CurrentPhase.
    /// </summary>
    public static class ScriptedEventDatabase
    {
        static readonly Dictionary<int, EventDefinition> _byPhase = new Dictionary<int, EventDefinition>();
        static EventDefinition _fallback;
        static bool _initialized;

        public static EventDefinition Get(int phase)
        {
            EnsureInitialized();
            return _byPhase.TryGetValue(phase, out var def) ? def : _fallback;
        }

        static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            Seed();
        }

        static void Seed()
        {
            // ----- Phase 1: Move Out -----
            // First-time independence: pay deposit + gain a rented place (no resale
            // value), or stay home and keep the family bond strong.
            _byPhase[1] = new EventDefinition
            {
                Id = "scripted_move_out",
                Title = "Move Out",
                Description = "Your parents want you out of the house. Time to spread your wings — or stay nested?",
                Kind = EventKind.Choice,
                ButtonOneText = "Rent a room",
                ButtonTwoText = "Stay at home",
                ButtonOneEffects = new[] {
                    EffectStep.Money(-300),
                    EffectStep.AddCard(Names.SharedAppartment),
                    EffectStep.Happiness(10),
                    EffectStep.Relationships(-5)
                },
                ButtonTwoEffects = new[] {
                    EffectStep.Happiness(-5),
                    EffectStep.Relationships(10)
                },
                MinPhase = 1, MaxPhase = 1
            };

            // ----- Phase 2: Rent Increase -----
            // One-shot tax on early career savings.
            _byPhase[2] = new EventDefinition
            {
                Id = "scripted_rent_increase",
                Title = "Rent Increase",
                Description = "Your landlord raises the rent. Welcome to grown-up bills.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Money(-400),
                    EffectStep.Happiness(-5)
                },
                MinPhase = 2, MaxPhase = 2
            };

            // ----- Phase 3: Family Pressure -----
            // Identity check: family or career? Both branches hurt the other side.
            _byPhase[3] = new EventDefinition
            {
                Id = "scripted_family_pressure",
                Title = "Family Pressure",
                Description = "Your family wants more of your time. Your boss wants more of your hours.",
                Kind = EventKind.Choice,
                ButtonOneText = "Family comes first",
                ButtonTwoText = "Career comes first",
                ButtonOneEffects = new[] {
                    EffectStep.Relationships(20),
                    EffectStep.Happiness(5),
                    EffectStep.Career(-15)
                },
                ButtonTwoEffects = new[] {
                    EffectStep.Career(20),
                    EffectStep.Money(200),
                    EffectStep.Relationships(-15),
                    EffectStep.Happiness(-5)
                },
                MinPhase = 3, MaxPhase = 3
            };

            // ----- Phase 4: Medical Bill -----
            // Forces budgeting around health costs.
            _byPhase[4] = new EventDefinition
            {
                Id = "scripted_medical_bill",
                Title = "Medical Bill",
                Description = "An unexpected bill arrives in the mail. The numbers sting.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Money(-500),
                    EffectStep.Health(-10),
                    EffectStep.Happiness(-5)
                },
                MinPhase = 4, MaxPhase = 4
            };

            // ----- Phase 5: Final Stretch -----
            // Run climax: bet big with a chance card or settle into a smoother finish.
            _byPhase[5] = new EventDefinition
            {
                Id = "scripted_final_stretch",
                Title = "Final Stretch",
                Description = "You're close to clearing your debt for good. Push hard or coast in?",
                Kind = EventKind.Choice,
                ButtonOneText = "Bet on a big payoff",
                ButtonTwoText = "Coast into retirement",
                ButtonOneEffects = new[] {
                    EffectStep.Money(-300),
                    EffectStep.AddCard("Legacy")
                },
                ButtonTwoEffects = new[] {
                    EffectStep.Happiness(15),
                    EffectStep.Health(10),
                    EffectStep.Relationships(5)
                },
                MinPhase = 5, MaxPhase = 5
            };

            _fallback = new EventDefinition
            {
                Id = "scripted_fallback",
                Title = "Life Event",
                Description = "Something important happens.",
                Kind = EventKind.Event,
                OnContinueEffects = new EffectStep[0]
            };
        }
    }
}
