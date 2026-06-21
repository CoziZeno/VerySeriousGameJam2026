using UnityEngine;

public class FlamethrowerUpgradeModule : SpinnerUpgradeModule
{
    public float range = 4f;
    public float coneAngle = 55f;
    public float damagePerSecond = 6f;
    public LayerMask targetMask = ~0;

    float _damageAccumulator;

    public override void TickModule(float deltaTime)
    {
        if (controller == null || !controller.IsAlive)
            return;

        _damageAccumulator += damagePerSecond * deltaTime;
        int damageTicks = Mathf.FloorToInt(_damageAccumulator);

        if (damageTicks <= 0)
            return;

        _damageAccumulator -= damageTicks;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        for (int i = 0; i < SpinnerController.AllSpinners.Count; i++)
        {
            SpinnerController other = SpinnerController.AllSpinners[i];
            if (other == null || other == controller || !other.IsAlive)
                continue;

            Vector3 toTarget = other.transform.position - origin;
            toTarget.y = 0f;

            float dist = toTarget.magnitude;
            if (dist > range)
                continue;

            float angle = Vector3.Angle(forward, toTarget.normalized);
            if (angle > coneAngle * 0.5f)
                continue;

            other.TakeHit(damageTicks, toTarget.normalized, 3.5f, 0.1f);
        }
    }
}