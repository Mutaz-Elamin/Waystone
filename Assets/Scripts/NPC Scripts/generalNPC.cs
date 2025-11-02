using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public enum npcType
    {
        Passive,
        Neutral,
        Enemy
    }

    // NPC type shouldn't be directly accessed outside of the npc script
    private npcType type;
    public npcType Type {get { return type; }}

    // Health and speed of the NPC will likely to be set in scene
    [SerializeField] private int health;
    [SerializeField] private float speed;




    // Method to switch movement settings/state machin
    public void checkMovementMode()
    {
        // State Machine logic for movement mode would go here
        // Each npc type will have different state machines
    }


    // Method to apply damage to NPCs based off attacks or other things
    public void takeDamage(int damage) {
        health -= damage;
        Debug.Log("NPC took " + damage + " damage, remaining health: " + health);

        if (health <= 0) {
            die();
        }
    }


    // Method to kill NPC (may be changed public if a feature to autokill npcs is added)
    private void die() {
        //Debug.Log("NPC has died.");

        dropResources();

        Destroy(gameObject);
    }


    // Method to set the resources that are dropped upon death and how this works
    // By default, die function will call this method but this may not be true for all npcs
    private void dropResources()
    {
        // Resource dropping logic
    }
}
