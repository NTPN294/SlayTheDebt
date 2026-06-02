using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Static catalog of all random events and choices. Phase-bucketed pools are built
    /// once on first access. WorldController draws from these via GetWeightedRandom.
    ///
    /// Authoring rules:
    ///  - Big "life-changing" events (promotion, layoff, burnout, crash, divorce,
    ///    inheritance, retirement crisis) set OncePerRun = true so they cannot
    ///    repeat in the same run.
    ///  - Smaller recurring noise (inflation, bills, small social events) stays
    ///    repeatable but lands in the short cooldown queue via RunFlags.RecordEvent.
    ///  - Add tag-based Conditions to make events feel earned, not random.
    /// </summary>
    public static class EventDatabase
    {
        static readonly Dictionary<string, EventDefinition> _byId =
            new Dictionary<string, EventDefinition>();
        static readonly List<EventDefinition> _all = new List<EventDefinition>();

        static List<EventDefinition>[] _byPhase;
        static bool _initialized;

        public static EventDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureInitialized();
            _byId.TryGetValue(id, out EventDefinition def);
            return def;
        }

        /// <summary>
        /// Picks a weighted-random EventDefinition from the phase's pool, filtered by Kind
        /// and the EventContext. Recently-seen events drop to a fallback bucket so they
        /// only re-appear when nothing else is available.
        /// </summary>
        public static EventDefinition GetWeightedRandom(int phase, EventKind kind, EventContext ctx)
        {
            EnsureInitialized();

            List<EventDefinition> phasePool = GetPhasePool(phase);
            if (phasePool == null || phasePool.Count == 0)
                return null;

            float totalWeight = 0f;
            var eligible = new List<EventDefinition>();
            var fallback = new List<EventDefinition>();
            float fallbackWeight = 0f;

            foreach (var def in phasePool)
            {
                if (def.Kind != kind) continue;
                if (!def.MatchesContext(ctx)) continue;
                float w = def.Weight > 0f ? def.Weight : 1f;

                if (RunFlags.RecentlySeen(def.Id))
                {
                    fallback.Add(def);
                    fallbackWeight += w;
                    continue;
                }

                totalWeight += w;
                eligible.Add(def);
            }

            if (eligible.Count == 0)
            {
                eligible = fallback;
                totalWeight = fallbackWeight;
            }

            if (eligible.Count == 0) return null;

            float roll = Random.value * totalWeight;
            float acc = 0f;
            foreach (var def in eligible)
            {
                acc += def.Weight > 0f ? def.Weight : 1f;
                if (roll <= acc) return def;
            }

            return eligible[eligible.Count - 1];
        }

        static List<EventDefinition> GetPhasePool(int phase)
        {
            if (_byPhase == null) return null;
            if (phase < 0 || phase >= _byPhase.Length) return null;
            return _byPhase[phase];
        }

        static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            Seed();

            foreach (var def in _all)
            {
                if (def == null || string.IsNullOrEmpty(def.Id)) continue;
                _byId[def.Id] = def;
            }

            int maxPhase = GameSession.MaxPhases + 1;
            _byPhase = new List<EventDefinition>[maxPhase + 1];
            for (int i = 0; i <= maxPhase; i++)
                _byPhase[i] = new List<EventDefinition>();

            foreach (var def in _all)
            {
                int lo = Mathf.Max(def.MinPhase, 0);
                int hi = Mathf.Min(def.MaxPhase, maxPhase);
                for (int p = lo; p <= hi; p++)
                    _byPhase[p].Add(def);
            }
        }

        static void Add(EventDefinition def) => _all.Add(def);

        // ---------------------------------------------------------------------
        // Helper predicates
        // ---------------------------------------------------------------------
        static bool DeckHasTag(EventContext ctx, string tag)
        {
            if (ctx == null || ctx.Deck == null) return false;
            foreach (var c in ctx.Deck.GetPlayerDeck())
            {
                var def = CardDatabase.GetById(c.Name);
                if (def != null && def.HasTag(tag)) return true;
            }
            return false;
        }

        // ---------------------------------------------------------------------
        // Seed content
        // ---------------------------------------------------------------------
        static void Seed()
        {
            // ----- Legacy events (migrated; preserve Names ids) -----
            Add(new EventDefinition {
                Id = Names.Fired,
                Title = "Fired",
                Description = "You lose your job. Bills do not pause.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.RemoveByTag("work"),
                    EffectStep.Career(-10),
                    EffectStep.Happiness(-8)
                },
                Weight = 0.6f,
                MinPhase = 1, MaxPhase = 5,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "work")
                    && ctx.Player != null
                    && ctx.World != null
                    && ctx.Player.Career + ctx.World.wealth < 100
            });

            Add(new EventDefinition {
                Id = Names.BreakUp,
                Title = "Break up",
                Description = "You and your partner go separate ways.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Relationships(-20),
                    EffectStep.Happiness(-15)
                },
                Weight = 0.6f,
                MinPhase = 1, MaxPhase = 4,
                OncePerRun = true,
                Condition = ctx => ctx.Player != null && ctx.Player.Relationships < 35
            });

            Add(new EventDefinition {
                Id = Names.FindWork,
                Title = "Work opportunity",
                Description = "A friend tips you off about a job opening.",
                Kind = EventKind.Choice,
                ButtonOneText = "Take the job",
                ButtonTwoText = "Pass on it",
                ButtonOneEffects = new[] {
                    EffectStep.AddCard(Names.WorkCardLow),
                    EffectStep.Career(5),
                    EffectStep.Relationships(-3)
                },
                ButtonTwoEffects = new[] { EffectStep.Happiness(3) },
                Weight = 0.8f,
                MinPhase = 2, MaxPhase = 3,
                Condition = ctx => ctx.Player != null
                    && ctx.World != null
                    && ctx.Player.Career + ctx.World.wealth > 30
            });

            // ===================== Phase 1: Student =====================
            Add(new EventDefinition {
                Id = "broken_laptop",
                Title = "Broken Laptop",
                Description = "Your laptop dies before a deadline. Emergencies always pick the worst time.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-200), EffectStep.Happiness(-10) },
                MinPhase = 1, MaxPhase = 2,
                Weight = 1f
            });
            Add(new EventDefinition {
                Id = "roommate_found",
                Title = "New Roommate",
                Description = "A new roommate clicks. Splitting rent eases the squeeze.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Relationships(10), EffectStep.Happiness(5) },
                MinPhase = 1, MaxPhase = 2,
                Weight = 0.8f
            });
            Add(new EventDefinition {
                Id = "inflation_1",
                Title = "Prices Are Up",
                Description = "Groceries cost more this month. Small leaks sink budgets.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-120) },
                MinPhase = 1, MaxPhase = 2,
                Weight = 1.3f
            });
            Add(new EventDefinition {
                Id = "class_canceled",
                Title = "Class Canceled",
                Description = "Your core course is dropped. Course planning matters.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Career(-5), EffectStep.Happiness(5) },
                MinPhase = 1, MaxPhase = 1,
                Weight = 0.8f
            });
            Add(new EventDefinition {
                Id = "birthday_party",
                Title = "Surprise Birthday",
                Description = "Friends throw a surprise party. Joy now, paycheck later.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Happiness(15), EffectStep.Money(-80), EffectStep.Relationships(5) },
                MinPhase = 1, MaxPhase = 2,
                Weight = 0.9f
            });
            Add(new EventDefinition {
                Id = "free_time",
                Title = "Free Semester",
                Description = "An open block in your schedule. How do you fill it?",
                Kind = EventKind.Choice,
                ButtonOneText = "Join the board",
                ButtonTwoText = "Take an Excel course",
                ButtonOneEffects = new[] { EffectStep.AddCard("Board"), EffectStep.Relationships(5) },
                ButtonTwoEffects = new[] { EffectStep.AddCard("Course"), EffectStep.Career(3) },
                MinPhase = 1, MaxPhase = 1,
                Weight = 0.8f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "money_crunch",
                Title = "End of Month",
                Description = "Bills are due, wallet is empty. Who do you turn to?",
                Kind = EventKind.Choice,
                ButtonOneText = "Ask parents",
                ButtonTwoText = "Cut all extras",
                ButtonOneEffects = new[] { EffectStep.AddCard("Parents") },
                ButtonTwoEffects = new[] { EffectStep.AddCard("Refocus") },
                MinPhase = 1, MaxPhase = 1,
                Weight = 0.8f,
                OncePerRun = true,
                Condition = ctx => ctx.Player != null && ctx.Player.Money < 250
            });
            Add(new EventDefinition {
                Id = "study_path",
                Title = "Study Path",
                Description = "Your coach suggests a focus area before the next semester.",
                Kind = EventKind.Choice,
                ButtonOneText = "Entrepreneurship minor",
                ButtonTwoText = "Stay on the core track",
                ButtonOneEffects = new[] { EffectStep.AddCard("Minor") },
                ButtonTwoEffects = new[] { EffectStep.Career(8), EffectStep.Happiness(-3) },
                MinPhase = 1, MaxPhase = 1,
                Weight = 0.7f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "burnout_warning",
                Title = "Early Warning",
                Description = "Sleep, mood and grades are sliding. Burnout starts quiet.",
                Kind = EventKind.Choice,
                ButtonOneText = "Slow down",
                ButtonTwoText = "Push through",
                ButtonOneEffects = new[] { EffectStep.AddCard("Warning"), EffectStep.Happiness(3) },
                ButtonTwoEffects = new[] { EffectStep.Health(-10), EffectStep.Happiness(-8), EffectStep.Career(3) },
                MinPhase = 1, MaxPhase = 2,
                Weight = 0.6f,
                OncePerRun = true,
                Condition = ctx => ctx.Player != null && ctx.Player.Health < 70
            });

            // ===================== Phase 2: Starter career =====================
            Add(new EventDefinition {
                Id = "promotion",
                Title = "Small Promotion",
                Description = "Your manager bumps you up. With more pay comes more hours.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Career(15), EffectStep.Happiness(5), EffectStep.Relationships(-5) },
                MinPhase = 2, MaxPhase = 3,
                Weight = 0.8f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "work")
            });
            Add(new EventDefinition {
                Id = "job_offer",
                Title = "Job Offer",
                Description = "Another firm wants you. Loyalty or leverage?",
                Kind = EventKind.Choice,
                ButtonOneText = "Accept",
                ButtonTwoText = "Stay put",
                ButtonOneEffects = new[] {
                    EffectStep.AddCard(Names.WorkCardLow),
                    EffectStep.Career(10),
                    EffectStep.Happiness(5),
                    EffectStep.Relationships(-5)
                },
                ButtonTwoEffects = new[] { EffectStep.Happiness(3), EffectStep.Career(-3) },
                MinPhase = 2, MaxPhase = 3,
                Weight = 0.7f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "inflation_2",
                Title = "Cost of Living",
                Description = "Your monthly bills sneak up. Lifestyle creep is real.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-220) },
                MinPhase = 2, MaxPhase = 5,
                Weight = 1.2f
            });
            Add(new EventDefinition {
                Id = "apartment_fire",
                Title = "Apartment Fire",
                Description = "A stove sets the hallway alight. This is why insurance exists.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-500), EffectStep.Health(-10), EffectStep.Happiness(-10) },
                MinPhase = 2, MaxPhase = 3,
                Weight = 0.4f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "friends_wedding",
                Title = "Friend's Wedding",
                Description = "You celebrate hard. Your wallet does not.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Happiness(10), EffectStep.Money(-180), EffectStep.Relationships(5) },
                MinPhase = 2, MaxPhase = 3,
                Weight = 0.9f
            });
            Add(new EventDefinition {
                Id = "lifestyle_creep",
                Title = "Lifestyle Creep",
                Description = "New raise, new subscriptions. Income grew faster than discipline.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-250), EffectStep.Happiness(5) },
                MinPhase = 2, MaxPhase = 4,
                Weight = 0.8f,
                Condition = ctx => DeckHasTag(ctx, "work")
            });

            // ===================== Phase 3: Family =====================
            Add(new EventDefinition {
                Id = "pregnancy_news",
                Title = "Happy News",
                Description = "You're going to be parents. The deepest love and the deepest cost.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Happiness(10),
                    EffectStep.Relationships(15),
                    EffectStep.AddCard("Child")
                },
                MinPhase = 3, MaxPhase = 3,
                Weight = 0.7f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "school_costs_up",
                Title = "School Fees Up",
                Description = "School announces a fee bump. Family budgets rarely shrink.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-300), EffectStep.Happiness(-5) },
                MinPhase = 3, MaxPhase = 4,
                Weight = 0.9f,
                Condition = ctx => DeckHasTag(ctx, "family")
            });
            Add(new EventDefinition {
                Id = "family_illness",
                Title = "Family Illness",
                Description = "Someone close needs your help. Some bills are not financial.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Health(-15), EffectStep.Happiness(-10), EffectStep.Relationships(5) },
                MinPhase = 3, MaxPhase = 5,
                Weight = 0.7f
            });
            Add(new EventDefinition {
                Id = "promotion_mid",
                Title = "Senior Promotion",
                Description = "You step up. The hours follow.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Career(20), EffectStep.Happiness(-5), EffectStep.Relationships(-5) },
                MinPhase = 3, MaxPhase = 4,
                Weight = 0.7f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "work")
            });
            Add(new EventDefinition {
                Id = "market_crash",
                Title = "Market Crash",
                Description = "The market drops 40%. Time in market beats timing it.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Money(-250),
                    EffectStep.Happiness(-8),
                    EffectStep.AddCard("Crash")
                },
                MinPhase = 4, MaxPhase = 5,
                Weight = 0.5f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "investment")
            });
            Add(new EventDefinition {
                Id = "divorce_event",
                Title = "Divorce",
                Description = "The marriage ends. Half of everything is rarely a clean split.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Money(-200),
                    EffectStep.Relationships(-15),
                    EffectStep.Happiness(-10),
                    EffectStep.AddCard("Divorce")
                },
                MinPhase = 3, MaxPhase = 4,
                Weight = 0.4f,
                OncePerRun = true,
                Condition = ctx => ctx.Player != null && ctx.Player.Relationships < 30
            });
            Add(new EventDefinition {
                Id = "mass_layoff",
                Title = "Layoff Wave",
                Description = "A round of cuts sweeps your firm. Your role is gone.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.RemoveByTag("work"),
                    EffectStep.Happiness(-8),
                    EffectStep.AddCard("Layoffs")
                },
                MinPhase = 3, MaxPhase = 4,
                Weight = 0.4f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "work")
            });
            Add(new EventDefinition {
                Id = "burnout_full",
                Title = "Full Burnout",
                Description = "You completely crash. Sick leave, gone for months.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Health(-15),
                    EffectStep.Happiness(-10),
                    EffectStep.AddCard("Burnout")
                },
                MinPhase = 3, MaxPhase = 5,
                Weight = 0.4f,
                OncePerRun = true,
                Condition = ctx => ctx.Player != null && ctx.Player.Health < 50
            });
            Add(new EventDefinition {
                Id = "inheritance_mid",
                Title = "Family Inheritance",
                Description = "A relative passes away. Grief and money rarely arrive separately.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Happiness(-5),
                    EffectStep.Relationships(-3),
                    EffectStep.AddCard("Inherit")
                },
                MinPhase = 3, MaxPhase = 4,
                Weight = 0.4f,
                OncePerRun = true
            });

            // ===================== Phase 4: Midcareer =====================
            Add(new EventDefinition {
                Id = "burnout_choice",
                Title = "Burnout",
                Description = "Body and brain are calling time. Rest is not optional forever.",
                Kind = EventKind.Choice,
                ButtonOneText = "Take leave",
                ButtonTwoText = "Push through",
                ButtonOneEffects = new[] {
                    EffectStep.Health(5),
                    EffectStep.Happiness(5),
                    EffectStep.AddCard("Burnout")
                },
                ButtonTwoEffects = new[] { EffectStep.Career(5), EffectStep.Health(-20), EffectStep.Happiness(-12) },
                MinPhase = 4, MaxPhase = 5,
                Weight = 0.7f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "heart_attack",
                Title = "Health Scare",
                Description = "An ambulance ride you will not forget.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Health(-20),
                    EffectStep.Money(-300),
                    EffectStep.Happiness(-8),
                    EffectStep.AddCard("Checkup")
                },
                MinPhase = 4, MaxPhase = 5,
                Weight = 0.4f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "investment_payout",
                Title = "Investment Payout",
                Description = "A long-held position pays off. Patience compounds.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(450), EffectStep.Happiness(5) },
                MinPhase = 4, MaxPhase = 5,
                Weight = 0.6f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "investment")
            });
            Add(new EventDefinition {
                Id = "layoff",
                Title = "Restructuring",
                Description = "The company restructures. Your role does not survive.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.RemoveByTag("work"),
                    EffectStep.Career(-15),
                    EffectStep.Happiness(-10)
                },
                MinPhase = 4, MaxPhase = 5,
                Weight = 0.5f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "work")
            });
            Add(new EventDefinition {
                Id = "midlife_crisis",
                Title = "Midlife Crisis",
                Description = "The road less traveled looks great. Big spends rarely cure feelings.",
                Kind = EventKind.Choice,
                ButtonOneText = "Burn it down",
                ButtonTwoText = "Reconnect with family",
                ButtonOneEffects = new[] {
                    EffectStep.Happiness(5),
                    EffectStep.Relationships(-10),
                    EffectStep.AddCard("Crisis")
                },
                ButtonTwoEffects = new[] { EffectStep.Relationships(15), EffectStep.Happiness(5), EffectStep.Money(-80) },
                MinPhase = 4, MaxPhase = 4,
                Weight = 0.6f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "pension_gap",
                Title = "Pension Gap",
                Description = "Your projection comes back short. Compound interest needs early years.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Happiness(-5),
                    EffectStep.Career(3),
                    EffectStep.AddCard("Pensiongap")
                },
                MinPhase = 4, MaxPhase = 4,
                Weight = 0.5f,
                OncePerRun = true
            });

            // ===================== Phase 5: Retirement =====================
            Add(new EventDefinition {
                Id = "pension_setback",
                Title = "Pension Setback",
                Description = "A clerical error trims your monthly payout. Always double-check the statement.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Money(-200), EffectStep.Happiness(-5) },
                MinPhase = 5, MaxPhase = 5,
                Weight = 1f
            });
            Add(new EventDefinition {
                Id = "hospital_visit",
                Title = "Hospital Visit",
                Description = "Nothing serious. Still expensive.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Health(-8),
                    EffectStep.Happiness(-5),
                    EffectStep.AddCard("Healthbills")
                },
                MinPhase = 5, MaxPhase = 5,
                Weight = 1f
            });
            Add(new EventDefinition {
                Id = "inheritance",
                Title = "Inheritance",
                Description = "A distant relative leaves you a surprise. Legacy is the slowest gift.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Happiness(-3),
                    EffectStep.Relationships(-3),
                    EffectStep.AddCard("Inherit")
                },
                MinPhase = 5, MaxPhase = 5,
                Weight = 0.5f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "friend_passing",
                Title = "A Friend Passes",
                Description = "You light a candle and remember. Some losses do not have a price.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Happiness(-15), EffectStep.Health(-5), EffectStep.Relationships(-5) },
                MinPhase = 5, MaxPhase = 5,
                Weight = 0.6f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "grandchild_born",
                Title = "A Grandchild",
                Description = "A brand-new tiny human to love. Your time is the gift.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] { EffectStep.Relationships(15), EffectStep.Happiness(10) },
                MinPhase = 5, MaxPhase = 5,
                Weight = 0.6f,
                OncePerRun = true,
                Condition = ctx => DeckHasTag(ctx, "family")
            });
            Add(new EventDefinition {
                Id = "inflation_shock",
                Title = "Inflation Shock",
                Description = "Prices jump, pensions do not. Fixed income is a moving target.",
                Kind = EventKind.Event,
                OnContinueEffects = new[] {
                    EffectStep.Money(-200),
                    EffectStep.Happiness(-5),
                    EffectStep.AddCard("Inflation")
                },
                MinPhase = 5, MaxPhase = 5,
                Weight = 0.5f,
                OncePerRun = true
            });
            Add(new EventDefinition {
                Id = "scam_call",
                Title = "Scam Call",
                Description = "A polite voice asks for a bank code. Fraudsters target retirees first.",
                Kind = EventKind.Choice,
                ButtonOneText = "Trust them",
                ButtonTwoText = "Hang up",
                ButtonOneEffects = new[] {
                    EffectStep.Money(-250),
                    EffectStep.Happiness(-10),
                    EffectStep.AddCard("Scam")
                },
                ButtonTwoEffects = new[] { EffectStep.Happiness(3), EffectStep.Career(3) },
                MinPhase = 5, MaxPhase = 5,
                Weight = 0.5f,
                OncePerRun = true
            });
        }
    }
}
