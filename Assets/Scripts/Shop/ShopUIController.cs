using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Shop UI: standalone ShopScene scroll grid or optional modal overlay in gameplay scenes.
/// </summary>
public class ShopUIController : MonoBehaviour
{
    [Header("Open / Close")]
    public Button shopOpenButton;
    [FormerlySerializedAs("closeButton")]
    public Button backButton;
    public GameObject shopRoot;
    public CanvasGroup shopPanel;

    [Header("Scroll grid")]
    [Tooltip("ScrollView Content — GridLayoutGroup parent for spawned ShopItemUI cards.")]
    public Transform gridContainer;
    public ShopItemUI shopItemPrefab;

    [Header("Header")]
    public TMP_Text moneyText;
    public TMP_Text energyText;

    [Header("Gameplay pause")]
    [Tooltip("CanvasGroups that receive input during combat (hand, cards, turn buttons). Disabled while shop modal is open.")]
    public CanvasGroup[] gameplayInputGroups;

    [Header("Animation")]
    public float fadeDuration = 0.2f;
    public float panelScaleFrom = 0.92f;

    [Header("References")]
    public ShopManager shopManager;

    [Header("Startup")]
    [Tooltip("Automatically opens the shop when entering ShopScene.")]
    public bool autoOpenInShopScene = true;

    readonly List<ShopItemUI> _spawnedItems = new List<ShopItemUI>();
    Coroutine _animRoutine;
    Coroutine _scrollRoutine;
    bool _isOpen;
    bool _isStandaloneShopScene;

    void Awake()
    {
        _isStandaloneShopScene = SceneManager.GetActiveScene().name == "ShopScene";

        if (shopManager == null)
            shopManager = ShopManager.Instance ?? FindFirstObjectByType<ShopManager>();

        if (shopOpenButton != null)
            shopOpenButton.onClick.AddListener(OpenShop);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (_isStandaloneShopScene)
        {
            if (shopRoot != null)
                shopRoot.SetActive(true);
            if (shopPanel != null)
            {
                shopPanel.alpha = 1f;
                shopPanel.interactable = true;
                shopPanel.blocksRaycasts = true;
                shopPanel.transform.localScale = Vector3.one;
            }
        }
        else
        {
            if (shopRoot != null)
                shopRoot.SetActive(false);
            if (shopPanel != null)
            {
                shopPanel.alpha = 0f;
                shopPanel.interactable = false;
                shopPanel.blocksRaycasts = false;
            }
        }
    }

    void OnEnable()
    {
        if (shopManager != null)
            shopManager.OnEconomyChanged += OnEconomyChanged;
    }

    void OnDisable()
    {
        if (shopManager != null)
            shopManager.OnEconomyChanged -= OnEconomyChanged;
    }

    void Start()
    {
        if (!autoOpenInShopScene || !_isStandaloneShopScene)
            return;

        InitializeStandaloneShop();
    }

    void OnBackClicked()
    {
        if (_isStandaloneShopScene)
            ReturnToPhase1Scene();
        else
            CloseShop();
    }

    public void InitializeStandaloneShop()
    {
        _isOpen = true;
        if (shopRoot != null)
            shopRoot.SetActive(true);

        BuildItemList();
        RefreshMoneyDisplay();
        RefreshEnergyDisplay();
        if (_scrollRoutine != null)
            StopCoroutine(_scrollRoutine);
        _scrollRoutine = StartCoroutine(ResetScroll());
    }

