using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    public List<SpinnerController> SpawnWave(int enemyCount)
    {
        List<SpinnerController> spawnedEnemies = new List<SpinnerController>(enemyCount);

        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner is missing an enemy prefab or spawn points.");
            return spawnedEnemies;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            GameObject enemyInstance = Instantiate(
                enemyPrefab,
                spawnPoints[randomIndex].position,
                Quaternion.identity
            );

            SpinnerController spinner = enemyInstance.GetComponent<SpinnerController>();
            if (spinner != null)
                spawnedEnemies.Add(spinner);
        }

        return spawnedEnemies;
    }
}
