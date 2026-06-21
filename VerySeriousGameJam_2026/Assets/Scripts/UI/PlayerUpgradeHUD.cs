using UnityEngine;

public class PlayerUpgradeHUD : MonoBehaviour
{
    public SpinnerUpgradeManager playerUpgradeManager;
    public UpgradeBarUI upgradeBar;

    void Start()
    {
        playerUpgradeManager.OnUpgradeAdded += HandleUpgradeAdded;
    }

    void OnDestroy()
    {
        if (playerUpgradeManager != null)
            playerUpgradeManager.OnUpgradeAdded -= HandleUpgradeAdded;
    }

    void HandleUpgradeAdded(UpgradeData data)
    {
        upgradeBar.AddUpgrade(data);
    }
}