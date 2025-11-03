using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public abstract class NpcAttack : MonoBehaviour
{
    // Basic attack fields that get changed by npc script
    public bool attackActive = false;
    public float lastAttackTime = Mathf.NegativeInfinity;

    // These values can be overridden by child classes
    // The child classes must have these properties implemented but can change the stats depending on the attack
    protected virtual float attackDamage => 10f;
    public virtual float attackTime => 1f;
    public virtual float attackCooldown => 3f;
    public virtual float attackRange => 2f;


    // Method to trigger the attack - abstract as all npc attacks will be different and will need to implement their own logic
    public abstract void TriggerAttack(NavMeshAgent agent, GameObject player);

    // Method to stop the attack - abstract as all npc attacks will be different and will need to implement their own logic
    public abstract void StopAttack();
}
