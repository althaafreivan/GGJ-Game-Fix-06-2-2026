using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;
using UnityEngine;

public class StaminaListener : MonoBehaviour
{
    void Start()
    {
        Player.onPlayerChange += ListenTo;   
    }

    private void ListenTo(Player player)
    {
        player.GetComponent<M_Stamina>().onStaminaChange.AddListener(changeStamina);
    }

    private void changeStamina(float val)
    {

    }
}
