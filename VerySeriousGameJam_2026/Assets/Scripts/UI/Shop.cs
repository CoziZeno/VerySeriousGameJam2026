using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public WaveManager waveManager;
    public GameObject shopPanel;

    [Header("Sweep Attack")]
    public SpinnerController playerController;
    public SpinnerCombat playerCombat;
    public int sweepAttackCost = 10;
    public Button sweepAttackButton;
    public TMP_Text sweepAttackButtonText;
    public TMP_Text sweepAttackPriceText;
    public string sweepPurchasedText = "Bought";

    [Header("Health Pod")]
    public int healthPodCost = 5;
    public int healthPodsPerPurchase = 1;
    public Button healthPodButton;
    public TMP_Text healthPodPriceText;
    public TMP_Text healthPodCountText;

    public void ContinueButton()
    {
        shopPanel.SetActive(false);

        Time.timeScale = 1f;

        waveManager.StartNextWave();
    }

    void OnEnable()
    {
        SubscribeToProgress();
        RefreshShopButtons();
    }

    void OnDisable()
    {
        UnsubscribeFromProgress();
    }

    void Start()
    {
        ResolvePlayerCombat();
        shopPanel.SetActive(false);
        RefreshShopButtons();
    }

    public void BuySweepAttackButton()
    {
        ResolvePlayerCombat();

        if (playerCombat == null || playerCombat.sweepUnlocked)
            return;

        GameProgressManager progress = GetProgressManager();
        if (progress == null || !progress.TrySpendCoins(sweepAttackCost))
            return;

        playerCombat.UnlockSweepAttack();
        RefreshShopButtons();
    }

    public void BuyHealthPodButton()
    {
        GameProgressManager progress = GetProgressManager();
        if (progress == null || !progress.TrySpendCoins(healthPodCost))
            return;

        progress.AddHealthPods(Mathf.Max(1, healthPodsPerPurchase));
        RefreshShopButtons();
    }

    void RefreshShopButtons()
    {
        ResolvePlayerCombat();

        GameProgressManager progress = GetProgressManager();
        int coins = progress != null ? progress.coins : 0;

        if (sweepAttackPriceText != null)
            sweepAttackPriceText.text = sweepAttackCost.ToString();

        if (healthPodPriceText != null)
            healthPodPriceText.text = healthPodCost.ToString();

        if (healthPodCountText != null)
            healthPodCountText.text = progress != null ? progress.healthPods.ToString() : "0";

        bool sweepBought = playerCombat != null && playerCombat.sweepUnlocked;

        if (sweepAttackButton != null)
            sweepAttackButton.interactable = !sweepBought && coins >= sweepAttackCost;

        if (sweepAttackButtonText != null && sweepBought)
            sweepAttackButtonText.text = sweepPurchasedText;

        if (healthPodButton != null)
            healthPodButton.interactable = coins >= healthPodCost;
    }

    void ResolvePlayerCombat()
    {
        if (playerCombat != null)
            return;

        if (playerController != null)
            playerCombat = playerController.GetComponent<SpinnerCombat>();
    }

    void SubscribeToProgress()
    {
        GameProgressManager progress = GetProgressManager();
        if (progress != null)
            progress.OnStatsChanged += RefreshShopButtons;
    }

    void UnsubscribeFromProgress()
    {
        GameProgressManager progress = GetProgressManager();
        if (progress != null)
            progress.OnStatsChanged -= RefreshShopButtons;
    }

    GameProgressManager GetProgressManager()
    {
        return GameProgressManager.Instance ?? FindObjectOfType<GameProgressManager>();
    }
}
