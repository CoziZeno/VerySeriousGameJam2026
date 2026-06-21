using UnityEngine;

public class GunUpgradeModule : SpinnerUpgradeModule
{
    public Transform muzzle;
    public float fireInterval = 0.45f;
    public float range = 12f;
    public int damage = 1;
    public LayerMask targetMask = ~0;

    float _nextFireTime;

    public override void TickModule(float deltaTime)
    {
        if (controller == null || !controller.IsAlive)
            return;

        if (Time.time < _nextFireTime)
            return;

        SpinnerController target = FindTarget();
        if (target == null)
            return;

        Vector3 origin = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = (target.transform.position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, targetMask, QueryTriggerInteraction.Ignore))
        {
            SpinnerController hitSpinner = hit.collider.GetComponentInParent<SpinnerController>();
            if (hitSpinner != null && hitSpinner != controller)
            {
                hitSpinner.TakeHit(Mathf.Max(1, damage), dir, 6f, 0.12f);
                _nextFireTime = Time.time + fireInterval;
            }
        }
    }

    SpinnerController FindTarget()
    {
        SpinnerController best = null;
        float bestDist = range;

        for (int i = 0; i < SpinnerController.AllSpinners.Count; i++)
        {
            SpinnerController other = SpinnerController.AllSpinners[i];
            if (other == null || other == controller || !other.IsAlive)
                continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = other;
            }
        }

        return best;
    }
}