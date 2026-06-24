using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("References")]
    public EnemySpawner spawner;
    public GameObject shopPanel;
    public Image waveForegroundFill;
    public TMP_Text waveNumberText;

    [Header("Wave Info")]
    public int currentWave = 1;

    private int aliveEnemies;
    private int totalEnemiesThisWave;
    private bool waveActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartWave();
    }

    void StartWave()
    {
        waveActive = true;

        int enemyCount = 2 + currentWave;

        totalEnemiesThisWave = enemyCount;
        aliveEnemies = enemyCount;
        UpdateWaveUI();
        UpdateWaveNumberUI();

        Debug.Log($"Starting Wave {currentWave} | Enemies: {enemyCount}");

        spawner.SpawnWave(enemyCount);
    }

    public void EnemyKilled()
    {
        if (!waveActive)
            return;

        aliveEnemies--;
        aliveEnemies = Mathf.Max(0, aliveEnemies);
        UpdateWaveUI();

        if (aliveEnemies <= 0)
        {
            WaveComplete();
        }
    }

    void WaveComplete()
    {
        waveActive = false;
        aliveEnemies = 0;
        UpdateWaveUI();

        Debug.Log("Wave Complete!");

        StartCoroutine(OpenShopDelay());
    }

    IEnumerator OpenShopDelay()
    {
        yield return new WaitForSeconds(2f);

        OpenShop();
    }

    void OpenShop()
    {
        Debug.Log("Opening Shop");

        shopPanel.SetActive(true);

        Debug.Log(shopPanel.activeSelf);

        Time.timeScale = 0f;
    }

    public void StartNextWave()
    {
        shopPanel.SetActive(false);

        Time.timeScale = 1f;

        currentWave++;

        StartCoroutine(StartNextWaveDelay());
    }

    IEnumerator StartNextWaveDelay()
    {
        yield return new WaitForSeconds(1f);

        StartWave();
    }


    void Update()
    {
        if (!waveActive)
            return;

        aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        UpdateWaveUI();

        if (aliveEnemies == 0)
        {
            waveActive = false;
            StartCoroutine(OpenShopDelay());
        }
    }

    void UpdateWaveUI()
    {
        if (waveForegroundFill == null)
            return;

        if (totalEnemiesThisWave <= 0)
        {
            waveForegroundFill.fillAmount = 0f;
            return;
        }

        waveForegroundFill.fillAmount = Mathf.Clamp01((float)aliveEnemies / totalEnemiesThisWave);
    }

    void UpdateWaveNumberUI()
    {
        if (waveNumberText == null)
            return;

        waveNumberText.text = $"Wave {currentWave}";
    }
}
