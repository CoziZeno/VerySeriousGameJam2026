using UnityEngine;

[CreateAssetMenu(menuName = "Spinner Game/Upgrade Data", fileName = "NewUpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string displayName = "Upgrade";

    [TextArea]
    public string description;

    public Sprite icon;

    public GameObject modulePrefab;
}