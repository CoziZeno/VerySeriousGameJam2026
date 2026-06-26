using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class GameProgressUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthPodsText;

    void OnEnable()
    {
        Subscribe();
        UpdateUI();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnStatsChanged += UpdateUI;
            return;
        }

        var mgr = FindObjectOfType<GameProgressManager>();
        if (mgr != null)
            mgr.OnStatsChanged += UpdateUI;
    }

    void Unsubscribe()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnStatsChanged -= UpdateUI;
            return;
        }

        var mgr = FindObjectOfType<GameProgressManager>();
        if (mgr != null)
            mgr.OnStatsChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        var mgr = GameProgressManager.Instance ?? FindObjectOfType<GameProgressManager>();
        if (mgr == null)
            return;

        if (coinsText != null)
            coinsText.text = mgr.coins.ToString();

        if (killsText != null)
            killsText.text = mgr.killCount.ToString();

        if (scoreText != null)
            scoreText.text = mgr.score.ToString();

        if (healthPodsText != null)
            healthPodsText.text = mgr.healthPods.ToString();
    }
}
