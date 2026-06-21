using UnityEngine;

public enum PowerupType
{
    SpeedMultiplier,
    DamageMultiplier,
    RadiusMultiplier,
    CooldownMultiplier,
    SpinMultiplier,
    ShieldCharge,
    Heal
}

[CreateAssetMenu(menuName = "Spinner Game/Powerup Data", fileName = "NewPowerupData")]
public class PowerupData : ScriptableObject
{
    public string displayName = "Powerup";
    public PowerupType type = PowerupType.SpeedMultiplier;

    [Tooltip("For multipliers, use values like 1.25, 1.5, 0.8. For Shield/Heal, use whole numbers.")]
    public float magnitude = 1.25f;

    [Tooltip("Duration in seconds. Use 0 for instant effects like ShieldCharge or Heal.")]
    public float duration = 5f;

    [TextArea]
    public string description;

    public Color uiColor = Color.white;
}