using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("References")]
    public EnemySpawner spawner;
    public GameObject shopPanel;

    [Header("Wave Info")]
    public int currentWave = 1;

    private int aliveEnemies;
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

        aliveEnemies = enemyCount;

        Debug.Log($"Starting Wave {currentWave} | Enemies: {enemyCount}");

        spawner.SpawnWave(enemyCount);
    }

    public void EnemyKilled()
    {
        if (!waveActive)
            return;

        aliveEnemies--;

        if (aliveEnemies <= 0)
        {
            WaveComplete();
        }
    }

    void WaveComplete()
    {
        waveActive = false;

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

        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            waveActive = false;
            StartCoroutine(OpenShopDelay());
        }
    }
}