using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 5;
    public bool destroyOnCollect = true;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (spinner == null)
            return;

        GunUpgradeModule gun = spinner.GetComponentInChildren<GunUpgradeModule>();
        if (gun == null)
            return;

        gun.AddAmmo(ammoAmount);

        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}