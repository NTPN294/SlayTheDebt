using UnityEngine;

using UnityEngine.Serialization;



/// <summary>

/// Data-driven shop listing. Create via Assets > Create > Slay The Debt > Shop Item.

/// </summary>

[CreateAssetMenu(fileName = "ShopItem", menuName = "Slay The Debt/Shop Item", order = 0)]

public class ShopItemData : ScriptableObject

{

    [Header("Display")]

    public string itemName = "Item";

    [TextArea(2, 4)]

    public string description;

    public Sprite icon;



    [Header("Economy")]

    [Min(0)] public int cost = 100;

    [Min(0)] public int energyCost = 1;



    [Header("Stat bonuses (multiples of 5 recommended)")]

    public int happinessBonus;

    [FormerlySerializedAs("hungerBonus")]

    public int healthBonus;

    public int careerBonus;

    public int relationshipBonus;



    public bool HasStatEffect =>

        happinessBonus != 0 || healthBonus != 0 || careerBonus != 0 || relationshipBonus != 0;



    public string GetEffectsSummary()

    {

        if (!HasStatEffect)

            return "No stat changes.";



        var parts = new System.Collections.Generic.List<string>();

        if (happinessBonus != 0) parts.Add(FormatStat("Happiness", happinessBonus));

        if (healthBonus != 0) parts.Add(FormatStat("Health", healthBonus));

        if (careerBonus != 0) parts.Add(FormatStat("Career", careerBonus));

        if (relationshipBonus != 0) parts.Add(FormatStat("Relationship", relationshipBonus));

        return string.Join("\n", parts);

    }



    static string FormatStat(string label, int value) =>

        value > 0 ? $"+{value} {label}" : $"{value} {label}";

}


