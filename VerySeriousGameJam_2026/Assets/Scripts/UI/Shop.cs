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
    public Button sweepAttackButton;
    public TMP_Text sweepAttackButtonText;
    public string sweepPurchasedText = "Bought";

    [Header("Double Health Upgrade")]
    public SpinnerUpgradeManager playerUpgradeManager;
    public UpgradeData doubleHealthUpgrade;
    public Button doubleHealthButton;
    public TMP_Text doubleHealthButtonText;
    public string doubleHealthPurchasedText = "Bought";

    bool _doubleHealthBought;

    public void ContinueButton()
    {
        shopPanel.SetActive(false);

        Time.timeScale = 1f;

        waveManager.StartNextWave();
    }

    void Start()
    {
        ResolvePlayerCombat();
        ResolvePlayerUpgradeManager();
        shopPanel.SetActive(false);
        RefreshSweepAttackButton();
        RefreshDoubleHealthButton();
    }

    public void BuySweepAttackButton()
    {
        ResolvePlayerCombat();

        if (playerCombat == null || playerCombat.sweepUnlocked)
            return;

        playerCombat.UnlockSweepAttack();
        RefreshSweepAttackButton();
    }

    public void BuyDoubleHealthButton()
    {
        ResolvePlayerUpgradeManager();

        if (_doubleHealthBought || playerUpgradeManager == null || doubleHealthUpgrade == null)
            return;

        playerUpgradeManager.AddUpgrade(doubleHealthUpgrade);
        _doubleHealthBought = true;
        RefreshDoubleHealthButton();
    }

    void RefreshSweepAttackButton()
    {
        if (playerCombat == null)
            return;

        if (!playerCombat.sweepUnlocked)
            return;

        if (sweepAttackButton != null)
            sweepAttackButton.interactable = false;

        if (sweepAttackButtonText != null)
            sweepAttackButtonText.text = sweepPurchasedText;
    }

    void RefreshDoubleHealthButton()
    {
        if (!_doubleHealthBought)
            return;

        if (doubleHealthButton != null)
            doubleHealthButton.interactable = false;

        if (doubleHealthButtonText != null)
            doubleHealthButtonText.text = doubleHealthPurchasedText;
    }

    void ResolvePlayerCombat()
    {
        if (playerCombat != null)
            return;

        if (playerController != null)
            playerCombat = playerController.GetComponent<SpinnerCombat>();
    }

    void ResolvePlayerUpgradeManager()
    {
        if (playerUpgradeManager != null)
            return;

        if (playerController != null)
            playerUpgradeManager = playerController.GetComponent<SpinnerUpgradeManager>();
    }
}
