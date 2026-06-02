using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds one ShopItemData card in the shop grid.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text energyCostText;
    public TMP_Text descriptionText;
    public TMP_Text effectsText;
    public Image iconImage;
    public Image cardBackground;
    public Button buyButton;
    public TMP_Text buyButtonLabel;
    public TMP_Text feedbackText;

    [Header("Affordability")]
    public Color affordableColor = new Color(0.2f, 0.55f, 0.35f, 1f);
    public Color unaffordableColor = new Color(0.45f, 0.2f, 0.2f, 1f);

    ShopItemData _item;
    ShopManager _shop;

    public void Bind(ShopItemData item, ShopManager shop)
    {
        _item = item;
        _shop = shop;

        if (nameText != null) nameText.text = item.itemName;
        if (costText != null) costText.text = $"€{item.cost}";
        if (energyCostText != null) energyCostText.text = item.energyCost.ToString();
        if (descriptionText != null) descriptionText.text = item.description;
        if (effectsText != null) effectsText.text = item.GetEffectsSummary();

        if (iconImage != null)
        {
            iconImage.enabled = item.icon != null;
            if (item.icon != null)
                iconImage.sprite = item.icon;
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        RefreshAffordability();
    }

    public void RefreshAffordability()
    {
        if (_item == null || _shop == null)
            return;

        bool canAfford = _shop.CanAfford(_item);

        if (buyButton != null)
            buyButton.interactable = canAfford;

        if (buyButtonLabel != null)
            buyButtonLabel.text = canAfford ? "Buy" : "Can't afford";

        if (buyButton != null)
        {
            var colors = buyButton.colors;
            colors.normalColor = canAfford ? affordableColor : unaffordableColor;
            colors.disabledColor = unaffordableColor;
            buyButton.colors = colors;
        }

        if (feedbackText != null)
        {
            if (canAfford)
                feedbackText.text = "";
            else if (_shop.GetPlayerMoney() < _item.cost)
                feedbackText.text = $"Need €{_item.cost}";
            else
                feedbackText.text = $"Need {_item.energyCost} energy";
        }
    }

    void OnBuyClicked()
    {
        if (_shop == null || _item == null)
            return;

        if (_shop.TryPurchase(_item, out string reason))
        {
            if (feedbackText != null)
                feedbackText.text = "Purchased!";
        }
        else if (feedbackText != null)
            feedbackText.text = reason ?? "Purchase failed.";
    }
}
