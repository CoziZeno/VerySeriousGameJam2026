using UnityEngine;

public class KillZone : MonoBehaviour
{
    public GameObject gameOverUI;

    private void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponent<SpinnerController>();

        if (spinner == null)
            return;

        spinner.ForceEliminate();
    }
}
