using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public float autoDestroyTime = 20f;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        Destroy(gameObject, autoDestroyTime);
    }

    void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (spinner == null || !spinner.usePlayerInput)
            return;

        GameProgressManager.Instance?.AddCoins(coinValue);
        Destroy(gameObject);
    }
}