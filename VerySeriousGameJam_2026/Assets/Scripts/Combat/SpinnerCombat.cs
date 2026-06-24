using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerCombat : MonoBehaviour
{
    [Header("Refs")]
    public SpinnerController controller;

    [Header("Dash")]
    public float dashDuration = 0.22f;
    public float dashCooldown = 0.65f;
    public float dashForce = 12f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float attackDuration = 0.18f;
    public float attackForce = 25f;
    public int attackDamage = 3;
    public float attackKnockback = 20f;

    [Header("Energy Drain")]
    public float energyLossOnContact = 10f;
    public float contactCooldown = 0.5f;

    public bool IsDashing => Time.time < _dashUntil;
    public bool IsAttacking => Time.time < _attackUntil;
    public bool CanLunge => CanAttack;

    public event Action<SpinnerCombat, int, bool> OnDealtDamage;

    float _dashUntil;
    float _attackUntil;

    float _nextDashTime;
    float _nextAttackTime;

    readonly Dictionary<SpinnerCombat, float> _contactTimers =
        new Dictionary<SpinnerCombat, float>();
    readonly HashSet<SpinnerCombat> _hitTargetsThisAttack =
        new HashSet<SpinnerCombat>();

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<SpinnerController>();
    }

    public bool CanDash =>
        controller != null &&
        controller.IsAlive &&
        Time.time >= _nextDashTime &&
        !IsDashing &&
        !IsAttacking;

    public bool CanAttack =>
        controller != null &&
        controller.IsAlive &&
        Time.time >= _nextAttackTime &&
        !IsDashing &&
        !IsAttacking;

    public void TryDash()
    {
        if (!CanDash)
            return;

        Vector3 dashDir =
            controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld
            : transform.forward;

        _dashUntil = Time.time + dashDuration;
        _nextDashTime = Time.time + dashCooldown;

        controller.GrantInvulnerability(dashDuration);

        controller.Rb.AddForce(
            dashDir * dashForce,
            ForceMode.VelocityChange);

        controller.LockControl(dashDuration * 0.5f);
    }

    public void TryLungeAttack()
    {
        if (!CanAttack)
            return;

        Vector3 attackDir =
            controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld
            : transform.forward;

        _attackUntil = Time.time + attackDuration;
        _nextAttackTime =
            Time.time + attackCooldown * controller.FinalCooldownMultiplier;

        _hitTargetsThisAttack.Clear();

        controller.Rb.AddForce(
            attackDir * attackForce,
            ForceMode.VelocityChange);

        controller.LockControl(attackDuration);
    }

    void OnCollisionEnter(Collision collision)
    {
        SpinnerCombat other =
            collision.collider.GetComponentInParent<SpinnerCombat>();

        if (other == null || other == this)
            return;

        HandleCollision(other);
    }

    void OnCollisionStay(Collision collision)
    {
        SpinnerCombat other =
            collision.collider.GetComponentInParent<SpinnerCombat>();

        if (other == null || other == this)
            return;

        HandleCollision(other);
    }

    void HandleCollision(SpinnerCombat other)
    {
        if (controller == null || other.controller == null)
            return;

        if (controller.isEnemy == other.controller.isEnemy)
            return;

        if (IsAttacking)
            DamageTarget(other);

        if (controller.isEnemy && !other.controller.isEnemy)
            DrainPlayerEnergy(other);
    }

    void DamageTarget(SpinnerCombat target)
    {
        if (!target.controller.IsAlive)
            return;

        if (_hitTargetsThisAttack.Contains(target))
            return;

        _hitTargetsThisAttack.Add(target);

        Vector3 pushDirection =
            target.transform.position - transform.position;

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.001f)
            pushDirection = transform.forward;

        target.controller.TakeHit(
            Mathf.RoundToInt(attackDamage * controller.FinalDamage),
            pushDirection.normalized,
            attackKnockback,
            0.35f);

        OnDealtDamage?.Invoke(
            target,
            attackDamage,
            true);
    }

    void DrainPlayerEnergy(SpinnerCombat player)
    {
        if (_contactTimers.TryGetValue(player, out float nextTime))
        {
            if (Time.time < nextTime)
                return;
        }

        _contactTimers[player] =
            Time.time + contactCooldown;

        player.controller.DrainSpinEnergy(energyLossOnContact);
    }
}
