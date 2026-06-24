using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public WaveManager waveManager;
    public GameObject shopPanel;

    public void ContinueButton()
    {
        shopPanel.SetActive(false);

        Time.timeScale = 1f;

        waveManager.StartNextWave();
    }

    void Start()
    {
        shopPanel.SetActive(false);
    }
}