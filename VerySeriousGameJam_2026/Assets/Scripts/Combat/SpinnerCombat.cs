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
    public GameObject dashVFXPrefab;
    public Vector3 dashVFXOffset = new Vector3(0f, 0.2f, 0f);
    public float dashVFXLifetime = 0.35f;
    public bool addFallbackDashTrail = true;
    public Color dashFlashColor = Color.white;
    public float dashFlashDuration = 0.12f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float attackDuration = 0.18f;
    public float attackForce = 25f;
    public int attackDamage = 3;
    public float attackKnockback = 20f;

    [Header("Sweep Attack")]
    public SweepSlashProjectile sweepSlashPrefab;
    public Transform sweepSpawnPoint;
    public float sweepCooldown = 1.2f;
    public float sweepSpeed = 18f;
    public float sweepRange = 10f;
    public int sweepDamage = 2;
    public float sweepKnockback = 9f;
    public float sweepStunDuration = 0.15f;
    public Vector3 sweepSlashScale = new Vector3(2.2f, 0.08f, 0.45f);
    public LayerMask sweepTargetMask = ~0;
    public GameObject sweepSlashVFXPrefab;
    public bool addFallbackSweepTrail = true;
    public bool sweepUnlocked;

    [Header("Energy Drain")]
    public float energyLossOnContact = 10f;
    public float contactCooldown = 0.5f;

    public bool IsDashing => Time.time < _dashUntil;
    public bool IsAttacking => Time.time < _attackUntil;
    public bool CanLunge => CanAttack;
    public bool CanSweep =>
        controller != null &&
        controller.IsAlive &&
        sweepUnlocked &&
        Time.time >= _nextSweepTime &&
        !IsDashing &&
        !IsAttacking;

    public event Action<SpinnerCombat, int, bool> OnDealtDamage;

    float _dashUntil;
    float _attackUntil;

    float _nextDashTime;
    float _nextAttackTime;
    float _nextSweepTime;

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

        if (!SpendPlayerMoveEnergy(controller.spinEnergy != null ? controller.spinEnergy.dashCost : 0f))
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

        PlayDashFeedback(dashDir);

        AudioController.Instance?.PlayDash();
    }

    public void TryLungeAttack()
    {
        if (!CanAttack)
            return;

        if (!SpendPlayerMoveEnergy(controller.spinEnergy != null ? controller.spinEnergy.attackCost : 0f))
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

        AudioController.Instance?.PlayAttack();
    }

    public void TrySweepAttack()
    {
        if (!CanSweep)
            return;

        Vector3 sweepDir =
            controller.MoveDirectionWorld.sqrMagnitude > 0.001f
            ? controller.MoveDirectionWorld
            : transform.forward;

        sweepDir.y = 0f;
        if (sweepDir.sqrMagnitude < 0.001f)
            sweepDir = transform.forward;

        sweepDir.Normalize();

        Vector3 spawnPos = sweepSpawnPoint != null
            ? sweepSpawnPoint.position
            : transform.position + sweepDir * 1.1f + Vector3.up * 0.25f;

        Quaternion spawnRot = Quaternion.LookRotation(sweepDir, Vector3.up);
        SweepSlashProjectile slash = CreateSweepSlash(spawnPos, spawnRot);

        slash.Initialize(
            ownerCombat: this,
            direction: sweepDir,
            speed: sweepSpeed,
            range: sweepRange,
            damage: Mathf.Max(1, Mathf.RoundToInt(sweepDamage * controller.FinalDamage)),
            knockbackForce: sweepKnockback,
            stunDuration: sweepStunDuration,
            targetMask: sweepTargetMask);

        _nextSweepTime = Time.time + sweepCooldown * controller.FinalCooldownMultiplier;

        AudioController.Instance?.PlayAttack();
    }

    public void UnlockSweepAttack()
    {
        sweepUnlocked = true;
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

    bool SpendPlayerMoveEnergy(float amount)
    {
        if (controller == null || controller.isEnemy || amount <= 0f)
            return true;

        if (controller.spinEnergy == null)
            return true;

        return controller.spinEnergy.SpendEnergy(amount);
    }

    void PlayDashFeedback(Vector3 dashDir)
    {
        if (controller != null)
            controller.TriggerColorFlash(dashFlashColor, dashFlashDuration);

        if (dashVFXPrefab != null)
        {
            Quaternion rotation = dashDir.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(dashDir.normalized, Vector3.up)
                : transform.rotation;

            GameObject dashVFX = Instantiate(
                dashVFXPrefab,
                transform.position + dashVFXOffset,
                rotation,
                transform);

            VFXAutoDestroy autoDestroy = dashVFX.GetComponent<VFXAutoDestroy>();
            if (autoDestroy == null)
                autoDestroy = dashVFX.AddComponent<VFXAutoDestroy>();

            autoDestroy.fallbackLifetime = dashVFXLifetime;
        }

        if (addFallbackDashTrail)
            StartCoroutine(DashTrailRoutine());
    }

    System.Collections.IEnumerator DashTrailRoutine()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.minVertexDistance = 0.03f;
        trail.widthMultiplier = 1.25f;
        trail.alignment = LineAlignment.View;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.55f, 0.95f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.45f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        trail.colorGradient = gradient;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            trail.material = new Material(shader);

        yield return new WaitForSeconds(dashDuration + trail.time);

        if (trail != null)
            Destroy(trail);
    }

    SweepSlashProjectile CreateSweepSlash(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (sweepSlashPrefab != null)
            return Instantiate(sweepSlashPrefab, spawnPos, spawnRot);

        GameObject slashObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slashObject.name = "Sweep Slash";
        slashObject.transform.SetPositionAndRotation(spawnPos, spawnRot);
        slashObject.transform.localScale = sweepSlashScale;

        Renderer slashRenderer = slashObject.GetComponent<Renderer>();
        if (slashRenderer != null)
            slashRenderer.material.color = new Color(0.25f, 0.9f, 1f, 0.85f);

        AddSweepSlashVFX(slashObject);

        return slashObject.AddComponent<SweepSlashProjectile>();
    }

    void AddSweepSlashVFX(GameObject slashObject)
    {
        if (slashObject == null)
            return;

        if (sweepSlashVFXPrefab != null)
        {
            GameObject vfx = Instantiate(sweepSlashVFXPrefab, slashObject.transform);
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localRotation = Quaternion.identity;
        }

        if (!addFallbackSweepTrail)
            return;

        TrailRenderer trail = slashObject.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.minVertexDistance = 0.03f;
        trail.widthMultiplier = 1.15f;
        trail.alignment = LineAlignment.TransformZ;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.2f, 1f, 1f), 0f),
                new GradientColorKey(Color.white, 0.35f),
                new GradientColorKey(new Color(0.05f, 0.35f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.55f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        trail.colorGradient = gradient;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            trail.material = new Material(shader);
    }

    public bool TryDamageSweepTarget(
        SpinnerCombat target,
        int damage,
        Vector3 pushDirection,
        float knockbackForce,
        float stunDuration)
    {
        if (controller == null || target == null || target.controller == null)
            return false;

        if (target == this || !target.controller.IsAlive)
            return false;

        if (controller.isEnemy == target.controller.isEnemy)
            return false;

        pushDirection.y = 0f;
        if (pushDirection.sqrMagnitude < 0.001f)
            pushDirection = transform.forward;

        target.controller.TakeHit(
            damage,
            pushDirection.normalized,
            knockbackForce,
            stunDuration);

        OnDealtDamage?.Invoke(
            target,
            damage,
            true);

        // If attacker is player and target died, register kill with progress manager
        bool attackerIsPlayer = controller != null && !controller.isEnemy;
        if (attackerIsPlayer && target.controller.CurrentHealth <= 0)
        {
            GameProgressManager.Instance?.RegisterKill(true, target.transform.position);
        }

        return true;
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

        bool attackerIsPlayer = controller != null && !controller.isEnemy;
        if (attackerIsPlayer && target.controller.CurrentHealth <= 0)
        {
            GameProgressManager.Instance?.RegisterKill(true, target.transform.position);
        }
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
