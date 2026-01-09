using UnityEngine;
using UnityEngine.UI;

public class SurvivalUI : MonoBehaviour
{

    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;

    void Update()
    {

        if (PlayerStats.Instance != null)
        {
            // Update the bars based on percentage (Current divided by Max)
            healthBar.value = PlayerStats.Instance.Health / PlayerStats.Instance.MaxHealth;
            staminaBar.value = PlayerStats.Instance.Stamina / PlayerStats.Instance.MaxStamina;
            hungerBar.value = PlayerStats.Instance.Hunger / PlayerStats.Instance.MaxHunger;
        }
    }
}