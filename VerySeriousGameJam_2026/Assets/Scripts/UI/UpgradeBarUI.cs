using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeBarUI : MonoBehaviour
{
    [Header("UI")]
    public UpgradeSlotUI slotPrefab;
    public Transform container;

    readonly List<UpgradeSlotUI> slots = new();

    public void AddUpgrade(UpgradeData data)
    {
        if (data == null)
            return;

        UpgradeSlotUI slot =
            Instantiate(slotPrefab, container);

        slot.Setup(data.icon);

        slots.Add(slot);
    }

    public void ClearBar()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        slots.Clear();
    }
}