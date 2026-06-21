using UnityEngine;
using UnityEngine.UI;

public class UpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void Setup(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
