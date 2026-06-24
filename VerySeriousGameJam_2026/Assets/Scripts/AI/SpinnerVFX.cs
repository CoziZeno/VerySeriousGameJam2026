using UnityEngine;

[RequireComponent(typeof(SpinnerController))]
public class EnemyVFX : MonoBehaviour
{
    public GameObject hitVFX;
    public GameObject deathVFX;
    public Vector3 vfxOffset = new Vector3(0f, 0.5f, 0f);
    public bool makeVFXFollowEnemy = true;
    public float vfxFallbackLifetime = 3f;

    SpinnerController spinner;

    void Awake()
    {
        spinner = GetComponent<SpinnerController>();
    }

    void OnEnable()
    {
        spinner.OnHitTaken += HandleHit;
        spinner.OnEliminated += HandleEliminated;
    }

    void OnDisable()
    {
        spinner.OnHitTaken -= HandleHit;
        spinner.OnEliminated -= HandleEliminated;
    }

    void HandleHit(SpinnerController target, int damage)
    {
        SpawnVFX(hitVFX);
    }

    void HandleEliminated(SpinnerController target)
    {
        SpawnVFX(deathVFX);
    }

    void SpawnVFX(GameObject prefab)
    {
        if (prefab == null)
            return;

        GameObject instance = Instantiate(prefab, transform.position + vfxOffset, Random.rotation);

        VFXAutoDestroy autoDestroy = instance.GetComponent<VFXAutoDestroy>();
        if (autoDestroy == null)
            autoDestroy = instance.AddComponent<VFXAutoDestroy>();

        autoDestroy.fallbackLifetime = vfxFallbackLifetime;

        if (!makeVFXFollowEnemy)
            return;

        VFXFollowTarget follower = instance.GetComponent<VFXFollowTarget>();
        if (follower == null)
            follower = instance.AddComponent<VFXFollowTarget>();

        follower.target = transform;
        follower.offset = vfxOffset;
    }
}
