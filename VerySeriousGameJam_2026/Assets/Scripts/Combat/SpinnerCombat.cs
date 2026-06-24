using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerCombat : MonoBehaviour
{
    [Header("Refs")]
    public SpinnerController controller;

    [Header("Momentum Combat")]
    public float minImpactThreshold = 2f;
    public float velocityDamageScaling = 0.5f;
    public float velocityKnockbackScaling = 1.5f;
    public int maxHitDamage = 5;

    [Header("Contact Base Stats")]
    public int baseContactDamage = 1;
    public float baseContactKnockback = 8f;
    public float contactStunDuration = 0.15f;

    [Header("Dash Attack")]
    public float dashDuration = 0.22f;
    public float dashCooldown = 0.65f;
    public float dashForce = 20f;
    public float dashDamageMultiplier = 1.5f;
    public float dashKnockbackMultiplier = 2.0f;

    [Header("Lunge Attack")]
    public float lungeCooldown = 2f;
    public float lungeDuration = 0.18f;
    public float lungeForce = 25f;

    public bool IsDashing => Time.time < _dashUntil;
    public bool IsLunging => Time.time < _lungeUntil;

    // Events for SpinnerUpgradeManager
    public event Action<SpinnerCombat, int, bool> OnDealtDamage;
    public event Action<SpinnerCombat, bool> OnClash;

    float _dashUntil;
    float _nextDashTime;
    float _lungeUntil;
    float _nextLungeTime;

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
    }

    public bool CanDash => controller != null && controller.IsAlive && Time.time >= _nextDashTime && !IsDashing && !IsLunging;
    public bool CanLunge => controller != null && controller.IsAlive && Time.time >= _nextLungeTime && !IsDashing && !IsLunging;

    public void TryDash()
    {
        if (!CanDash) return;

        Vector3 dashDir = controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld : transform.forward;

        _dashUntil = Time.time + dashDuration;
        _nextDashTime = Time.time + dashCooldown * controller.FinalCooldownMultiplier;

        // Apply burst of physical speed
        controller.Rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);
        controller.LockControl(dashDuration * 0.5f);
    }

    public void TryLungeAttack()
    {
        if (!CanLunge) return;

        // Attacks in the direction we are moving
        Vector3 attackDir = controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld : transform.forward;

        _lungeUntil = Time.time + lungeDuration;
        _nextLungeTime = Time.time + lungeCooldown * controller.FinalCooldownMultiplier;

        controller.Rb.AddForce(attackDir * lungeForce, ForceMode.VelocityChange);
        controller.LockControl(lungeDuration);
    }

    void OnCollisionEnter(Collision collision)
    {
        SpinnerCombat other = collision.collider.GetComponentInParent<SpinnerCombat>();
        if (other == null || other == this) return;

        if (GetInstanceID() > other.GetInstanceID()) return;

        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minImpactThreshold) return;

        Vector3 hitNormal = collision.GetContact(0).normal;
        hitNormal.y = 0;
        hitNormal.Normalize();

        ResolveDynamicImpact(this, other, impactForce, hitNormal);
    }

    static void ResolveDynamicImpact(SpinnerCombat a, SpinnerCombat b, float impactForce, Vector3 hitNormalFromBtoA)
    {
        bool aAggressive = a.IsDashing || a.IsLunging;
        bool bAggressive = b.IsDashing || b.IsLunging;

        if (aAggressive && bAggressive)
        {
            a.OnClash?.Invoke(b, true);
            b.OnClash?.Invoke(a, true);

            a.ProcessHitReceived(b, impactForce, hitNormalFromBtoA, isClash: true);
            b.ProcessHitReceived(a, impactForce, -hitNormalFromBtoA, isClash: true);
            return;
        }

        a.ProcessHitReceived(b, impactForce, hitNormalFromBtoA, isClash: false);
        b.ProcessHitReceived(a, impactForce, -hitNormalFromBtoA, isClash: false);
    }

    void ProcessHitReceived(SpinnerCombat attacker, float impactForce, Vector3 pushDirection, bool isClash)
    {
        if (controller == null || !controller.IsAlive) return;

        bool attackerAggressive = attacker.IsDashing || attacker.IsLunging;
        bool iAmAggressive = IsDashing || IsLunging;

        float rawDamage = baseContactDamage + (impactForce * velocityDamageScaling);
        if (attackerAggressive && !isClash) rawDamage *= attacker.dashDamageMultiplier;
        if (isClash) rawDamage *= 0.25f; // Clashes do chip damage
        if (iAmAggressive && !isClash) rawDamage *= 0.5f; // Armor while aggressive

        int finalDamage = Mathf.RoundToInt(rawDamage * attacker.controller.FinalDamage);
        finalDamage = Mathf.Clamp(finalDamage, 0, maxHitDamage);

        float rawKnockback = baseContactKnockback + (impactForce * velocityKnockbackScaling);
        if (attackerAggressive || isClash) rawKnockback *= attacker.dashKnockbackMultiplier;

        float stun = contactStunDuration * (attackerAggressive ? 1.5f : 1f);

        if (finalDamage > 0)
        {
            controller.TakeHit(finalDamage, pushDirection, rawKnockback, stun);
            attacker.OnDealtDamage?.Invoke(this, finalDamage, attackerAggressive);
        }
        else
        {
            // Just push them physically
            controller.Rb.AddForce(pushDirection * rawKnockback, ForceMode.Impulse);
        }
    }
}