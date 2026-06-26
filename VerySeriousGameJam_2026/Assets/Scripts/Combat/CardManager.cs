using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public SpinnerController player;
    public TMP_Text healthPodCountText;

    [Header("VFX")]
    public GameObject healVFX;

    [Header("Healing")]
    public int healAmount = 50;

    void OnEnable()
    {
        SubscribeToProgress();
        UpdateHealthPodCount();
    }

    void OnDisable()
    {
        UnsubscribeFromProgress();
    }

    public void HealCard()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing!");
            return;
        }

        GameProgressManager progress = GetProgressManager();
        if (progress == null || !progress.TryUseHealthPod())
            return;

        // Heal player
        player.Heal(healAmount);

        // Spawn heal VFX
        if (healVFX != null)
        {
            GameObject fx = Instantiate(
                healVFX,
                player.transform.position,
                Quaternion.identity
            );

            // Follow player while effect is active
            fx.transform.SetParent(player.transform);
        }

        UpdateHealthPodCount();
    }

    void SubscribeToProgress()
    {
        GameProgressManager progress = GetProgressManager();
        if (progress != null)
            progress.OnStatsChanged += UpdateHealthPodCount;
    }

    void UnsubscribeFromProgress()
    {
        GameProgressManager progress = GetProgressManager();
        if (progress != null)
            progress.OnStatsChanged -= UpdateHealthPodCount;
    }

    void UpdateHealthPodCount()
    {
        if (healthPodCountText == null)
            return;

        GameProgressManager progress = GetProgressManager();
        healthPodCountText.text = progress != null ? progress.healthPods.ToString() : "0";
    }

    GameProgressManager GetProgressManager()
    {
        return GameProgressManager.Instance ?? FindObjectOfType<GameProgressManager>();
    }
}
