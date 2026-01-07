using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageCause
{
    PlayerAttack,
    PlayerPickaxe,
    PlayerAxe,
    EnemyAttack,
    Environment,
    Other,
    None
}

public class HealthBasedAsset : MonoBehaviour
{

    [SerializeField] private int startHealth;
    protected int StartHealth { get { return startHealth; } }
    protected int health;
    protected int Health { get { return health; } }

    protected virtual void Awake()
    {
        health = startHealth;
    }

    // Method to apply damage
    public virtual void TakeDamage(int damage, DamageCause cause)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }


    // Method to kill asset (may be changed public if a feature to autokill npcs is added)
    protected virtual void Die()
    {
        DropResources();

        Destroy(gameObject);
    }


    // Method to set the resources that are dropped upon death and how this works
    // By default, die function will call this method but this may not be true for all assets
    protected virtual void DropResources()
    {
        // Resource dropping logic
    }
}
