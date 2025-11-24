using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float currentHealth;
    public float currentStamina;
    public float currentHunger;

    [Header("Stat Maxima")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;

    [Header("Rates (Units per Second)")]
    public float healthRegenRate = 0f;
    public float staminaRegenRate = 10f;
    public float hungerDecayRate = 1f;

    [Header("UI Bar References")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (staminaBar != null) staminaBar.maxValue = maxStamina;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        UpdateStatUI();
    }

    private void Update()
    {
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        if (currentHunger <= 0)
        {
            DamageHealth(5f * Time.deltaTime);
        }

        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        if (healthBar != null) healthBar.value = currentHealth;
        if (staminaBar != null) staminaBar.value = currentStamina;
        if (hungerBar != null) hungerBar.value = currentHunger;
    }

    public void DamageHealth(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player has died!");
        }
        UpdateStatUI();
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            UpdateStatUI();
            return true;
        }
        return false;
    }

    public void RestoreHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        UpdateStatUI();
    }
}