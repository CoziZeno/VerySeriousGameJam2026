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

    [Header("Melee Attack")]
    public Camera aimCamera;
    public float meleeAttackCooldown = 3f;
    public float meleeAttackDuration = 0.18f;
    public float meleeAttackSpeed = 16f;
    public float meleeReturnDuration = 0.16f;
    public float meleeReturnSpeed = 14f;

    [Tooltip("While dashing, this spinner has contact priority.")]
    public bool IsDashing => Time.time < _dashUntil;

    float _dashUntil;
    float _nextDashTime;
    float _nextMeleeAttackTime;
    bool _isMeleeAttacking;

    void Reset()
    {
        controller = GetComponent<SpinnerController>();
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (aimCamera == null) aimCamera = Camera.main;
    }

    public bool CanDash => controller != null && controller.IsAlive && Time.time >= _nextDashTime && !IsDashing && !_isMeleeAttacking;
    public bool CanMeleeAttack => controller != null && controller.IsAlive && Time.time >= _nextMeleeAttackTime && !IsDashing && !_isMeleeAttacking;

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

    public void TryMeleeAttackAtCursor()
    {
        if (!CanMeleeAttack)
            return;

        Vector3 attackDir = GetCursorDirectionWorld();
        if (attackDir.sqrMagnitude < 0.001f)
            attackDir = transform.forward;

        _nextMeleeAttackTime = Time.time + meleeAttackCooldown * controller.FinalCooldownMultiplier;
        StartCoroutine(MeleeAttackRoutine(attackDir));
    }

    System.Collections.IEnumerator MeleeAttackRoutine(Vector3 attackDir)
    {
        _isMeleeAttacking = true;

        Vector3 startPosition = transform.position;

        _dashUntil = Time.time + meleeAttackDuration;
        controller.ApplyForcedHorizontalVelocity(attackDir * meleeAttackSpeed, meleeAttackDuration);
        controller.LockControl(meleeAttackDuration + meleeReturnDuration);

        yield return new WaitForSeconds(meleeAttackDuration);

        if (controller == null || !controller.IsAlive)
        {
            _isMeleeAttacking = false;
            yield break;
        }

        Vector3 returnDir = startPosition - transform.position;
        returnDir.y = 0f;

        if (returnDir.sqrMagnitude < 0.001f)
            returnDir = -attackDir;

        controller.ApplyForcedHorizontalVelocity(returnDir.normalized * meleeReturnSpeed, meleeReturnDuration);

        yield return new WaitForSeconds(meleeReturnDuration);

        _isMeleeAttacking = false;
    }

    Vector3 GetCursorDirectionWorld()
    {
        Camera cam = aimCamera != null ? aimCamera : Camera.main;
        if (cam == null)
            return transform.forward;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return transform.forward;

        Vector3 cursorWorldPosition = ray.GetPoint(distance);
        Vector3 direction = cursorWorldPosition - transform.position;
        direction.y = 0f;

        return direction.normalized;
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
