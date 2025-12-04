using UnityEngine;
using UnityEngine.UI;
using TMPro; // <<< REQUIRED to use TextMeshProUGUI

public class PlayerStats : MonoBehaviour
{
    // --- Private Backing Fields ---
    private float _currentHealth;
    private float _currentStamina;
    private float _currentHunger;

    // --- Public Properties (Read-Only Access for other scripts) ---
    public float CurrentHealth => _currentHealth;
    public float CurrentStamina => _currentStamina;
    public float CurrentHunger => _currentHunger;

    // --- Maximum Values ---
    [Header("Stat Maxima")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;

    // --- Regeneration and Decay Rates (Units per Second) ---
    [Header("Rates & Costs")]
    public float healthRegenRate = 0f;
    public float staminaRegenRate = 10f;
    public float hungerDecayRate = 0.5f;
    [Tooltip("Health points lost per second when hunger is zero.")]
    public float starvationDamageRate = 5f;

    // --- Stamina Regeneration Control ---
    [Header("Stamina Control")]
    public float staminaRegenDelay = 1.0f; // Time delay before regen starts
    private float lastStaminaConsumptionTime;

    [HideInInspector] public bool isConsumingStamina = false; // Set by PlayerMovement
    [HideInInspector] public bool isJumping = false; // Set by PlayerMovement

    // --- UI References ---
    [Header("UI Bar & Text References (Required)")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;

    // TextMeshPro Labels (Connect these in the Inspector)
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI hungerText;


    private void Start()
    {
        // Initialize all stats to their maximum value
        _currentHealth = maxHealth;
        _currentStamina = maxStamina;
        _currentHunger = maxHunger;

        // Set the max value for the UI sliders
        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (staminaBar != null) staminaBar.maxValue = maxStamina;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        UpdateStatUI();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // --- Hunger Decay ---
        _currentHunger -= hungerDecayRate * deltaTime;
        _currentHunger = Mathf.Clamp(_currentHunger, 0f, maxHunger);

        // --- Stamina Regeneration ---
        bool canRegen = !isConsumingStamina && !isJumping;

        if (canRegen && _currentStamina < maxStamina)
        {
            // Check for the regeneration delay
            if (Time.time >= lastStaminaConsumptionTime + staminaRegenDelay)
            {
                _currentStamina += staminaRegenRate * deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0f, maxStamina);
            }
        }

        // --- Health Regeneration ---
        if (_currentHealth < maxHealth && healthRegenRate > 0)
        {
            _currentHealth += healthRegenRate * deltaTime;
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
        }

        // --- Starvation Damage ---
        if (_currentHunger <= 0)
        {
            DamageHealth(starvationDamageRate * deltaTime);
        }

        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        // Update Sliders based on private values
        if (healthBar != null) healthBar.value = _currentHealth;
        if (staminaBar != null) staminaBar.value = _currentStamina;
        if (hungerBar != null) hungerBar.value = _currentHunger;

        // Update Text Labels using the private values
        if (healthText != null) healthText.text = $"Health: {Mathf.Ceil(_currentHealth)} / {maxHealth}";
        if (staminaText != null) staminaText.text = $"Stamina: {Mathf.Ceil(_currentStamina)} / {maxStamina}";
        if (hungerText != null) hungerText.text = $"Hunger: {Mathf.Ceil(_currentHunger)} / {maxHunger}";
    }

    // --- Public Functions for Integration ---

    public void DamageHealth(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Debug.Log("Player has died!");
        }
    }

    public bool ConsumeStamina(float amount)
    {
        if (_currentStamina >= amount)
        {
            _currentStamina -= amount;
            lastStaminaConsumptionTime = Time.time;
            return true;
        }
        return false;
    }

    public void RestoreHunger(float amount)
    {
        _currentHunger += amount;
        _currentHunger = Mathf.Clamp(_currentHunger, 0f, maxHunger);
    }
}