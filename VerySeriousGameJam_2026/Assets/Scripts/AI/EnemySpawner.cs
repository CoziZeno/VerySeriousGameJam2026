using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    public void SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            Instantiate(
                enemyPrefab,
                spawnPoints[randomIndex].position,
                Quaternion.identity
            );
        }
    }
}