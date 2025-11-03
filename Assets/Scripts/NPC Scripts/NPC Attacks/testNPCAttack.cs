using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Basic test NPC attack used for basic testing of NPC scripts making use of attacks
// This version works very similar to the version in the prototype and is likely to work different for the actual thing with usage of animations etc.
public class TestNPCAttack : NpcAttack
{
    // Basic override of attack stats
    public override float attackTime => 0.3f;
    public override float attackCooldown => 3f;
    public override float attackRange => 2f;
    private BoxCollider attackCollider;


    // Attack fields necessary for this version of the attack logic
    private Coroutine attackRoutine;
    private GeneralNPC thisNPC;

    //  Attack logic setup to start when the script begins
    private void Awake()
    {
        attackCollider = GetComponentInChildren<BoxCollider>();
        
        // Ignore collisions between the attack hitbox and all colliders on the same NPC
        var ownerCols = transform.root.GetComponentsInChildren<Collider>(true);
        foreach (var c in ownerCols)
        {
            if (c && attackCollider && c != attackCollider)
                Physics.IgnoreCollision(attackCollider, c, true);
        }
    }

    // Method to trigger the attack - override of abstract method in parent class
    public override void TriggerAttack(NavMeshAgent agent, GameObject player)
    {
        //Debug.Log("Test NPC Attack Triggered!");
        // Implement attack logic here

        if (player != null)
        {
            transform.LookAt(player.transform);
        }

        if (attackRoutine == null)
        {
            attackActive = true;
            attackRoutine = StartCoroutine(AttackCycle(attackTime, attackCooldown));
        }
    }


    // Method to stop the attack - for switching modes or clearing the necessary info about the attack
    public override void StopAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        attackActive = false;
    }

    // Attack cycle coroutine to handle the timing of the attack
    private IEnumerator AttackCycle(float activeDuration, float interval)
    {
        float offDuration = Mathf.Max(0f, interval - activeDuration);
        
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
        
        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration);
        else
            yield return null;
        
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        StopAttack();
    }

    // OnTriggerEnter to handle hit detection for the attack
    private void OnTriggerEnter(Collider other)
    {
        // In the future the collider will be set up to only exist for the npc and player layers but right player takedamage doesn't exist yet so cannot call without if statements
        if (other.CompareTag("Player"))
        {
            Debug.Log("Test NPC Attack Hit: " + other.gameObject.name);
            // this will eventually call the player's TakeDamage method but currently this doesn't exist due to other person being assigned to the feature
        }
        if (other.CompareTag("npc"))
        {
            Debug.Log("Test NPC Attack Hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(2, DamageCause.EnemyAttack);
        }
    }
}
