using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn Spacing")]
    public float spawnCheckRadius = 1.5f;
    public float spawnScatterRadius = 3f;
    public int spawnAttemptsPerEnemy = 12;

    public List<SpinnerController> SpawnWave(int enemyCount)
    {
        List<SpinnerController> spawnedEnemies = new List<SpinnerController>(enemyCount);

        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner is missing an enemy prefab or spawn points.");
            return spawnedEnemies;
        }

        List<Vector3> plannedPositions = new List<Vector3>(enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(plannedPositions);
            plannedPositions.Add(spawnPosition);

            GameObject enemyInstance = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

            SpinnerController spinner = enemyInstance.GetComponent<SpinnerController>();
            if (spinner != null)
                spawnedEnemies.Add(spinner);
        }

        return spawnedEnemies;
    }

    Vector3 GetSpawnPosition(List<Vector3> plannedPositions)
    {
        for (int i = 0; i < spawnAttemptsPerEnemy; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector2 scatter = Random.insideUnitCircle * spawnScatterRadius;
            Vector3 candidate = spawnPoint.position + new Vector3(scatter.x, 0f, scatter.y);

            if (!HasEnemyNear(candidate) && !HasPlannedSpawnNear(candidate, plannedPositions))
                return candidate;
        }

        Transform leastCrowded = GetLeastCrowdedSpawnPoint();
        return GetFallbackPosition(leastCrowded.position, plannedPositions);
    }

    bool HasEnemyNear(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, spawnCheckRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            SpinnerController spinner = hits[i].GetComponentInParent<SpinnerController>();
            if (spinner != null && spinner.isEnemy && spinner.IsAlive)
                return true;
        }

        return false;
    }

    bool HasPlannedSpawnNear(Vector3 position, List<Vector3> plannedPositions)
    {
        if (plannedPositions == null)
            return false;

        float minDistanceSqr = spawnCheckRadius * spawnCheckRadius;

        for (int i = 0; i < plannedPositions.Count; i++)
        {
            Vector3 toPlanned = position - plannedPositions[i];
            toPlanned.y = 0f;

            if (toPlanned.sqrMagnitude < minDistanceSqr)
                return true;
        }

        return false;
    }

    Transform GetLeastCrowdedSpawnPoint()
    {
        Transform best = spawnPoints[0];
        int bestCount = int.MaxValue;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int count = CountNearbyEnemies(spawnPoints[i].position);
            if (count < bestCount)
            {
                bestCount = count;
                best = spawnPoints[i];
            }
        }

        return best;
    }

    int CountNearbyEnemies(Vector3 position)
    {
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(position, spawnScatterRadius + spawnCheckRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            SpinnerController spinner = hits[i].GetComponentInParent<SpinnerController>();
            if (spinner != null && spinner.isEnemy && spinner.IsAlive)
                count++;
        }

        return count;
    }

    Vector3 GetFallbackPosition(Vector3 center, List<Vector3> plannedPositions)
    {
        float angleStep = 360f / Mathf.Max(1, spawnAttemptsPerEnemy);

        for (int i = 0; i < spawnAttemptsPerEnemy; i++)
        {
            float radius = spawnCheckRadius * (1f + i * 0.35f);
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

            if (!HasEnemyNear(candidate) && !HasPlannedSpawnNear(candidate, plannedPositions))
                return candidate;
        }

        return center;
    }
}
