using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // This makes it so other scripts can find "PlayerStats.Instance" easily
    public static PlayerStats Instance { get; private set; }

    [Header("Max Limits")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float maxStamina = 100f;
    [SerializeField] protected float maxHunger = 100f;

    [Header("Speeds")]
    [SerializeField] protected float hungerLoseSpeed = 0.5f; 
    [SerializeField] protected float staminaRegenSpeed = 10f; 
    [SerializeField] protected float staminaRegenSpeed = 10f;

    [SerializeField] private float minHungerMultiplier = 1f;   
    [SerializeField] private float maxHungerMultiplier = 1.6f;

    // Internal numbers (hidden from the Inspector to keep it clean)
    private float _currentHealth;
    private float _currentStamina;
    private float _currentHunger;
    private bool isDead;

    // These allow other scripts to SEEE the values, but not change them directly
    public float Health => _currentHealth;
    public float MaxHealth => maxHealth;
    public float Stamina => _currentStamina;
    public float MaxStamina => maxStamina;
    public float Hunger => _currentHunger;
    public float MaxHunger => maxHunger;

    private void Awake()
    {
        // Setup the "shortcut" to this script
        if (Instance == null) Instance = this;

        // Start everything at full capacity
        _currentHealth = maxHealth;
        _currentStamina = maxStamina;
        _currentHunger = maxHunger;
    }

    private void Update()
    {
        // 1. Lose hunger over time
        if (_currentHunger > 0)
        {
            float staminaPercent = _currentStamina / maxStamina;

            float hungerMultiplier = Mathf.Lerp(
                maxHungerMultiplier,
                minHungerMultiplier,
                staminaPercent
            );

            _currentHunger -= hungerLoseSpeed * hungerMultiplier * Time.deltaTime;
            _currentHealth -= healthLoseSpeed * Time.deltaTime;
        }
        else
        {
            TakeDamage(1f * Time.deltaTime);
        }

        // 2. Refill stamina over time
        if (_currentStamina < maxStamina)
        {
            _currentStamina += staminaRegenSpeed * Time.deltaTime;
        }



    }

    // Call this for spikes, falls, or enemies
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_currentHealth <= 0)
        {
            isDead = true;
            DeathManager.Instance.HandleDeath();
        }
    }

    // Call this for medkits or potions
    public void Heal(float amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, maxHealth);
    }

    // Call this when the player jumps or sprints
    public void UseStamina(float amount)
    {
        _currentStamina = Mathf.Clamp(_currentStamina - amount, 0, maxStamina);
    }

    // Call this for food items
    public void Eat(float amount)
    {
        _currentHunger = Mathf.Clamp(_currentHunger + amount, 0, maxHunger);
    }

    public void setHungerSpeed(float amount)
    {
        hungerLoseSpeed = amount;
    }

    public float HungerLoseSpeed
    {
        get { return hungerLoseSpeed; }
    }

    public void setHealthLoseSpeed(float amount)
    {
        healthLoseSpeed = amount;
    }

    public float HealthLoseSpeed() { return healthLoseSpeed; }
}  