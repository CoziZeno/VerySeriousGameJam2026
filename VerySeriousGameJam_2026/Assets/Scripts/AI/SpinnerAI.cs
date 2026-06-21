using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
[RequireComponent(typeof(SpinnerCombat))]
public class SpinnerAI : MonoBehaviour
{
    public SpinnerController controller;
    public SpinnerCombat combat;

    [Header("AI")]
    public float senseRadius = 20f;
    public float attackRange = 2.4f;
    public float strafeAmount = 0.35f;
    public float retargetInterval = 0.35f;
    public float randomWanderStrength = 0.2f;

    [Header("Targeting")]
    public LayerMask targetMask = ~0;

    SpinnerController _target;
    float _nextRetargetTime;
    Vector2 _wanderDir;

    void Reset()
    {
        controller = GetComponent<SpinnerController>();
        combat = GetComponent<SpinnerCombat>();
    }

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (combat == null) combat = GetComponent<SpinnerCombat>();

        controller.usePlayerInput = false;
    }

    void Update()
    {
        if (controller == null || !controller.IsAlive)
            return;

        if (Time.time >= _nextRetargetTime || _target == null || !_target.IsAlive)
        {
            _target = FindNearestTarget();
            _nextRetargetTime = Time.time + retargetInterval;
        }

        if (_target == null)
        {
            Wander();
            return;
        }

        Vector3 toTarget = _target.transform.position - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        Vector3 targetDir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;

        if (distance > attackRange)
        {
            Vector3 strafe = Vector3.Cross(Vector3.up, targetDir) * RandomSign() * strafeAmount;
            Vector3 move = (targetDir + strafe).normalized;
            controller.SetMoveInput(WorldToInput(move));
        }
        else
        {
            controller.SetMoveInput(Vector2.zero);

            Vector3 forward = transform.forward;
            float facing = Vector3.Dot(forward, targetDir);

            if (facing > 0.4f)
            {
                combat.TryAttack();
            }
            else
            {
                controller.SetMoveInput(WorldToInput(targetDir));
            }
        }
    }

    void OnDisable()
    {
        if (controller != null)
            controller.ClearMoveInput();
    }

    void Wander()
    {
        if (Time.frameCount % 30 == 0)
        {
            _wanderDir = Random.insideUnitCircle.normalized;
        }

        Vector2 input = _wanderDir * randomWanderStrength;
        controller.SetMoveInput(input);
    }

    SpinnerController FindNearestTarget()
    {
        SpinnerController best = null;
        float bestDist = senseRadius;

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

    Vector2 WorldToInput(Vector3 worldDir)
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (controller.cameraTransform != null)
        {
            forward = Vector3.ProjectOnPlane(controller.cameraTransform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(controller.cameraTransform.right, Vector3.up).normalized;
        }

        float x = Vector3.Dot(worldDir, right);
        float y = Vector3.Dot(worldDir, forward);

        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    float RandomSign()
    {
        return Random.value < 0.5f ? -1f : 1f;
    }
}
