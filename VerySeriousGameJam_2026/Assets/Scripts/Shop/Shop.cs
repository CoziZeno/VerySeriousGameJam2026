using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    [Header("References")]
    public GameObject shopUI;
    public GameObject interactMessage; // "Press E"
    public SpinnerController playerController;

    [Header("Sweep Attack")]
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

    private bool playerInside;
    bool _doubleHealthBought;

    void Start()
    {
        ResolvePlayerCombat();
        ResolvePlayerUpgradeManager();
        shopUI.SetActive(false);
        interactMessage.SetActive(false);
        RefreshSweepAttackButton();
        RefreshDoubleHealthButton();
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            interactMessage.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            interactMessage.SetActive(false);
        }
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        interactMessage.SetActive(false);

        playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);

        playerController.enabled = true;


        Time.timeScale = 1f;

        // If player is still inside the shop area, show the prompt again.
        if (playerInside)
            interactMessage.SetActive(true);
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

        if (HasDoubleHealthUpgrade() || playerUpgradeManager == null || doubleHealthUpgrade == null)
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
        if (!HasDoubleHealthUpgrade())
            return;

        _doubleHealthBought = true;

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

    bool HasDoubleHealthUpgrade()
    {
        if (_doubleHealthBought)
            return true;

        return playerUpgradeManager != null &&
            playerUpgradeManager.HasModule<DoubleHealthUpgradeModule>();
    }
}
