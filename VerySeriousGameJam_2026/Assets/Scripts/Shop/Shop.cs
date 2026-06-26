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

    private bool playerInside;

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
        shopUI.SetActive(false);
        interactMessage.SetActive(false);
        RefreshShopButtons();
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
        RefreshShopButtons();
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
