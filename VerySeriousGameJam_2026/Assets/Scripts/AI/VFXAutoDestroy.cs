using UnityEngine;

public class VFXAutoDestroy : MonoBehaviour
{
    public float fallbackLifetime = 1.5f;
    public float extraLifetime = 0.25f;

    void Start()
    {
        Destroy(gameObject, GetLifetime());
    }

    float GetLifetime()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        float lifetime = 0f;

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle == null)
                continue;

            ParticleSystem.MainModule main = particle.main;
            float duration = main.duration;
            float startLifetime = GetMaxCurveValue(main.startLifetime);
            float startDelay = GetMaxCurveValue(main.startDelay);

            lifetime = Mathf.Max(lifetime, duration + startLifetime + startDelay);
        }

        if (lifetime <= 0f)
            lifetime = fallbackLifetime;

        return lifetime + extraLifetime;
    }

    float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.Curve:
                return GetMaxAnimationCurveValue(curve.curve);
            case ParticleSystemCurveMode.TwoCurves:
                return GetMaxAnimationCurveValue(curve.curveMax);
            default:
                return 0f;
        }
    }

    float GetMaxAnimationCurveValue(AnimationCurve curve)
    {
        if (curve == null || curve.length == 0)
            return 0f;

        float maxValue = 0f;
        Keyframe[] keys = curve.keys;

        for (int i = 0; i < keys.Length; i++)
            maxValue = Mathf.Max(maxValue, keys[i].value);

        return maxValue;
    }
}
