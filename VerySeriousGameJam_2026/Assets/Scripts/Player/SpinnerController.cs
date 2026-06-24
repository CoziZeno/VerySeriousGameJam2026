using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class SpinnerController : MonoBehaviour
{
    public static readonly List<SpinnerController> AllSpinners = new List<SpinnerController>();

    [Header("Control")]
    public bool usePlayerInput = true;
    public SpinnerCombat combat;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode attackKey = KeyCode.Space;

    [Header("Movement")]
    public float baseMoveSpeed = 8f;
    public float acceleration = 10f; // Adjusted for ForceMode acceleration
    public float turnSpeed = 14f;
    public float inputDeadZone = 0.08f;

    [Tooltip("If assigned, this transform is rotated for the visible spinner spin.")]
    public Transform visualSpinner;
    public float baseVisualSpinSpeed = 720f;

    [Header("Health & Defense")]
    public int maxHealth = 3;
    public float hitStunDuration = 0.2f;
    public float invulnerabilityDuration = 0.05f; // Shortened so consecutive hits matter
    [Range(0f, 1f)] public float knockbackResistance = 0.1f;

    [Header("Powerups")]
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float radiusMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public float spinMultiplier = 1f;
    public int shieldCharges = 0;

    [Header("Elimination")]
    public bool disableOnEliminate = true;
    public float eliminateDelay = 0.6f;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsStunned => Time.time < _stunnedUntil;
    public bool IsInvulnerable => Time.time < _invulnerableUntil || shieldCharges > 0;

    public Vector3 MoveDirectionWorld { get; private set; }
    public Vector2 CurrentMoveInput { get; private set; }

    public event Action<SpinnerController> OnEliminated;
    public event Action<SpinnerController, int> OnHitTaken;

    public Rigidbody Rb { get; private set; }

    Vector2 _externalMoveInput;
    bool _hasExternalMoveInput;
    float _stunnedUntil;
    float _invulnerableUntil;

    readonly List<ActivePowerup> _activePowerups = new List<ActivePowerup>();

    [Serializable]
    class ActivePowerup
    {
        public PowerupData data;
        public float expiresAt;
    }

    void OnEnable() { if (!AllSpinners.Contains(this)) AllSpinners.Add(this); }
    void OnDisable() { AllSpinners.Remove(this); }

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        Rb.interpolation = RigidbodyInterpolation.Interpolate;
        Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        CurrentHealth = maxHealth;
        if (combat == null) combat = GetComponent<SpinnerCombat>();
    }

    void Update()
    {
        UpdatePowerups();

        if (visualSpinner != null)
        {
            float spinSpeed = baseVisualSpinSpeed * spinMultiplier;
            visualSpinner.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }

        if (!IsAlive) return;

        if (usePlayerInput)
        {
            CurrentMoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            CurrentMoveInput = Vector2.ClampMagnitude(CurrentMoveInput, 1f);

            if (combat != null)
            {
                if (Input.GetKeyDown(dashKey)) combat.TryDash();
                if (Input.GetKeyDown(attackKey)) combat.TryLungeAttack();
            }
        }
        else if (_hasExternalMoveInput)
        {
            CurrentMoveInput = Vector2.ClampMagnitude(_externalMoveInput, 1f);
        }
        else
        {
            CurrentMoveInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (!IsAlive) return;

        // Calculate intended direction based on pure world axes (no camera needed)
        if (CurrentMoveInput.sqrMagnitude > inputDeadZone * inputDeadZone)
        {
            MoveDirectionWorld = new Vector3(CurrentMoveInput.x, 0f, CurrentMoveInput.y).normalized;
        }
        else
        {
            MoveDirectionWorld = Vector3.zero;
        }

        // Apply physics-friendly movement ONLY if not stunned
        if (!IsStunned)
        {
            Vector3 currentVelocity = new Vector3(Rb.linearVelocity.x, 0f, Rb.linearVelocity.z);
            Vector3 desiredVelocity = MoveDirectionWorld * FinalMoveSpeed;

            // Calculate how much force is needed to reach the desired velocity
            Vector3 velocityDifference = desiredVelocity - currentVelocity;

            // Apply it as a force, letting Unity handle momentum and deflections
            Rb.AddForce(velocityDifference * acceleration, ForceMode.Acceleration);
        }

        // Smooth rotation towards movement direction
        if (MoveDirectionWorld.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(MoveDirectionWorld, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    public float FinalMoveSpeed => baseMoveSpeed * speedMultiplier;
    public float FinalDamage => damageMultiplier;
    public float FinalAttackRadius => radiusMultiplier;
    public float FinalCooldownMultiplier => cooldownMultiplier;
    public float FinalSpinMultiplier => spinMultiplier;

    public void SetMoveInput(Vector2 input)
    {
        _externalMoveInput = input;
        _hasExternalMoveInput = true;
    }

    public void ClearMoveInput()
    {
        _externalMoveInput = Vector2.zero;
        _hasExternalMoveInput = false;
    }

    public void LockControl(float duration)
    {
        _stunnedUntil = Mathf.Max(_stunnedUntil, Time.time + duration);
    }

    public void GrantInvulnerability(float duration)
    {
        _invulnerableUntil = Mathf.Max(_invulnerableUntil, Time.time + duration);
    }

    public void ApplyPowerup(PowerupData data)
    {
        if (data == null) return;

        if (data.type == PowerupType.ShieldCharge)
            shieldCharges += Mathf.Max(1, Mathf.RoundToInt(data.magnitude));
        else if (data.type == PowerupType.Heal)
            Heal(Mathf.Max(1, Mathf.RoundToInt(data.magnitude)));
        else
            _activePowerups.Add(new ActivePowerup { data = data, expiresAt = data.duration > 0f ? Time.time + data.duration : float.PositiveInfinity });
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
    }

    public void TakeHit(int damage, Vector3 pushDir, float knockbackForce, float stunDuration)
    {
        if (!IsAlive) return;

        if (IsInvulnerable)
        {
            if (shieldCharges > 0) shieldCharges--;
            return;
        }

        CurrentHealth -= Mathf.Max(1, damage);
        OnHitTaken?.Invoke(this, damage);

        float resistance = Mathf.Clamp01(knockbackResistance);
        float finalKnockback = knockbackForce * (1f - resistance);

        // Actual physics knockback instead of overriding velocity
        Rb.AddForce(pushDir * finalKnockback, ForceMode.Impulse);

        LockControl(stunDuration);
        GrantInvulnerability(invulnerabilityDuration);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnEliminated?.Invoke(this);
            StartCoroutine(EliminateRoutine());
        }
    }

    System.Collections.IEnumerator EliminateRoutine()
    {
        if (eliminateDelay > 0f) yield return new WaitForSeconds(eliminateDelay);
        if (disableOnEliminate) gameObject.SetActive(false);
    }

    void UpdatePowerups()
    {
        if (_activePowerups.Count == 0) return;

        float speed = 1f, damage = 1f, radius = 1f, cooldown = 1f, spin = 1f;

        for (int i = _activePowerups.Count - 1; i >= 0; i--)
        {
            var p = _activePowerups[i];
            if (Time.time >= p.expiresAt)
            {
                _activePowerups.RemoveAt(i);
                continue;
            }

            if (p.data == null) continue;

            switch (p.data.type)
            {
                case PowerupType.SpeedMultiplier: speed *= p.data.magnitude; break;
                case PowerupType.DamageMultiplier: damage *= p.data.magnitude; break;
                case PowerupType.RadiusMultiplier: radius *= p.data.magnitude; break;
                case PowerupType.CooldownMultiplier: cooldown *= p.data.magnitude; break;
                case PowerupType.SpinMultiplier: spin *= p.data.magnitude; break;
            }
        }

        speedMultiplier = speed; damageMultiplier = damage; radiusMultiplier = radius;
        cooldownMultiplier = cooldown; spinMultiplier = spin;
    }
}