public class VampireModule : SpinnerUpgradeModule
{
    public override void Initialize(SpinnerController ctrl, SpinnerCombat cmbt, SpinnerUpgradeManager mgr)
    {
        base.Initialize(ctrl, cmbt, mgr);
        cmbt.OnDealtDamage += HandleDamageDealt;
    }

    void HandleDamageDealt(SpinnerCombat target, int amount, bool wasDash)
    {
        if (wasDash) controller.Heal(1); // Lifesteal on dashes!
    }
}