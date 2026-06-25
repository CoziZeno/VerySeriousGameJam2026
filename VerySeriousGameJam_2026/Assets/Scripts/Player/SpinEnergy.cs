using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
[RequireComponent(typeof(SpinnerCombat))]
public class PlayerSpinEnergy : MonoBehaviour
{
    [Header("References")]
    public SpinnerController controller;
    public SpinnerCombat combat;
    public Image energyFillImage;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy = 0f;

    [Header("Drain")]
    public float moveDrainPerSecond = 1f;
    public float dashCost = 20f;
    public float attackCost = 30f;

    [Header("Gain")]
    public float hitReward = 10f;

    [Header("Focus")]
    public float focusFullChargeDuration = 5f;
    public GameObject focusVFX;
    public Vector3 focusVFXOffset = new Vector3(0f, 0.4f, 0f);

    [Header("Intro")]
    public float introFillDuration = 3f;

    public float EnergyPercent => currentEnergy / maxEnergy;

    Coroutine _introFillRoutine;
    bool _restorePlayerInputAfterIntro;
    bool _inputLockedForIntro;
    GameObject _activeFocusVFX;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<SpinnerController>();

        if (combat == null)
            combat = GetComponent<SpinnerCombat>();

        if (focusVFX != null)
            focusVFX.SetActive(false);
    }

    void OnEnable()
    {
        if (combat != null)
            combat.OnDealtDamage += HandleDamageDealt;
    }

    void OnDisable()
    {
        if (combat != null)
            combat.OnDealtDamage -= HandleDamageDealt;

        StopFocusVFX();
        RestorePlayerInputAfterIntro();
    }

    void Start()
    {
        currentEnergy = 0f;
        LockPlayerInputForIntro();
        _introFillRoutine = StartCoroutine(IntroFillRoutine());
    }

    IEnumerator IntroFillRoutine()
    {
        float timer = 0f;

        while (timer < introFillDuration)
        {
            timer += Time.deltaTime;

            currentEnergy = Mathf.Lerp(
                0f,
                maxEnergy,
                timer / introFillDuration);

            UpdateUI();

            yield return null;
        }

        currentEnergy = maxEnergy;
        UpdateUI();
        _introFillRoutine = null;
        RestorePlayerInputAfterIntro();
    }

    void Update()
    {
        UpdateFocus();
        DrainMovementEnergy();
        UpdateUI();
    }

    void UpdateFocus()
    {
        if (controller == null || controller.isEnemy || _introFillRoutine != null)
        {
            StopFocusVFX();
            return;
        }

        if (Input.GetMouseButton(1))
        {
            controller.SetMovementLocked(true);
            float chargePerSecond = maxEnergy / Mathf.Max(0.01f, focusFullChargeDuration);
            AddEnergy(chargePerSecond * Time.deltaTime);
            StartFocusVFX();
        }
        else
        {
            StopFocusVFX();
        }
    }

    void DrainMovementEnergy()
    {
        if (controller == null)
            return;

        if (controller.CurrentMoveInput.sqrMagnitude > 0.01f)
        {
            SpendEnergy(moveDrainPerSecond * Time.deltaTime);
        }
    }

    void HandleDamageDealt(SpinnerCombat target, int damage, bool aggressive)
    {
        AddEnergy(hitReward);
    }

    public bool HasEnoughEnergy(float amount)
    {
        return currentEnergy >= amount;
    }

    public bool SpendEnergy(float amount)
    {
        if (currentEnergy < amount)
            return false;

        StopIntroFill();
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        UpdateUI();

        return true;
    }

    public void DrainEnergy(float amount)
    {
        StopIntroFill();
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        UpdateUI();
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
    }

    public float GetMovementMultiplier()
    {
        float energy = currentEnergy;

        if (energy <= 80f)
        {
            return Mathf.Lerp(
                0.1f,
                1f,
                energy / 80f);
        }

        return Mathf.Lerp(
            1f,
            1.5f,
            (energy - 80f) / 20f);
    }

    public float GetSpinMultiplier()
    {
        return GetMovementMultiplier();
    }

    void UpdateUI()
    {
        if (energyFillImage == null)
            return;

        energyFillImage.fillAmount =
            currentEnergy / maxEnergy;
    }

    void StopIntroFill()
    {
        if (_introFillRoutine == null)
            return;

        StopCoroutine(_introFillRoutine);
        _introFillRoutine = null;
        RestorePlayerInputAfterIntro();
    }

    void LockPlayerInputForIntro()
    {
        if (controller == null || controller.isEnemy || introFillDuration <= 0f)
            return;

        _restorePlayerInputAfterIntro = controller.usePlayerInput;
        _inputLockedForIntro = true;
        controller.usePlayerInput = false;
    }

    void RestorePlayerInputAfterIntro()
    {
        if (!_inputLockedForIntro || controller == null || controller.isEnemy)
            return;

        controller.usePlayerInput = _restorePlayerInputAfterIntro;
        _inputLockedForIntro = false;
    }

    void StartFocusVFX()
    {
        if (focusVFX == null || _activeFocusVFX != null)
            return;

        _activeFocusVFX = focusVFX;
        _activeFocusVFX.transform.localPosition = focusVFXOffset;
        _activeFocusVFX.SetActive(true);

        ParticleSystem[] particles = _activeFocusVFX.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
                particles[i].Play(true);
        }
    }

    void StopFocusVFX()
    {
        if (controller != null)
            controller.SetMovementLocked(false);

        if (_activeFocusVFX == null)
            return;

        ParticleSystem[] particles = _activeFocusVFX.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        _activeFocusVFX.SetActive(false);
        _activeFocusVFX = null;
    }
}
