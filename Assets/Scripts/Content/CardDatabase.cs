using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// Static catalog of every card in the game.
    ///
    /// Authoring rules (keep consistent when adding rows):
    ///  - Id (= the name printed on the card UI) must be ONE word.
    ///  - Description is two short lines: "[gameplay action]\n[financial lesson]".
    ///  - Value > 0 only for cards that actually earn / convert into money in
    ///    real life (income, owned assets, borrowed money, big gambles).
    ///    Lifestyle / family / care cards stay Value 0 and act through stat deltas.
    ///  - Every "strong" card has at least one downside stat. No pure upside.
    ///  - MinPhase / MaxPhase define the phase pool. Tags drive event filters
    ///    (e.g. "work" for Fired/Layoff, "investment" for crash/payout).
    /// </summary>
    public static class CardDatabase
    {
        static readonly Dictionary<string, CardDefinition> _byId =
            new Dictionary<string, CardDefinition>();
        static readonly List<CardDefinition> _all = new List<CardDefinition>();
        static bool _initialized;

        public static IReadOnlyList<CardDefinition> All
        {
            get { EnsureInitialized(); return _all; }
        }

        public static CardDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureInitialized();
            _byId.TryGetValue(id, out CardDefinition def);
            return def;
        }

        public static Card BuildCard(string id) => GetById(id)?.Build();

        public static List<CardDefinition> GetForPhase(int phase, CardType? type = null, string tag = null)
        {
            EnsureInitialized();
            var result = new List<CardDefinition>();
            foreach (var def in _all)
            {
                if (!def.IsInPhase(phase)) continue;
                if (type.HasValue && def.Type != type.Value) continue;
                if (!string.IsNullOrEmpty(tag) && !def.HasTag(tag)) continue;
                result.Add(def);
            }
            return result;
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
        }

        static void Add(CardDefinition def) => _all.Add(def);

        static void Seed()
        {
            // ============================================================
            // Cross-phase keepers
            // ============================================================
            Add(new CardDefinition {
                Id = Names.Bike,
                Description = "Cycle everywhere for free.\nLow upkeep beats car payments.",
                Type = CardType.Asset, Value = 120, EnergyCost = 0,
                Happiness = 4, Health = 8,
                MinPhase = 1, MaxPhase = 5,
                Tags = new[] { "asset", "transport" }
            });
            Add(new CardDefinition {
                Id = Names.Gift,
                Description = "An unexpected gift arrives.\nFree cash is rare — enjoy it.",
                Type = CardType.Consumable, Value = 200, EnergyCost = 1,
                Happiness = 8, Relationships = 5,
                MinPhase = 1, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = Names.Gamble,
                Description = "Spin the casino wheel.\nThe house wins on average.",
                Type = CardType.Chance, Value = 180, EnergyCost = 2,
                Happiness = 10, Health = -4, Relationships = -3,
                MinPhase = 1, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = Names.Food,
                Description = "Cook a proper meal at home.\nNutrition beats takeout, week after week.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 0,
                Happiness = 4, Health = 12,
                MinPhase = 1, MaxPhase = 5,
                Tags = new[] { "asset" }
            });
            Add(new CardDefinition {
                Id = Names.MotorCycle,
                Description = "Buy a sport bike.\nThrill on two wheels, costly on healthcare.",
                Type = CardType.Asset, Value = 700, EnergyCost = 1,
                Happiness = 12, Health = -6, Career = 3,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "asset", "transport", "luxury" }
            });
            Add(new CardDefinition {
                Id = Names.Car,
                Description = "Own a reliable car.\nFreedom on wheels, insurance forever.",
                Type = CardType.Asset, Value = 1100, EnergyCost = 1,
                Happiness = 8, Career = 8, Health = -2,
                MinPhase = 2, MaxPhase = 5,
                Tags = new[] { "asset", "transport" }
            });
            Add(new CardDefinition {
                Id = Names.House,
                Description = "Pay off your house in full.\nReal equity, slow to liquidate.",
                Type = CardType.Asset, Value = 2200, EnergyCost = 2,
                Happiness = 18, Relationships = 12,
                MinPhase = 3, MaxPhase = 5,
                Tags = new[] { "asset", "housing" }
            });

            // ============================================================
            // Phase 1 - Student (18-24)
            // Theme: instability, cheap survival, gamble, debt buildup.
            // ============================================================
            Add(new CardDefinition {
                Id = Names.WorkCardStudent,
                Description = "Bag groceries between classes.\nHours worked are hours not learned.",
                Type = CardType.Income, Value = 180, EnergyCost = 2,
                Happiness = -4, Career = 3, Relationships = -3,
                MinPhase = 1, MaxPhase = 2,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = Names.TutorJob,
                Description = "Tutor classmates for cash.\nTeaching is the fastest way to learn.",
                Type = CardType.Income, Value = 140, EnergyCost = 1,
                Happiness = -2, Career = 5,
                MinPhase = 1, MaxPhase = 2,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Sidejob",
                Description = "Stack shelves on bad shifts.\nIrregular hours, irregular pay.",
                Type = CardType.Chance, Value = 280, EnergyCost = 2,
                Happiness = -5, Health = -3, Career = 2,
                MinPhase = 1, MaxPhase = 2,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = Names.Internship,
                Description = "Cash one summer's stipend.\nPay is small — the CV impact lasts years.",
                Type = CardType.Consumable, Value = 100, EnergyCost = 1,
                Happiness = -5, Career = 12,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = Names.Scholarship,
                Description = "Cash a one-off merit grant.\nFree money — but only this year.",
                Type = CardType.Consumable, Value = 220, EnergyCost = 1,
                Happiness = 5, Career = 3,
                MinPhase = 1, MaxPhase = 1
            });
            Add(new CardDefinition {
                Id = Names.Loan,
                Description = "Borrow to plug the gap.\nEasy money now, real cost later.",
                Type = CardType.Asset, Value = 250, EnergyCost = 0,
                Happiness = -10,
                MinPhase = 1, MaxPhase = 5,
                Tags = new[] { "debt" }
            });
            Add(new CardDefinition {
                Id = "Maxloan",
                Description = "Max out the student loan.\nFreedom now, decades of interest later.",
                Type = CardType.Asset, Value = 450, EnergyCost = 0,
                Happiness = -18, Health = -3, Career = 2,
                MinPhase = 1, MaxPhase = 2,
                Tags = new[] { "debt" }
            });
            Add(new CardDefinition {
                Id = "Restraint",
                Description = "Refuse the extra credit line.\nDiscipline buys peace of mind.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = 5, Health = 3, Career = 5,
                MinPhase = 1, MaxPhase = 3
            });
            Add(new CardDefinition {
                Id = Names.SharedAppartment,
                Description = "Split rent with roommates.\nCheap living, no privacy.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 5, Relationships = 10,
                MinPhase = 1, MaxPhase = 2,
                Tags = new[] { "asset", "housing" }
            });
            Add(new CardDefinition {
                Id = Names.Studio,
                Description = "Rent your own tiny place.\nPrivacy is sweet, rent is loud.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 15, Career = -3, Relationships = -5,
                MinPhase = 1, MaxPhase = 3,
                Tags = new[] { "asset", "housing", "luxury" }
            });
            Add(new CardDefinition {
                Id = Names.Festival,
                Description = "Three days off the grid.\nJoy now, paycheck later.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = 22, Health = -10, Relationships = 6,
                MinPhase = 1, MaxPhase = 3
            });
            Add(new CardDefinition {
                Id = Names.LateNightStudy,
                Description = "Cram past midnight.\nGrade up, body down.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -5, Health = -10, Career = 12,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = Names.Textbooks,
                Description = "Read the required books.\nKnowledge sticks, paper is once.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 0,
                Happiness = -5, Career = 10,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = "Crypto",
                Description = "Bet savings on a coin.\nGains shout, losses whisper.",
                Type = CardType.Chance, Value = 380, EnergyCost = 2,
                Happiness = -5, Health = -2,
                MinPhase = 1, MaxPhase = 3,
                Tags = new[] { "investment" }
            });
            Add(new CardDefinition {
                Id = "Romance",
                Description = "Build a serious relationship.\nWarmth and weight in one package.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = 10, Relationships = 20, Career = -3,
                MinPhase = 1, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Board",
                Description = "Take a year on the student board.\nNetwork wide, graduation late.",
                Type = CardType.Asset, Value = 0, EnergyCost = 2,
                Happiness = -3, Career = 8, Relationships = 12,
                MinPhase = 1, MaxPhase = 1,
                Tags = new[] { "asset" }
            });
            Add(new CardDefinition {
                Id = "Refocus",
                Description = "Drop the extras, focus on grades.\nLess fun, sharper degree.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -3, Health = 5, Career = 10,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = "Warning",
                Description = "Heed the early burnout signs.\nSmall pauses now beat long breaks later.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = 5, Health = 10, Career = -5,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = "Course",
                Description = "Take an online Excel course.\nSmall skill jumps unlock bigger jobs.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -3, Career = 10,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = "Parents",
                Description = "Ask family for a hand.\nFree cash — but it costs your pride.",
                Type = CardType.Consumable, Value = 250, EnergyCost = 1,
                Happiness = -8, Relationships = 3,
                MinPhase = 1, MaxPhase = 2
            });
            Add(new CardDefinition {
                Id = "Minor",
                Description = "Pick an entrepreneurship minor.\nNo paycheck, sharper instincts.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = -3, Career = 12,
                MinPhase = 1, MaxPhase = 2
            });

            // ============================================================
            // Phase 2 - Starter career (24-32)
            // Theme: ambition, career growth, lifestyle inflation, first big bets.
            // ============================================================
            Add(new CardDefinition {
                Id = Names.WorkCardLow,
                Description = "Hold a steady office salary.\nRoutine pays, routine ages.",
                Type = CardType.Income, Value = 320, EnergyCost = 2,
                Happiness = -8, Career = 8, Relationships = -10,
                MinPhase = 2, MaxPhase = 3,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Overbid",
                Description = "Win a bidding war on a house.\nYours forever — debt forever.",
                Type = CardType.Asset, Value = 550, EnergyCost = 0,
                Happiness = -18, Health = -3, Relationships = -5,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "asset", "housing", "debt" }
            });
            Add(new CardDefinition {
                Id = "Renting",
                Description = "Stay flexible, keep renting.\nNo equity, no anchor either.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 5, Career = 5,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "asset", "housing" }
            });
            Add(new CardDefinition {
                Id = "Promotion",
                Description = "Pitch yourself for a raise.\nA brave ask, a real downside.",
                Type = CardType.Chance, Value = 500, EnergyCost = 2,
                Happiness = 3, Career = 10, Relationships = -5,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Sideproject",
                Description = "Build something on the side.\nSmall money in, big time out.",
                Type = CardType.Asset, Value = 150, EnergyCost = 2,
                Happiness = -5, Health = -3, Career = 8,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Indexfund",
                Description = "Park cash in broad market funds.\nSlow, steady, boring — and it works.",
                Type = CardType.Asset, Value = 260, EnergyCost = 1,
                Happiness = 3, Career = 2,
                MinPhase = 2, MaxPhase = 5,
                Tags = new[] { "asset", "investment" }
            });
            Add(new CardDefinition {
                Id = "Carloan",
                Description = "Drive new, pay monthly.\nThe car ages, the loan does not.",
                Type = CardType.Asset, Value = 280, EnergyCost = 0,
                Happiness = 12, Career = 3, Relationships = 5,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "asset", "transport", "debt" }
            });
            Add(new CardDefinition {
                Id = "Savings",
                Description = "Automate part of every paycheck.\nBoring habit, comfortable future.",
                Type = CardType.Income, Value = 180, EnergyCost = 1,
                Happiness = -2, Career = 2,
                MinPhase = 2, MaxPhase = 5,
                Tags = new[] { "asset" }
            });
            Add(new CardDefinition {
                Id = "Cohabit",
                Description = "Move in with your partner.\nShared rent, shared bathroom.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = 12, Relationships = 18, Career = -3,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "family", "housing" }
            });
            Add(new CardDefinition {
                Id = "Jobswitch",
                Description = "Quit and try somewhere new.\nGrowth or regret — a real coin flip.",
                Type = CardType.Chance, Value = 0, EnergyCost = 2,
                Happiness = 0, Career = 15, Relationships = -5,
                MinPhase = 2, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Refinance",
                Description = "Push extra cash at old debt.\nCompound interest cuts both ways.",
                Type = CardType.Consumable, Value = 500, EnergyCost = 2,
                Happiness = -8, Career = 3,
                MinPhase = 2, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Organize",
                Description = "Run a side festival.\nHuge effort, fragile payoff.",
                Type = CardType.Chance, Value = 450, EnergyCost = 3,
                Happiness = -10, Health = -5, Career = 8, Relationships = 5,
                MinPhase = 2, MaxPhase = 4
            });
            Add(new CardDefinition {
                Id = "Pensionplan",
                Description = "Open a pension account early.\nA small habit compounds for decades.",
                Type = CardType.Asset, Value = 150, EnergyCost = 1,
                Happiness = -2, Career = 3,
                MinPhase = 2, MaxPhase = 5,
                Tags = new[] { "asset", "investment", "retirement" }
            });
            Add(new CardDefinition {
                Id = "Lend",
                Description = "Loan cash to a close friend.\nMoney usually leaves; sometimes friendship too.",
                Type = CardType.Chance, Value = 0, EnergyCost = 1,
                Happiness = -5, Relationships = -8,
                MinPhase = 2, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Network",
                Description = "Polish your online profile.\nCheap effort, occasional payoff.",
                Type = CardType.Chance, Value = 0, EnergyCost = 1,
                Happiness = -2, Career = 8,
                MinPhase = 2, MaxPhase = 5,
                Tags = new[] { "work" }
            });

            // ============================================================
            // Phase 3 - Family (32-42)
            // Theme: high expenses, relationship stress, hard tradeoffs.
            // ============================================================
            Add(new CardDefinition {
                Id = "Child",
                Description = "Welcome a new family member.\nDeepest love, deepest expense.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 12, Health = -8, Relationships = 25, Career = -5,
                MinPhase = 3, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Reset",
                Description = "Your mortgage rate jumps.\nFixed periods always end.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -15,
                MinPhase = 3, MaxPhase = 5,
                Tags = new[] { "debt" }
            });
            Add(new CardDefinition {
                Id = "Parttime",
                Description = "Partner drops hours for family.\nLess income, more presence.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 5, Relationships = 15, Career = -5,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Renovation",
                Description = "Tear up the house, rebuild it.\nDust today, value tomorrow.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = 20, Health = -5, Relationships = 8,
                MinPhase = 3, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Benefits",
                Description = "Claim every child benefit.\nPaperwork now, breathing room later.",
                Type = CardType.Chance, Value = 220, EnergyCost = 1,
                Happiness = 2,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "School",
                Description = "Pay for private schooling.\nBetter classmates, smaller savings.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = 5, Relationships = 5, Career = 0,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Pivot",
                Description = "Switch industries mid-career.\nBig risk, bigger learning curve.",
                Type = CardType.Chance, Value = 500, EnergyCost = 3,
                Happiness = -5, Career = 15, Relationships = -8,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Policy",
                Description = "Audit your insurance plans.\nPay less for the right cover.",
                Type = CardType.Chance, Value = 180, EnergyCost = 1,
                Happiness = 3, Career = 2,
                MinPhase = 3, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Sibling",
                Description = "A second child joins.\nDouble the joy, double the chaos.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 10, Health = -8, Relationships = 20, Career = -8,
                MinPhase = 3, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Sellhouse",
                Description = "Cash out the family home.\nBig payday, big upheaval.",
                Type = CardType.Consumable, Value = 3800, EnergyCost = 3,
                Happiness = -10, Relationships = -10,
                MinPhase = 3, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Emergency",
                Description = "Tap the emergency fund.\nIt's there for a reason — and only once.",
                Type = CardType.Consumable, Value = 500, EnergyCost = 1,
                Happiness = -5, Health = 3,
                MinPhase = 3, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Mortgage",
                Description = "Borrow against your house.\nLow rate, long leash.",
                Type = CardType.Asset, Value = 700, EnergyCost = 1,
                Happiness = -10, Relationships = 6,
                MinPhase = 3, MaxPhase = 5,
                Tags = new[] { "asset", "housing", "debt" }
            });
            Add(new CardDefinition {
                Id = "Childcare",
                Description = "Hire daycare so you can work.\nBuy your career hours back.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -5, Career = 10, Relationships = -3,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Divorce",
                Description = "End the marriage, split the assets.\nHalf the wealth, twice the freedom.",
                Type = CardType.Consumable, Value = 700, EnergyCost = 3,
                Happiness = -20, Health = -5, Relationships = -25,
                MinPhase = 3, MaxPhase = 4
            });
            Add(new CardDefinition {
                Id = "Layoffs",
                Description = "Take the severance package.\nA forced fresh start.",
                Type = CardType.Consumable, Value = 500, EnergyCost = 1,
                Happiness = -10, Career = -10,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Inherit",
                Description = "Receive an unexpected legacy.\nGrief and money rarely arrive separately.",
                Type = CardType.Consumable, Value = 700, EnergyCost = 1,
                Happiness = -5, Relationships = -3,
                MinPhase = 3, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Burnout",
                Description = "Take a long sick leave.\nIgnoring warnings always costs more.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = -10, Health = 5, Career = -15, Relationships = 5,
                MinPhase = 3, MaxPhase = 5
            });

            // ============================================================
            // Phase 4 - Midcareer (42-58)
            // Theme: peak earning, burnout, expensive coping, investment risk.
            // ============================================================
            Add(new CardDefinition {
                Id = Names.WorkCardMid,
                Description = "Senior salary at your firm.\nGood income, longer leashes.",
                Type = CardType.Income, Value = 450, EnergyCost = 2,
                Happiness = -12, Health = -3, Career = 10, Relationships = -12,
                MinPhase = 3, MaxPhase = 4,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Manager",
                Description = "Step up into management.\nMore power, more politics.",
                Type = CardType.Chance, Value = 800, EnergyCost = 2,
                Happiness = -8, Health = -5, Career = 15, Relationships = -8,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Tax",
                Description = "Hunt for legal deductions.\nA weekend of reading, a year of savings.",
                Type = CardType.Chance, Value = 400, EnergyCost = 1,
                Happiness = -3, Career = 3,
                MinPhase = 4, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Business",
                Description = "Launch your own company.\nUnlimited upside, unlimited stress.",
                Type = CardType.Asset, Value = 500, EnergyCost = 3,
                Happiness = -10, Health = -8, Career = 12, Relationships = -10,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "work", "investment" }
            });
            Add(new CardDefinition {
                Id = "Sabbatical",
                Description = "Take a month off to reset.\nLost income beats lost sanity.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = 15, Health = 15, Career = -10,
                MinPhase = 4, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "MBA",
                Description = "Pay for a business degree.\nPaper today, paychecks tomorrow.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 3,
                Happiness = -5, Career = 18, Relationships = -5,
                MinPhase = 4, MaxPhase = 4
            });
            Add(new CardDefinition {
                Id = "Property",
                Description = "Buy a place to rent out.\nPassive income — until tenants call.",
                Type = CardType.Chance, Value = 550, EnergyCost = 2,
                Happiness = -3, Career = 5,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "asset", "investment", "housing" }
            });
            Add(new CardDefinition {
                Id = "Quit",
                Description = "Walk out without a plan.\nFreedom first, panic later.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = 12, Health = 8, Career = -20,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Headhunter",
                Description = "Field a recruiter's offer.\nLeverage when you don't need it.",
                Type = CardType.Chance, Value = 700, EnergyCost = 1,
                Happiness = 3, Career = 8,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Solar",
                Description = "Put panels on the roof.\nUpfront sting, decades of free sun.",
                Type = CardType.Asset, Value = 140, EnergyCost = 1,
                Happiness = 3, Health = 3,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "asset" }
            });
            Add(new CardDefinition {
                Id = "Eldercare",
                Description = "Care for ageing parents.\nLove given, hours lost.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = -3, Health = -5, Relationships = 12, Career = -8,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "IPO",
                Description = "Stake on a hot tech listing.\nFortunes made and burnt overnight.",
                Type = CardType.Chance, Value = 1200, EnergyCost = 3,
                Happiness = -5, Health = -3,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "investment" }
            });
            Add(new CardDefinition {
                Id = "Lifecover",
                Description = "Insure your earning power.\nCover costs; absence costs more.",
                Type = CardType.Asset, Value = 0, EnergyCost = 0,
                Happiness = 5, Relationships = 8,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Crash",
                Description = "Buy quality at firesale prices.\nFear is the steepest discount.",
                Type = CardType.Chance, Value = 800, EnergyCost = 3,
                Happiness = -10, Career = 3,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "investment" }
            });
            Add(new CardDefinition {
                Id = "Pensiongap",
                Description = "Confront the pension shortfall.\nDecades of small saves beat panic.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = -8, Career = 8,
                MinPhase = 4, MaxPhase = 4
            });
            Add(new CardDefinition {
                Id = "Crisis",
                Description = "Midlife divorce, midlife reset.\nFreedom is loud — and expensive.",
                Type = CardType.Consumable, Value = 900, EnergyCost = 3,
                Happiness = -20, Health = -8, Relationships = -25,
                MinPhase = 4, MaxPhase = 4
            });

            // ============================================================
            // Phase 5 - Retirement (58-70+)
            // Theme: fixed income, health costs, preserving wealth, legacy.
            // ============================================================
            Add(new CardDefinition {
                Id = Names.WorkCardHigh,
                Description = "Hold a top leadership role.\nTop pay, top stress.",
                Type = CardType.Income, Value = 700, EnergyCost = 3,
                Happiness = -15, Health = -8, Career = 15, Relationships = -20,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "work" }
            });
            Add(new CardDefinition {
                Id = "Pension",
                Description = "Collect your monthly pension.\nFixed income beats no income.",
                Type = CardType.Income, Value = 350, EnergyCost = 1,
                Happiness = 5,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "work", "retirement" }
            });
            Add(new CardDefinition {
                Id = "Heirloom",
                Description = "Set money aside for the kids.\nLegacy is the slowest gift.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = 8, Relationships = 15,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Travel",
                Description = "Spend on the bucket list.\nMemories cost; regret too.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = 28, Health = -5, Relationships = 10,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Rebalance",
                Description = "Rotate stocks into safer assets.\nLess upside, fewer sleepless nights.",
                Type = CardType.Chance, Value = 450, EnergyCost = 2,
                Happiness = 3,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "investment" }
            });
            Add(new CardDefinition {
                Id = "Downsize",
                Description = "Sell the family home, rent small.\nLess space, more freedom.",
                Type = CardType.Consumable, Value = 4500, EnergyCost = 2,
                Happiness = 5, Relationships = -10,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "housing" }
            });
            Add(new CardDefinition {
                Id = "Carepartner",
                Description = "Care for an ill partner.\nLove keeps showing up.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = -8, Health = -8, Relationships = 15,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Advisor",
                Description = "Hire a pension advisor.\nFees today buy clarity tomorrow.",
                Type = CardType.Chance, Value = 250, EnergyCost = 1,
                Happiness = 4,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Volunteer",
                Description = "Give time to a local cause.\nNo paycheck, lots of meaning.",
                Type = CardType.Asset, Value = 0, EnergyCost = 1,
                Happiness = 15, Health = 5, Career = 3, Relationships = 10,
                MinPhase = 4, MaxPhase = 5,
                Tags = new[] { "asset" }
            });
            Add(new CardDefinition {
                Id = "Savingsfund",
                Description = "Open a fund for the grandkids.\nSmall steady wins.",
                Type = CardType.Chance, Value = 220, EnergyCost = 1,
                Happiness = 8, Relationships = 10,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "family" }
            });
            Add(new CardDefinition {
                Id = "Reverse",
                Description = "Borrow against your own house.\nCash now, less to leave behind.",
                Type = CardType.Asset, Value = 600, EnergyCost = 1,
                Happiness = -5, Relationships = -8,
                MinPhase = 5, MaxPhase = 5,
                Tags = new[] { "housing", "debt" }
            });
            Add(new CardDefinition {
                Id = "Will",
                Description = "Write down clear wishes.\nClarity now, fewer fights later.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = 5, Relationships = 10,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Legacy",
                Description = "Roll the dice on a final big bet.\nWin big, or learn humility.",
                Type = CardType.Chance, Value = 2200, EnergyCost = 3,
                Happiness = 8,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Healthbills",
                Description = "Pay the mounting medical bills.\nRetirement is when cover gaps bite.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 2,
                Happiness = -10, Health = -8,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Inflation",
                Description = "Tighten the retirement budget.\nFixed income is a moving target.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -10, Career = 3,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Scam",
                Description = "Realize you were defrauded.\nFraudsters target trust, not greed.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -15, Health = -3,
                MinPhase = 5, MaxPhase = 5
            });
            Add(new CardDefinition {
                Id = "Checkup",
                Description = "Get a thorough medical exam.\nEarly news beats no news.",
                Type = CardType.Consumable, Value = 0, EnergyCost = 1,
                Happiness = -5, Health = 12,
                MinPhase = 5, MaxPhase = 5
            });
        }
    }
}
