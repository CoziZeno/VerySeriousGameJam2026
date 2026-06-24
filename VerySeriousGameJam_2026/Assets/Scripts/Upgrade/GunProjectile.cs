using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GunProjectile : MonoBehaviour
{
    Rigidbody _rb;
    Collider _col;

    SpinnerController _ownerController;
    GunUpgradeModule _ownerGun;

    Vector3 _direction;
    float _knockbackForce;
    float _stunDuration;
    int _damage;
    bool _initialized;
    bool _hitSomething;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Projectiles work best as trigger bullets for simple jam logic.
        _col.isTrigger = true;
    }

    public void Initialize(
        SpinnerController ownerController,
        GunUpgradeModule ownerGun,
        Vector3 direction,
        float speed,
        float range,
        int damage,
        float knockbackForce,
        float stunDuration,
        LayerMask targetMask)
    {
        _ownerController = ownerController;
        _ownerGun = ownerGun;
        _direction = direction.normalized;
        _damage = damage;
        _knockbackForce = knockbackForce;
        _stunDuration = stunDuration;
        _initialized = true;

        transform.forward = _direction;
        _rb.linearVelocity = _direction * speed;

        float lifetime = Mathf.Max(0.25f, range / Mathf.Max(0.01f, speed) + 0.25f);
        Destroy(gameObject, lifetime);

        IgnoreOwnerCollisions();
    }

    void FixedUpdate()
    {
        if (!_initialized || _hitSomething)
            return;

        // Keep the bullet moving even if physics gets messy.
        _rb.linearVelocity = _direction * _rb.linearVelocity.magnitude;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_initialized || _hitSomething)
            return;

        SpinnerController hitSpinner = other.GetComponentInParent<SpinnerController>();

        if (hitSpinner != null)
        {
            if (hitSpinner == _ownerController)
                return;

            _hitSomething = true;
            hitSpinner.TakeHit(_damage, _direction, _knockbackForce, _stunDuration);

            // Ammo reward if this bullet kills the enemy.
            if (!hitSpinner.IsAlive && _ownerGun != null)
                _ownerGun.AddAmmo(_ownerGun.ammoOnKill);

            Destroy(gameObject);
            return;
        }

        // Hit a wall / ground / anything else.
        if (!other.isTrigger)
        {
            _hitSomething = true;
            Destroy(gameObject);
        }
    }

    void IgnoreOwnerCollisions()
    {
        if (_ownerController == null)
            return;

        Collider[] ownerColliders = _ownerController.GetComponentsInChildren<Collider>();
        for (int i = 0; i < ownerColliders.Length; i++)
        {
            if (ownerColliders[i] != null && ownerColliders[i] != _col)
                Physics.IgnoreCollision(_col, ownerColliders[i], true);
        }
    }
}