using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum npcType
{
    Passive,
    Neutral,
    Enemy
}

public class GeneralNPC : MonoBehaviour
{
    // Health and speed of the NPC will likely to be set in scene
    [SerializeField] private int health;
    [SerializeField] protected float speed;




    // Method to switch movement settings/state machin
    public virtual void CheckMovementMode()
    {
        // State Machine logic for movement mode would go here
        // Each npc type will have different state machines
    }


    // Method to apply damage to NPCs based off attacks or other things
    public virtual void TakeDamage(int damage) {
        health -= damage;
        Debug.Log("NPC took " + damage + " damage, remaining health: " + health);

        if (health <= 0) {
            Die();
        }
    }


    // Method to kill NPC (may be changed public if a feature to autokill npcs is added)
    private void Die() {
        //Debug.Log("NPC has died.");

        DropResources();

        Destroy(gameObject);
    }


    // Method to set the resources that are dropped upon death and how this works
    // By default, die function will call this method but this may not be true for all npcs
    private void DropResources()
    {
        // Resource dropping logic
    }
}
