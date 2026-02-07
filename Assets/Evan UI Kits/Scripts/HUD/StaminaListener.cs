using EvanGameKits.Entity;
using EvanGameKits.Entity.Module;
using EvanUIKits.Sliders;
using UnityEngine;

public class StaminaListener : MonoBehaviour
{
    private AdvancedSlider slider;
    private M_Stamina stamina;

    void Start()
    {
        slider = GetComponent<AdvancedSlider>();
        Player.onPlayerChange += ListenTo;

        if (Player.ActivePlayer != null)
        {
            ListenTo(Player.ActivePlayer);
        }
    }

    private void OnDestroy()
    {
        Player.onPlayerChange -= ListenTo;
        if (stamina != null)
        {
            stamina.onStaminaChange?.RemoveListener(changeStamina);
        }
    }

    private void ListenTo(Player player)
    {
        if (stamina != null)
        {
            stamina.onStaminaChange?.RemoveListener(changeStamina);
        }

        if (player != null)
        {
            stamina = player.GetComponent<M_Stamina>();
            if (stamina != null)
            {
                stamina.onStaminaChange?.AddListener(changeStamina);
            }
        }
        else
        {
            stamina = null;
        }
    }

    private void changeStamina(float val)
    {
        if (stamina != null && slider != null)
        {
            slider.SetValue(val / stamina.maxStamina);
        }
    }
}
