using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public SpinnerController player;

    public void HealCard()
    {
        player.Heal(50);
    }
}