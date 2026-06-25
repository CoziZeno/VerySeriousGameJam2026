using UnityEngine;

public class DoubleHealthUpgradeModule : SpinnerUpgradeModule
{
    public float healthMultiplier = 2f;
    public bool healAddedHealth = true;

    int _addedMaxHealth;

    protected override void OnEquipped()
    {
        if (controller == null)
            return;

        int newMaxHealth = Mathf.Max(1, Mathf.RoundToInt(controller.maxHealth * healthMultiplier));
        _addedMaxHealth = Mathf.Max(0, newMaxHealth - controller.maxHealth);

        if (_addedMaxHealth > 0)
            controller.AddMaxHealth(_addedMaxHealth, healAddedHealth);
    }

    protected override void OnUnequipped()
    {
        if (controller == null || _addedMaxHealth <= 0)
            return;

        controller.AddMaxHealth(-_addedMaxHealth, false);
        _addedMaxHealth = 0;
    }
}
