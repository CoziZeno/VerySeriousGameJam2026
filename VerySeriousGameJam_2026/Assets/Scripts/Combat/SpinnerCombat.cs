using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerCombat : MonoBehaviour
{
    [Header("Refs")]
    public SpinnerController controller;
    public Rigidbody rb;
    public Transform hitOrigin;

    [Header("Attack")]
    public LayerMask hitMask = ~0;
    public float attackDuration = 0.22f;
    public float attackCooldown = 0.65f;
    public float attackDashSpeed = 14f;
    public float attackKnockbackForce = 10f;
    public float hitStunDuration = 0.25f;
    public int baseDamage = 1;

    [Tooltip("Extra radius added on top of controller powerup multiplier.")]
    public float baseAttackRadius = 1.35f;

    [Tooltip("If true, the attack will only hit each target once per attack.")]
    public bool hitOncePerAttack = true;

    bool _attacking;
    float _nextAttackTime;
    readonly HashSet<SpinnerController> _hitTargets = new HashSet<SpinnerController>();

    void Reset()
    {
        controller = GetComponent<SpinnerController>();
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (hitOrigin == null) hitOrigin = transform;
    }

    public bool CanAttack => !_attacking && Time.time >= _nextAttackTime && controller != null && controller.IsAlive;

    public void TryAttack()
    {
        if (!CanAttack)
            return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        _attacking = true;
        _nextAttackTime = Time.time + attackCooldown * controller.FinalCooldownMultiplier;
        _hitTargets.Clear();

        Vector3 dashDir = controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld
            : transform.forward;

        controller.AddExternalVelocity(dashDir * attackDashSpeed, attackDuration);
        controller.Stun(attackDuration * 0.2f);

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            DoHitCheck(dashDir);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _attacking = false;
    }

    void DoHitCheck(Vector3 dashDir)
    {
        float radius = baseAttackRadius * controller.FinalAttackRadius;
        Vector3 origin = hitOrigin != null ? hitOrigin.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, radius, hitMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            SpinnerController other = hits[i].GetComponentInParent<SpinnerController>();
            if (other == null || other == controller || !other.IsAlive)
                continue;

            if (hitOncePerAttack && _hitTargets.Contains(other))
                continue;

            _hitTargets.Add(other);

            Vector3 hitDirection = (other.transform.position - controller.transform.position).normalized;
            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * controller.FinalDamage));

            other.TakeHit(damage, hitDirection, attackKnockbackForce, hitStunDuration);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (controller == null)
            controller = GetComponent<SpinnerController>();

        float radius = baseAttackRadius;
        if (controller != null)
            radius = baseAttackRadius * controller.radiusMultiplier;

        Vector3 origin = hitOrigin != null ? hitOrigin.position : transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, radius);
    }
}
