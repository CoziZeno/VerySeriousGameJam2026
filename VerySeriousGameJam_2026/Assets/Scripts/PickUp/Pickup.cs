using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public PowerupData powerup;
    public UpgradeData upgrade;

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
        if (spinner == null)
            return;

        if (powerup != null)
        {
            spinner.ApplyPowerup(powerup);
        }

        if (upgrade != null)
        {
            SpinnerUpgradeManager upgradeManager = spinner.GetComponent<SpinnerUpgradeManager>();
            if (upgradeManager != null)
            {
                upgradeManager.AddUpgrade(upgrade);
            }
            else
            {
                Debug.LogWarning($"{spinner.name} has no SpinnerUpgradeManager, cannot add upgrade '{upgrade.displayName}'.");
            }
        }

        if (destroyOnCollect)
        {
            if (respawnDelay > 0f)
                StartCoroutine(RespawnRoutine());
            else
                Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator RespawnRoutine()
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