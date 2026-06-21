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
    public float preferredDistance = 2.5f;
    public float retargetInterval = 0.35f;
    public float strafeAmount = 0.25f;

    [Header("Dash Behavior")]
    public float dashDecisionChance = 0.02f;
    public float dashRange = 4f;
    public float faceThreshold = 0.45f;

    SpinnerController _target;
    float _nextRetargetTime;

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
            controller.SetMoveInput(Vector2.zero);
            return;
        }

        Vector3 toTarget = _target.transform.position - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        Vector3 dir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;

        if (distance > preferredDistance)
        {
            Vector3 strafe = Vector3.Cross(Vector3.up, dir) * RandomSign() * strafeAmount;
            Vector3 move = (dir + strafe).normalized;
            controller.SetMoveInput(WorldToInput(move));
        }
        else
        {
            controller.SetMoveInput(Vector2.zero);

            float facing = Vector3.Dot(transform.forward, dir);

            if (distance <= dashRange && facing > faceThreshold && combat.CanDash && Random.value < dashDecisionChance)
            {
                combat.TryDash();
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