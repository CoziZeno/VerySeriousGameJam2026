using UnityEngine;

public class GunUpgradeModule : SpinnerUpgradeModule
{
    [Header("Projectile")]
    public GunProjectile bulletPrefab;
    public Transform muzzle;
    public float bulletSpeed = 22f;
    public float fireInterval = 0.45f;
    public float range = 12f;
    public int damage = 1;
    public float knockbackForce = 6f;
    public float stunDuration = 0.12f;
    public LayerMask targetMask = ~0;

    [Header("Ammo")]
    public int currentAmmo = 12;
    public int maxAmmo = 30;
    public int ammoPerShot = 1;
    public int ammoOnKill = 1;

    [Header("Input")]
    public KeyCode fireKey = KeyCode.Return;
    public bool playerOnly = true;

    float _nextFireTime;

    public override void TickModule(float deltaTime)
    {
        if (controller == null || !controller.IsAlive)
            return;

        if (playerOnly && !controller.usePlayerInput)
            return;

        if (!Input.GetKeyDown(fireKey) && !Input.GetKeyDown(KeyCode.KeypadEnter))
            return;

        TryFire();
    }

    public bool TryFire()
    {
        if (Time.time < _nextFireTime)
            return false;

        if (currentAmmo < ammoPerShot)
            return false;

        if (bulletPrefab == null)
        {
            Debug.LogWarning($"{name}: GunUpgradeModule has no bulletPrefab assigned.");
            return false;
        }

        Vector3 dir = GetFireDirection();
        if (dir.sqrMagnitude < 0.001f)
            dir = transform.forward;

        Vector3 spawnPos = muzzle != null
            ? muzzle.position
            : transform.position + dir.normalized * 0.5f + Vector3.up * 0.15f;

        Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        GunProjectile bullet = Object.Instantiate(bulletPrefab, spawnPos, rot);
        bullet.Initialize(
            ownerController: controller,
            ownerGun: this,
            direction: dir.normalized,
            speed: bulletSpeed,
            range: range,
            damage: Mathf.Max(1, Mathf.RoundToInt(damage * controller.FinalDamage)),
            knockbackForce: knockbackForce,
            stunDuration: stunDuration,
            targetMask: targetMask
        );

        currentAmmo -= ammoPerShot;
        _nextFireTime = Time.time + fireInterval * controller.FinalCooldownMultiplier;

        return true;
    }

    public void AddAmmo(int amount)
    {
        if (amount <= 0)
            return;

        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    }

    Vector3 GetFireDirection()
    {
        // Fire in the direction the spinner is facing.
        // This keeps it simple and readable for a jam game.
        return transform.forward;
    }
}