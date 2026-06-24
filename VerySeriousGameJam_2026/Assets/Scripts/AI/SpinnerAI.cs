using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
[RequireComponent(typeof(SpinnerCombat))]
public class SpinnerAI : MonoBehaviour
{
    public SpinnerController controller;
    public SpinnerCombat combat;

    [Header("Detection")]
    public float senseRadius = 25f;

    [Header("Attack")]
    public float attackRange = 3.5f;
    public float attackCooldown = 3f;

    SpinnerController _target;
    float _nextRetargetTime;
    float _nextAttackTime;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<SpinnerController>();

        if (combat == null)
            combat = GetComponent<SpinnerCombat>();

        controller.usePlayerInput = false;
        controller.isEnemy = true;
    }

    void Update()
    {
        if (controller == null || !controller.IsAlive)
            return;

        if (Time.time >= _nextRetargetTime ||
            _target == null ||
            !_target.IsAlive)
        {
            _target = FindNearestTarget();
            _nextRetargetTime = Time.time + 0.25f;
        }

        if (_target == null)
        {
            controller.SetMoveInput(Vector2.zero);
            return;
        }

        Vector3 toTarget =
            _target.transform.position - transform.position;

        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        Vector3 moveDir =
            distance > 0.001f
            ? toTarget.normalized
            : transform.forward;

        // ALWAYS CHASE
        controller.SetMoveInput(
            new Vector2(moveDir.x, moveDir.z));

        // ATTACK
        if (distance <= attackRange)
        {
            if (Time.time >= _nextAttackTime && combat.CanAttack)
            {
                combat.TryLungeAttack();

                _nextAttackTime =
                    Time.time + attackCooldown;
            }
        }
    }

    void OnDisable()
    {
        if (controller != null)
            controller.ClearMoveInput();
    }

    SpinnerController FindNearestTarget()
    {
        SpinnerController best = null;
        float bestDist = senseRadius;

        for (int i = 0; i < SpinnerController.AllSpinners.Count; i++)
        {
            SpinnerController other =
                SpinnerController.AllSpinners[i];

            if (other == null)
                continue;

            if (other == controller)
                continue;

            if (!other.IsAlive)
                continue;

            if (other.isEnemy == controller.isEnemy)
                continue;

            float dist =
                Vector3.Distance(
                    transform.position,
                    other.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = other;
            }
        }

        return best;
    }
}
