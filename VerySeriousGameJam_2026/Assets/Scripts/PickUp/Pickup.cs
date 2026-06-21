using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public PowerupData powerup;
    public bool destroyOnCollect = true;
    public float respawnDelay = 0f;

    Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (spinner == null || powerup == null)
            return;

        spinner.ApplyPowerup(powerup);

        if (destroyOnCollect)
        {
            if (respawnDelay > 0f)
            {
                StartCoroutine(RespawnRoutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator RespawnRoutine()
    {
        _col.enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        _col.enabled = true;
    }
}