using Assets.Scripts;

[System.Serializable]
public class Card
{
    public string Name { get; private set; }
    public CardType Type { get; private set; }
    public int Value { get; private set;  }
    public int EnergyCost { get; private set;  }
    public string Description { get; }

    // Stat-wijzigingen toegepast zodra de kaart gespeeld wordt
    public int HappinessChange { get; }
    public int HealthChange { get; }
    public int CareerChange { get; }
    public int RelationshipChange { get; }

    // Oorspronkelijke constructor — achterwaarts compatibel (stats = 0)
    //public Card(string name, CardType type, int value, int energyCost, string description)
    //    : this(name, type, value, energyCost, description, 0, 0, 0, 0) { }

    // Constructor met stat-wijzigingen
    public Card(string name, CardType type, int value, int energyCost,
                string description, int happinessChange, int healthChange, int careerChange, int relationshipChange)
    {
        Name = name;
        Type = type;
        Value = value;
        EnergyCost = energyCost;
        Description = description;
        HappinessChange = happinessChange;
        HealthChange = healthChange;
        CareerChange = careerChange;
        RelationshipChange = relationshipChange;
    }


    /// <summary>
    /// Set the card variables
    /// </summary>
    /// <param name="name">The name of the card</param>
    /// <param name="type">The type of the card</param>
    /// <param name="value">The value of the card</param>
    /// <param name="energyCost">The energy cost of the card</param>
    public void SetCardVariables(string name, CardType type, int value, int energyCost)
    {
        Name = name;
        Type = type;
        Value = value;
        EnergyCost = energyCost;
    }

    /// <summary>
    /// Determines whether the specified card details match the current card instance.
    /// </summary>
    /// <param name="name">The name of the card to compare.</param>
    /// <param name="type">The type of the card to compare.</param>
    /// <param name="value">The value of the card to compare.</param>
    /// <returns>true if the specified name, type, and value match those of the current card; otherwise, false.</returns>
    public bool IsSameCard(string name, CardType type, int value)
    {
        if (Name.Equals(name) && Type == type && Value == value)
            return true;
        else
            return false;
    }
}