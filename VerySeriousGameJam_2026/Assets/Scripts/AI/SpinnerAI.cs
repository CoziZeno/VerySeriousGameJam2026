using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
[RequireComponent(typeof(SpinnerCombat))]
public class SpinnerAI : MonoBehaviour
{
    public SpinnerController controller;
    public SpinnerCombat combat;

    [Header("AI Senses")]
    public float senseRadius = 25f;
    public float preferredDistance = 1.5f;
    public float orbitChangeInterval = 2.5f;

    [Header("Aggression")]
    public float attackRange = 4.5f;
    public float faceThreshold = 0.8f;

    private SpinnerController _target;
    private float _nextRetargetTime;

    private float _currentOrbitDirection = 1f;
    private float _nextOrbitChangeTime;
    private float _nextActionAttemptTime;

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (combat == null) combat = GetComponent<SpinnerCombat>();

        controller.usePlayerInput = false;
    }

    void Update()
    {
        if (controller == null || !controller.IsAlive) return;

        if (Time.time >= _nextRetargetTime || _target == null || !_target.IsAlive)
        {
            _target = FindNearestTarget();
            _nextRetargetTime = Time.time + 0.5f;
        }

        if (_target == null)
        {
            controller.SetMoveInput(Vector2.zero);
            return;
        }

        Vector3 toTarget = _target.transform.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        Vector3 dirToTarget = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;

        if (Time.time > _nextOrbitChangeTime)
        {
            _currentOrbitDirection = Random.value > 0.5f ? 1f : -1f;
            _nextOrbitChangeTime = Time.time + orbitChangeInterval * Random.Range(0.8f, 1.2f);
        }

        Vector3 orbitDir = Vector3.Cross(Vector3.up, dirToTarget) * _currentOrbitDirection;
        float distanceFactor = Mathf.Clamp01((distance - preferredDistance) / 3f);
        Vector3 moveDir = Vector3.Lerp(orbitDir, dirToTarget, distanceFactor).normalized;

        // Supply World X and Z as Input since the Controller no longer uses Cameras
        controller.SetMoveInput(new Vector2(moveDir.x, moveDir.z));

        float facing = Vector3.Dot(transform.forward, dirToTarget);

        // Attack logic based purely on distance and facing direction
        if (distance <= attackRange && facing > faceThreshold)
        {
            if (Time.time > _nextActionAttemptTime)
            {
                if (Random.value > 0.5f && combat.CanLunge)
                    combat.TryLungeAttack();
                else if (combat.CanDash)
                    combat.TryDash();

                _nextActionAttemptTime = Time.time + Random.Range(0.5f, 1.5f);
            }
        }
    }

    void OnDisable()
    {
        if (controller != null) controller.ClearMoveInput();
    }

    SpinnerController FindNearestTarget()
    {
        SpinnerController best = null;
        float bestDist = senseRadius;

        for (int i = 0; i < SpinnerController.AllSpinners.Count; i++)
        {
            SpinnerController other = SpinnerController.AllSpinners[i];
            if (other == null || other == controller || !other.IsAlive) continue;

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