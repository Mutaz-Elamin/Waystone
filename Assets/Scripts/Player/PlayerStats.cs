using UnityEngine;
using UnityEngine.UI;
using TMPro; 

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

    [Header("Defense Settings")]
    [Range(0f, 1f)]
    public float defendDamageMultiplier = 0.5f; // multiply damage while defending (50% if 0.5)

    [Header("Defend Stamina")]
    public bool defendUsesStamina = true;
    public float defendStaminaDrainPerSecond = 10f; // stamina drained while holding defend

    [HideInInspector] public bool isDefending = false;
    [HideInInspector] public Armor equippedArmor = null;


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

        if (isDefending && defendUsesStamina)
        {
            float drain = defendStaminaDrainPerSecond * deltaTime;
            _currentStamina = Mathf.Max(0f, _currentStamina - drain);

            // if stamina depleted, stop defending immediately
            if (_currentStamina <= 0f)
                isDefending = false;

            // reset regen timer so regen waits after defending
            lastStaminaConsumptionTime = Time.time;
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
        // defensive multiplier first (player actively blocking)
        if (isDefending)
            amount *= defendDamageMultiplier;

        // armor reduction (armorRating is percent 0..100)
        if (equippedArmor != null && equippedArmor.armorRating > 0)
        {
            float armorReduction = Mathf.Clamp01(equippedArmor.armorRating / 100f);
            amount *= (1f - armorReduction);
        }

        // apply
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);

        if (_currentHealth <= 0f)
        {
            Debug.Log("Player has died!");
            // TODO: call death/respawn logic if you have it
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

    public void StartDefend()
    {
        if (defendUsesStamina && _currentStamina <= 0f)
        {
            // can't defend if no stamina
            isDefending = false;
            return;
        }
        isDefending = true;
    }

    public void StopDefend()
    {
        isDefending = false;
    }

}