using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerCameraShakeEvents : MonoBehaviour
{
    [Header("Hit Shake")]
    public bool shakeWhenThisSpinnerGetsHit = true;
    public bool onlyIfPlayer = true;
    public float hitShakeStrength = 0.48f;
    public float hitShakeDuration = 0.26f;

    [Header("Elimination Shake")]
    public bool shakeWhenThisSpinnerDies = true;
    public bool onlyIfEnemy = true;
    public float eliminationShakeStrength = 5.0f;
    public float eliminationShakeDuration = 1.5f;

    SpinnerController _spinner;

    void Awake()
    {
        _spinner = GetComponent<SpinnerController>();
    }

    void OnEnable()
    {
        if (_spinner == null)
            _spinner = GetComponent<SpinnerController>();

        _spinner.OnHitTaken += HandleHitTaken;
        _spinner.OnEliminated += HandleEliminated;
    }

    void OnDisable()
    {
        if (_spinner == null)
            return;

        _spinner.OnHitTaken -= HandleHitTaken;
        _spinner.OnEliminated -= HandleEliminated;
    }

    void HandleHitTaken(SpinnerController spinner, int damage)
    {
        if (!shakeWhenThisSpinnerGetsHit)
            return;

        if (onlyIfPlayer && spinner.isEnemy)
            return;

        SpinnerCamera.Shake(hitShakeStrength, hitShakeDuration);
    }

    void HandleEliminated(SpinnerController spinner)
    {
        if (!shakeWhenThisSpinnerDies)
            return;

        if (onlyIfEnemy && !spinner.isEnemy)
            return;

        SpinnerCamera.Shake(eliminationShakeStrength, eliminationShakeDuration);
    }
}
