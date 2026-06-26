using System;
using UnityEngine;

[DisallowMultipleComponent]
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("Current Stats")]
    public int score;
    public int coins;
    public int xp;
    public int healthPods;

    [Header("Kill Rewards")]
    public int scorePerKill = 100;
    public int xpPerKill = 10;
    public int coinDropsPerKill = 3;

    [Header("Coin Drops")]
    public CoinPickup coinPickupPrefab;
    public float coinScatterRadius = 1.1f;

    public event Action OnStatsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        score += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnStatsChanged?.Invoke();
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return true;

        if (coins < amount)
            return false;

        coins -= amount;
        OnStatsChanged?.Invoke();
        return true;
    }

    public void AddHealthPods(int amount)
    {
        if (amount <= 0) return;
        healthPods += amount;
        OnStatsChanged?.Invoke();
    }

    public bool TryUseHealthPod()
    {
        if (healthPods <= 0)
            return false;

        healthPods--;
        OnStatsChanged?.Invoke();
        return true;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        xp += amount;
        OnStatsChanged?.Invoke();
    }

    public int killCount;

    public void RegisterKill(bool playerOwnedKill, Vector3 deathPosition)
    {
        if (!playerOwnedKill)
            return;

        killCount++;
        AddScore(scorePerKill);
        AddXP(xpPerKill);
        OnStatsChanged?.Invoke();
        SpawnCoinDrops(deathPosition);
    }

    void SpawnCoinDrops(Vector3 deathPosition)
    {
        if (coinPickupPrefab == null)
            return;

        for (int i = 0; i < coinDropsPerKill; i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * coinScatterRadius;
            Vector3 pos = deathPosition + new Vector3(r.x, 0.2f, r.y);

            Instantiate(coinPickupPrefab, pos, Quaternion.identity);
        }
    }
}
