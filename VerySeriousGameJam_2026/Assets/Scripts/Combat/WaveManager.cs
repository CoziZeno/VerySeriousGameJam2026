using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    private readonly HashSet<SpinnerController> _activeWaveEnemies = new HashSet<SpinnerController>();

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

        ClearWaveEnemySubscriptions();
        RegisterWaveEnemies(spawner.SpawnWave(enemyCount));

        UpdateWaveUI();
        UpdateWaveNumberUI();

        Debug.Log($"Starting Wave {currentWave} | Enemies: {enemyCount}");
    }

    public void EnemyKilled()
    {
        if (!waveActive)
            return;

        ForceWaveProgressRefresh();
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

    void RegisterWaveEnemies(List<SpinnerController> enemies)
    {
        _activeWaveEnemies.Clear();

        if (enemies == null)
        {
            totalEnemiesThisWave = 0;
            aliveEnemies = 0;
            return;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            SpinnerController enemy = enemies[i];
            if (enemy == null)
                continue;

            if (_activeWaveEnemies.Add(enemy))
                enemy.OnEliminated += HandleWaveEnemyEliminated;
        }

        totalEnemiesThisWave = _activeWaveEnemies.Count;
        aliveEnemies = totalEnemiesThisWave;
    }

    void HandleWaveEnemyEliminated(SpinnerController enemy)
    {
        if (!_activeWaveEnemies.Remove(enemy))
            return;

        enemy.OnEliminated -= HandleWaveEnemyEliminated;
        aliveEnemies = _activeWaveEnemies.Count;
        UpdateWaveUI();

        if (waveActive && aliveEnemies <= 0)
            WaveComplete();
    }

    void ForceWaveProgressRefresh()
    {
        _activeWaveEnemies.RemoveWhere(enemy => enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy);
        aliveEnemies = _activeWaveEnemies.Count;
        UpdateWaveUI();

        if (waveActive && aliveEnemies <= 0)
            WaveComplete();
    }

    void ClearWaveEnemySubscriptions()
    {
        foreach (SpinnerController enemy in _activeWaveEnemies)
        {
            if (enemy != null)
                enemy.OnEliminated -= HandleWaveEnemyEliminated;
        }

        _activeWaveEnemies.Clear();
        aliveEnemies = 0;
        totalEnemiesThisWave = 0;
    }
}
