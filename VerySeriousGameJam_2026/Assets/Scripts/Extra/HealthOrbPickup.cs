using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class HealthOrbPickup : MonoBehaviour
{
    public int healAmount = 1;
    public float autoDestroyTime = 20f;
    public bool playerOnly = true;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        Destroy(gameObject, autoDestroyTime);
    }

    void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (spinner == null || !spinner.IsAlive)
            return;

        if (playerOnly && !spinner.usePlayerInput)
            return;

        if (spinner.CurrentHealth >= spinner.maxHealth)
            return;

        spinner.Heal(Mathf.Max(1, healAmount));
        Destroy(gameObject);
    }
}
