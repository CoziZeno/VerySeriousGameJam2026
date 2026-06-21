using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class SpinnerUpgradeManager : MonoBehaviour
{
    public SpinnerController controller;
    public SpinnerCombat combat;
    public Transform moduleRoot;

    readonly List<SpinnerUpgradeModule> _modules = new List<SpinnerUpgradeModule>();
    public System.Action<UpgradeData> OnUpgradeAdded;

    void Awake()
    {
        if (controller == null) controller = GetComponent<SpinnerController>();
        if (combat == null) combat = GetComponent<SpinnerCombat>();
        if (moduleRoot == null) moduleRoot = transform;
    }

    void Update()
    {
        for (int i = 0; i < _modules.Count; i++)
        {
            var module = _modules[i];
            if (module != null)
                module.TickModule(Time.deltaTime);
        }
    }

    public bool HasModule<T>() where T : SpinnerUpgradeModule
    {
        for (int i = 0; i < _modules.Count; i++)
        {
            if (_modules[i] is T)
                return true;
        }

        return false;
    }

    public SpinnerUpgradeModule AddUpgrade(UpgradeData data)
    {
        if (data == null || data.modulePrefab == null)
            return null;

        GameObject instance = Instantiate(data.modulePrefab, moduleRoot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        
        SpinnerUpgradeModule module = instance.GetComponent<SpinnerUpgradeModule>();
        if (module == null)
        {
            Debug.LogWarning($"Upgrade prefab '{data.name}' has no SpinnerUpgradeModule on it.");
            Destroy(instance);
            return null;
        }

        module.Initialize(controller, combat, this);
        _modules.Add(module);
        OnUpgradeAdded?.Invoke(data);
        return module;
    }

    public void RemoveModule(SpinnerUpgradeModule module)
    {
        if (module == null)
            return;

        if (_modules.Remove(module))
        {
            module.Cleanup();
            Destroy(module.gameObject);
        }
    }
}