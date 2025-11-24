using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    // --- Current Values ---
    public float currentHealth;
    public float currentStamina;
    public float currentHunger;

    // --- Maximum Values ---
    [Header("Stat Maxima")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;

    // --- Regeneration and Decay Rates (Units per Second) ---
    [Header("Rates (Units per Second)")]
    public float healthRegenRate = 0f;
    public float staminaRegenRate = 10f;
    public float hungerDecayRate = 0.5f; // Decreased decay rate for testing

    // --- UI References ---
    [Header("UI Bar References")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;

    private void Start()
    {
        // Initialize all stats to their maximum value
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;

        // Set the max value for the UI sliders
        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (staminaBar != null) staminaBar.maxValue = maxStamina;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        // Initial UI update
        UpdateStatUI();
    }

    private void Update()
    {
        // --- Hunger Decay ---
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        // --- Stamina Regeneration ---
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        // --- Health Regeneration ---
        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        // --- Starvation Damage ---
        if (currentHunger <= 0)
        {
            DamageHealth(2f * Time.deltaTime); // Reduced starvation damage rate
        }

        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        if (healthBar != null) healthBar.value = currentHealth;
        if (staminaBar != null) staminaBar.value = currentStamina;
        if (hungerBar != null) hungerBar.value = currentHunger;
    }

    // --- Public Functions for Integration ---

    public void DamageHealth(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player has died!");
            // TODO: Add actual death/respawn logic here
        }
        UpdateStatUI();
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            // The UI update is called every frame in Update(), but we call it here for immediate feedback too.
            UpdateStatUI();
            return true;
        }
        return false; // Failed to consume stamina
    }

    public void RestoreHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        UpdateStatUI();
    }
}