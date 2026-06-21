using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerCombat : MonoBehaviour
{
    [Header("Refs")]
    public SpinnerController controller;
    public Rigidbody rb;

    [Header("Contact Combat")]
    public int baseContactDamage = 1;
    public float contactKnockbackForce = 8f;
    public float contactStunDuration = 0.2f;

    [Header("Dash")]
    public float dashDuration = 0.22f;
    public float dashCooldown = 0.65f;
    public float dashSpeed = 14f;
    public int dashDamageBonus = 1;
    public float dashKnockbackMultiplier = 1.2f;

    [Tooltip("While dashing, this spinner has contact priority.")]
    public bool IsDashing => Time.time < _dashUntil;

    float _dashUntil;
    float _nextDashTime;

    void Reset()
    {
        controller = GetComponent<SpinnerController>();
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    public bool CanDash => controller != null && controller.IsAlive && Time.time >= _nextDashTime && !IsDashing;

    public void TryDash()
    {
        if (!CanDash)
            return;

        Vector3 dashDir = controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld
            : transform.forward;

        _dashUntil = Time.time + dashDuration;
        _nextDashTime = Time.time + dashCooldown * controller.FinalCooldownMultiplier;

        controller.ApplyForcedHorizontalVelocity(dashDir * dashSpeed, dashDuration);
        controller.LockControl(dashDuration * 0.15f);
    }

    void OnCollisionEnter(Collision collision)
    {
        SpinnerCombat other = collision.collider.GetComponentInParent<SpinnerCombat>();
        if (other == null || other == this)
            return;

        // Resolve each pair only once.
        if (GetInstanceID() > other.GetInstanceID())
            return;

        ResolveContactPair(this, other);
    }

    static void ResolveContactPair(SpinnerCombat a, SpinnerCombat b)
    {
        bool aDash = a.IsDashing;
        bool bDash = b.IsDashing;

        // Dash priority:
        // - dashing vs normal => only the dashing one damages
        // - normal vs normal => both damage each other
        // - dashing vs dashing => both damage each other
        if (aDash && !bDash)
        {
            a.DealHitTo(b, true);
            return;
        }

        if (!aDash && bDash)
        {
            b.DealHitTo(a, true);
            return;
        }

        a.DealHitTo(b, aDash);
        b.DealHitTo(a, bDash);
    }

    void DealHitTo(SpinnerCombat victim, bool wasDashHit)
    {
        if (victim == null || victim.controller == null || !victim.controller.IsAlive)
            return;

        Vector3 hitDirection = (victim.transform.position - transform.position).normalized;

        int damage = Mathf.Max(1, baseContactDamage + (wasDashHit ? dashDamageBonus : 0));
        damage = Mathf.RoundToInt(damage * controller.FinalDamage);

        float knockback = contactKnockbackForce * (wasDashHit ? dashKnockbackMultiplier : 1f);
        float stun = contactStunDuration * (wasDashHit ? 0.85f : 1f);

        victim.controller.TakeHit(damage, hitDirection, knockback, stun);
    }
}