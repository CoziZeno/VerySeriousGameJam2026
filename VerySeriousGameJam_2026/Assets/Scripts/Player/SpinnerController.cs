using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class SpinnerController : MonoBehaviour
{
    public static readonly List<SpinnerController> AllSpinners = new List<SpinnerController>();

    [Header("Control")]
    [Tooltip("If true, reads input from Unity's old Input Manager. If false, use SetMoveInput() from AI or another script.")]
    public bool usePlayerInput = true;

    [Tooltip("Camera transform used to make movement camera-relative. If null, world-relative movement is used.")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float baseMoveSpeed = 8f;
    public float acceleration = 28f;
    public float turnSpeed = 14f;
    public float inputDeadZone = 0.08f;

    [Tooltip("If assigned, this transform is rotated for the visible spinner spin.")]
    public Transform visualSpinner;

    [Tooltip("Degrees per second for the visible spinner rotation.")]
    public float baseVisualSpinSpeed = 720f;

    [Header("Health / Hit Reaction")]
    public int maxHealth = 3;
    public float hitStunDuration = 0.2f;
    public float invulnerabilityDuration = 0.35f;
    public float knockbackResistance = 0.15f;

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

    Rigidbody _rb;
    Vector2 _externalMoveInput;
    bool _hasExternalMoveInput;
    float _stunnedUntil;
    float _invulnerableUntil;
    Vector3 _externalHorizontalVelocity;
    float _externalVelocityUntil;

    readonly List<ActivePowerup> _activePowerups = new List<ActivePowerup>();

    [Serializable]
    class ActivePowerup
    {
        public PowerupData data;
        public float expiresAt;
    }

    void OnEnable()
    {
        if (!AllSpinners.Contains(this))
            AllSpinners.Add(this);
    }

    void OnDisable()
    {
        AllSpinners.Remove(this);
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.angularDamping = 8f;

        CurrentHealth = maxHealth;
    }

    void Update()
    {
        UpdatePowerups();

        if (visualSpinner != null)
        {
            float spinSpeed = baseVisualSpinSpeed * spinMultiplier;
            visualSpinner.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }

        if (usePlayerInput)
        {
            CurrentMoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            CurrentMoveInput = Vector2.ClampMagnitude(CurrentMoveInput, 1f);
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
        if (!IsAlive)
            return;

        bool movementAllowed = !IsStunned;

        Vector3 moveDir = Vector3.zero;
        if (movementAllowed && CurrentMoveInput.sqrMagnitude > inputDeadZone * inputDeadZone)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }

            moveDir = (forward * CurrentMoveInput.y + right * CurrentMoveInput.x).normalized;
        }

        MoveDirectionWorld = moveDir;

        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 desiredHorizontal = Vector3.zero;

        if (movementAllowed)
        {
            desiredHorizontal = moveDir * FinalMoveSpeed;
        }

        if (Time.time < _externalVelocityUntil)
        {
            desiredHorizontal += _externalHorizontalVelocity;
        }

        Vector3 newHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            desiredHorizontal,
            acceleration * Time.fixedDeltaTime
        );

        _rb.linearVelocity = new Vector3(newHorizontal.x, currentVelocity.y, newHorizontal.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
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

    public void AddExternalVelocity(Vector3 worldVelocity, float duration)
    {
        _externalHorizontalVelocity = new Vector3(worldVelocity.x, 0f, worldVelocity.z);
        _externalVelocityUntil = Mathf.Max(_externalVelocityUntil, Time.time + duration);
    }

    public void Stun(float duration)
    {
        _stunnedUntil = Mathf.Max(_stunnedUntil, Time.time + duration);
    }

    public void GrantInvulnerability(float duration)
    {
        _invulnerableUntil = Mathf.Max(_invulnerableUntil, Time.time + duration);
    }

    public void ApplyPowerup(PowerupData data)
    {
        if (data == null)
            return;

        switch (data.type)
        {
            case PowerupType.SpeedMultiplier:
            case PowerupType.DamageMultiplier:
            case PowerupType.RadiusMultiplier:
            case PowerupType.CooldownMultiplier:
            case PowerupType.SpinMultiplier:
                _activePowerups.Add(new ActivePowerup
                {
                    data = data,
                    expiresAt = data.duration > 0f ? Time.time + data.duration : float.PositiveInfinity
                });
                break;

            case PowerupType.ShieldCharge:
                shieldCharges += Mathf.Max(1, Mathf.RoundToInt(data.magnitude));
                break;

            case PowerupType.Heal:
                Heal(Mathf.Max(1, Mathf.RoundToInt(data.magnitude)));
                break;
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive)
            return;

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
    }

    public void TakeHit(int damage, Vector3 hitDirection, float knockbackForce, float stunDuration)
    {
        if (!IsAlive)
            return;

        if (IsInvulnerable)
        {
            if (shieldCharges > 0)
                shieldCharges--;

            return;
        }

        CurrentHealth -= Mathf.Max(1, damage);
        OnHitTaken?.Invoke(this, damage);

        Vector3 push = Vector3.ProjectOnPlane(hitDirection, Vector3.up).normalized * knockbackForce;
        float resistance = Mathf.Clamp01(knockbackResistance);
        _rb.AddForce(push * (1f - resistance), ForceMode.Impulse);

        Stun(stunDuration);
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
        if (eliminateDelay > 0f)
            yield return new WaitForSeconds(eliminateDelay);

        if (disableOnEliminate)
            gameObject.SetActive(false);
    }

    void UpdatePowerups()
    {
        if (_activePowerups.Count == 0)
            return;

        float speed = 1f;
        float damage = 1f;
        float radius = 1f;
        float cooldown = 1f;
        float spin = 1f;

        for (int i = _activePowerups.Count - 1; i >= 0; i--)
        {
            var p = _activePowerups[i];
            if (Time.time >= p.expiresAt)
            {
                _activePowerups.RemoveAt(i);
                continue;
            }

            if (p.data == null)
                continue;

            switch (p.data.type)
            {
                case PowerupType.SpeedMultiplier:
                    speed *= p.data.magnitude;
                    break;
                case PowerupType.DamageMultiplier:
                    damage *= p.data.magnitude;
                    break;
                case PowerupType.RadiusMultiplier:
                    radius *= p.data.magnitude;
                    break;
                case PowerupType.CooldownMultiplier:
                    cooldown *= p.data.magnitude;
                    break;
                case PowerupType.SpinMultiplier:
                    spin *= p.data.magnitude;
                    break;
            }
        }

        speedMultiplier = speed;
        damageMultiplier = damage;
        radiusMultiplier = radius;
        cooldownMultiplier = cooldown;
        spinMultiplier = spin;
    }
}