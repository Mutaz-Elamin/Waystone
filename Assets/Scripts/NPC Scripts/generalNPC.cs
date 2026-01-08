using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcType
{
    Passive,
    Neutral,
    Enemy
}

public class GeneralNPC : HealthBasedAsset
{
    // Health and speed of the NPC will likely to be set in scene
    [SerializeField] protected float speed;


    // Awake is called when the script instance is being loaded
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("NPC created with health: " + health);
    }

    // Method to switch movement settings/state machin
    public virtual void CheckMovementMode()
    {
        // State Machine logic for movement mode would go here
        // Each npc type will have different state machines
    }

    public override void TakeDamage(int damage, DamageCause cause)
    {
        NpcHitFeedback fx = GetComponentInChildren<NpcHitFeedback>(true);
        if (fx != null) fx.Play();


        base.TakeDamage(damage, cause);
    }
}
