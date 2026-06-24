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

    [Header("Intro")]
    public float introFillDuration = 3f;

    public float EnergyPercent => currentEnergy / maxEnergy;

    Coroutine _introFillRoutine;
    bool _restorePlayerInputAfterIntro;
    bool _inputLockedForIntro;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<SpinnerController>();

        if (combat == null)
            combat = GetComponent<SpinnerCombat>();
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
        DrainMovementEnergy();
        UpdateUI();
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
}
