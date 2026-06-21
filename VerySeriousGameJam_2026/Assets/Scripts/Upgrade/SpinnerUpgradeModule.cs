using UnityEngine;

public abstract class SpinnerUpgradeModule : MonoBehaviour
{
    protected SpinnerController controller;
    protected SpinnerCombat combat;
    protected SpinnerUpgradeManager manager;
    protected Rigidbody rb;

    public virtual void Initialize(SpinnerController ownerController, SpinnerCombat ownerCombat, SpinnerUpgradeManager ownerManager)
    {
        controller = ownerController;
        combat = ownerCombat;
        manager = ownerManager;
        rb = GetComponentInParent<Rigidbody>();
        OnEquipped();
    }

    protected virtual void OnEquipped() { }
    protected virtual void OnUnequipped() { }

    public virtual void TickModule(float deltaTime) { }

    public void Cleanup()
    {
        OnUnequipped();
    }
}