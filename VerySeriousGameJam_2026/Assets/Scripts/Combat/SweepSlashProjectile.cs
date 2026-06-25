using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SweepSlashProjectile : MonoBehaviour
{
    Rigidbody _rb;
    Collider _col;

    SpinnerCombat _ownerCombat;
    Vector3 _direction;
    float _speed;
    float _range;
    int _damage;
    float _knockbackForce;
    float _stunDuration;
    LayerMask _targetMask;
    Vector3 _startPosition;
    bool _initialized;

    readonly HashSet<SpinnerCombat> _damagedTargets = new HashSet<SpinnerCombat>();

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _col.isTrigger = true;
    }

    public void Initialize(
        SpinnerCombat ownerCombat,
        Vector3 direction,
        float speed,
        float range,
        int damage,
        float knockbackForce,
        float stunDuration,
        LayerMask targetMask)
    {
        _ownerCombat = ownerCombat;
        _direction = direction.normalized;
        _speed = speed;
        _range = range;
        _damage = damage;
        _knockbackForce = knockbackForce;
        _stunDuration = stunDuration;
        _targetMask = targetMask;
        _startPosition = transform.position;
        _initialized = true;

        transform.forward = _direction;
        _rb.linearVelocity = _direction * _speed;

        float lifetime = Mathf.Max(0.2f, _range / Mathf.Max(0.01f, _speed) + 0.15f);
        Destroy(gameObject, lifetime);

        IgnoreOwnerCollisions();
    }

    void FixedUpdate()
    {
        if (!_initialized)
            return;

        _rb.linearVelocity = _direction * _speed;

        if (Vector3.Distance(_startPosition, transform.position) >= _range)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_initialized)
            return;

        SpinnerCombat target = other.GetComponentInParent<SpinnerCombat>();

        if (target != null)
        {
            if (_damagedTargets.Contains(target))
                return;

            if (!LayerIsInMask(other.gameObject.layer, _targetMask) &&
                !LayerIsInMask(target.gameObject.layer, _targetMask))
                return;

            _damagedTargets.Add(target);
            _ownerCombat.TryDamageSweepTarget(
                target,
                _damage,
                _direction,
                _knockbackForce,
                _stunDuration);

            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }

    void IgnoreOwnerCollisions()
    {
        if (_ownerCombat == null || _ownerCombat.controller == null)
            return;

        Collider[] ownerColliders = _ownerCombat.controller.GetComponentsInChildren<Collider>();
        for (int i = 0; i < ownerColliders.Length; i++)
        {
            if (ownerColliders[i] != null && ownerColliders[i] != _col)
                Physics.IgnoreCollision(_col, ownerColliders[i], true);
        }
    }

    bool LayerIsInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
