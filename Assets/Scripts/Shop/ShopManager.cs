using System;
using UnityEngine;

/// <summary>
/// Purchase logic: money checks, deductions, and stat application via StatusController.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Catalog")]
    [Tooltip("Assign ScriptableObject shop items, or leave empty to load from Resources/Shop.")]
    public ShopItemData[] catalog;

    [Header("References")]
    public StatusController statusController;

    public event Action OnEconomyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResolveSceneReferences();

        EnsureCatalog();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void EnsureCatalog()
    {
        if (catalog != null && catalog.Length > 0)
            return;

        var loaded = Resources.LoadAll<ShopItemData>("Shop");
        if (loaded != null && loaded.Length > 0)
            catalog = loaded;
    }

    void ResolveSceneReferences()
    {
        if (statusController == null)
            statusController = FindFirstObjectByType<StatusController>();
    }

    public ShopItemData[] GetCatalog() => catalog ?? Array.Empty<ShopItemData>();

    public int GetPlayerMoney()
    {
        if (GameSession.Player == null)
            return 0;
        return GameSession.Player.Money;
    }

    public int GetPlayerEnergy()
    {
        if (GameSession.Player == null)
            return 0;
        return GameSession.Player.Energy;
    }

    public bool CanAfford(ShopItemData item)
    {
        if (item == null) return false;
        return GetPlayerMoney() >= item.cost && GetPlayerEnergy() >= item.energyCost;
    }

    public bool TryPurchase(ShopItemData item, out string failureReason)
    {
        failureReason = null;

        if (item == null)
        {
            failureReason = "Invalid item.";
            return false;
        }

        if (GameSession.Player == null)
        {
            failureReason = "No active player session.";
            return false;
        }

        if (GetPlayerMoney() < item.cost)
        {
            failureReason = $"Not enough money (need €{item.cost}).";
            return false;
        }

        if (GetPlayerEnergy() < item.energyCost)
        {
            failureReason = $"Not enough energy (need {item.energyCost}).";
            return false;
        }

        GameSession.Player.Money -= item.cost;
        GameSession.Player.Energy -= item.energyCost;
        GameSession.RecordShopPurchase(item.cost);

        ResolveSceneReferences();
        ApplyShopBonuses(item);

        OnEconomyChanged?.Invoke();
        return true;
    }

    void ApplyShopBonuses(ShopItemData item)
    {
        var player = GameSession.Player;

        player.Happiness = Mathf.Clamp(player.Happiness + item.happinessBonus, 0, 100);
        player.Health = Mathf.Clamp(player.Health + item.healthBonus, 0, 100);
        player.Career = Mathf.Clamp(player.Career + item.careerBonus, 0, 100);
        player.Relationships = Mathf.Clamp(player.Relationships + item.relationshipBonus, 0, 100);

        if (statusController != null)
            statusController.LoadFromGameSession();
    }
}
