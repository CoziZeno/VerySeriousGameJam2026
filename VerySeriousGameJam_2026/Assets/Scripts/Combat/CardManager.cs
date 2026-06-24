using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public SpinnerController player;

    [Header("VFX")]
    public GameObject healVFX;

    [Header("Healing")]
    public int healAmount = 50;

    public void HealCard()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing!");
            return;
        }

        // Heal player
        player.Heal(healAmount);

        // Spawn heal VFX
        if (healVFX != null)
        {
            GameObject fx = Instantiate(
                healVFX,
                player.transform.position,
                Quaternion.identity
            );

            // Follow player while effect is active
            fx.transform.SetParent(player.transform);
        }
    }
}