    public void OpenShop()
    {
        if (_isOpen)
            return;

        _isOpen = true;

        if (shopRoot != null)
            shopRoot.SetActive(true);

        BuildItemList();
        RefreshMoneyDisplay();
        RefreshEnergyDisplay();
        if (_scrollRoutine != null)
            StopCoroutine(_scrollRoutine);
        _scrollRoutine = StartCoroutine(ResetScroll());
        SetGameplayInputBlocked(true);

        if (_animRoutine != null)
            StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimateOpen());
    }

    public void OpenShopScene()
    {
        PersistEnergyFromPhase1();
        SceneManager.LoadScene("ShopScene");
    }

    public static void PersistEnergyFromPhase1()
    {
        if (GameSession.Player == null)
            return;

        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController?.EnergyText == null)
            return;

        if (int.TryParse(playerController.EnergyText.text, out int energy))
            GameSession.Player.Energy = energy;
    }

    public void ReturnToPhase1Scene()
    {
        if (PhaseFlowManager.Instance != null)
        {
            PhaseFlowManager.Instance.NotifyShopComplete();
            return;
        }

        SceneManager.LoadScene("Phase1Scene");
    }

    public void CloseShop()
    {
        if (!_isOpen || _isStandaloneShopScene)
            return;

        if (_animRoutine != null)
            StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimateClose());
    }

    void BuildItemList()
    {
        if (shopManager == null || gridContainer == null || shopItemPrefab == null)
            return;

        foreach (var row in _spawnedItems)
        {
            if (row != null)
                Destroy(row.gameObject);
        }
        _spawnedItems.Clear();

        foreach (var item in shopManager.GetCatalog())
        {
            if (item == null) continue;

            var row = Instantiate(shopItemPrefab, gridContainer);
            row.Bind(item, shopManager);
            _spawnedItems.Add(row);
        }

        Canvas.ForceUpdateCanvases();
        if (gridContainer is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    void RebuildGridLayout()
    {
        if (gridContainer is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    IEnumerator ResetScroll()
    {
        yield return null;
        var scroll = GetComponentInChildren<ScrollRect>();
        if (scroll != null)
            scroll.verticalNormalizedPosition = 1f;
        _scrollRoutine = null;
    }

    void ClearItemList()
    {
        foreach (var row in _spawnedItems)
        {
            if (row != null)
                Destroy(row.gameObject);
        }
        _spawnedItems.Clear();
        RebuildGridLayout();
    }

    void OnEconomyChanged()
    {
        RefreshAllItems();
        RefreshMoneyDisplay();
        RefreshEnergyDisplay();
    }

    void RefreshAllItems()
    {
        foreach (var row in _spawnedItems)
        {
            if (row != null)
                row.RefreshAffordability();
        }
    }

    public void RefreshMoneyDisplay()
    {
        if (moneyText == null || shopManager == null)
            return;

        moneyText.text = $"Money: €{shopManager.GetPlayerMoney()}";
    }

    public void RefreshEnergyDisplay()
    {
        if (energyText == null || shopManager == null)
            return;

        energyText.text = $"Energy: {shopManager.GetPlayerEnergy()}";
    }

    void SetGameplayInputBlocked(bool blocked)
    {
        if (gameplayInputGroups == null)
            return;

        foreach (var group in gameplayInputGroups)
        {
            if (group == null) continue;
            group.interactable = !blocked;
            group.blocksRaycasts = !blocked;
        }
    }

    IEnumerator AnimateOpen()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            float eased = 1f - (1f - p) * (1f - p);

            if (shopPanel != null)
            {
                shopPanel.alpha = eased;
                shopPanel.blocksRaycasts = eased > 0.01f;
                shopPanel.interactable = eased > 0.5f;
                float scale = Mathf.Lerp(panelScaleFrom, 1f, eased);
                shopPanel.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (shopPanel != null)
        {
            shopPanel.alpha = 1f;
            shopPanel.blocksRaycasts = true;
            shopPanel.interactable = true;
            shopPanel.transform.localScale = Vector3.one;
        }

        _animRoutine = null;
    }

    IEnumerator AnimateClose()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            float eased = p * p;

            if (shopPanel != null)
            {
                shopPanel.alpha = 1f - eased;
                float scale = Mathf.Lerp(1f, panelScaleFrom, eased);
                shopPanel.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        _isOpen = false;
        SetGameplayInputBlocked(false);

        if (shopPanel != null)
        {
            shopPanel.alpha = 0f;
            shopPanel.interactable = false;
            shopPanel.blocksRaycasts = false;
        }

        ClearItemList();
        yield return null;
        RebuildGridLayout();
        yield return null;

        if (shopRoot != null)
            shopRoot.SetActive(false);

        _animRoutine = null;
    }
}
