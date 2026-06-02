namespace Assets.Scripts
{
    /// <summary>
    /// Stable C# handles for content ids. The string VALUES are also the names
    /// rendered on the cards/events, so they are kept to ONE word each.
    ///
    /// To add new content prefer editing CardDatabase / EventDatabase directly;
    /// only add a constant here when C# code outside the databases needs to
    /// reference the id (e.g. starter decks in StartScript / PlayerController).
    /// </summary>
    internal class Names
    {
        // ---- Work / income (legacy slots renamed to one word) ----
        public const string WorkCardStudent = "Student";
        public const string WorkCardLow     = "Job";
        public const string WorkCardMid     = "Salary";
        public const string WorkCardHigh    = "Director";

        // ---- Universal cards ----
        public const string Bike       = "Bike";
        public const string Gift       = "Gift";
        public const string Gamble     = "Gamble";
        public const string MotorCycle = "Motorcycle";
        public const string Car        = "Car";
        public const string House      = "House";
        public const string Food       = "Food";
        public const string Loan       = "Loan";

        // ---- Housing ----
        public const string Studio           = "Studio";
        public const string SharedAppartment = "Roomshare";

        // ---- Phase 1 starter pool (referenced by StartScript) ----
        public const string TutorJob       = "Tutor";
        public const string Internship     = "Internship";
        public const string Scholarship    = "Scholarship";
        public const string Festival       = "Festival";
        public const string LateNightStudy = "Allnighter";
        public const string Textbooks      = "Textbooks";

        // ---- Enemy name (purely cosmetic) ----
        public const string PayRent = "Rent";

        // ---- Event ids ----
        public const string Fired    = "Fired";
        public const string BreakUp  = "Breakup";
        public const string FindWork = "Workoffer";
    }
}
