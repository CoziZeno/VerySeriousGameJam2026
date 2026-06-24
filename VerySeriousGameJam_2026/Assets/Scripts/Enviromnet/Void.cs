using UnityEngine;

public class KillZone : MonoBehaviour
{
    public GameObject gameOverUI;

    private void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponent<SpinnerController>();

        if (spinner == null)
            return;

        if (other.CompareTag("Player"))
        {
            gameOverUI.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            WaveManager.Instance.EnemyKilled();
            Destroy(gameObject);
        }
    }
}