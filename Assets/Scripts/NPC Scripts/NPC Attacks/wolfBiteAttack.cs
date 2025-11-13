using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Basic wolf NPC attack which is the primitive bite attack for the wolf NPC
// This attack largely exists to test how animations/attacks will work in the NPC system as well as for an NPC with more than one attack and how well this works
public class WolfBiteAttack : NpcAttack
{
    // Basic override of attack stats
    public override float attackTime => 0.6f;
    public override float attackCooldown => 1.2f;
    public override float attackRange => 1.7f;


    private BoxCollider attackCollider;

    // Attack fields necessary for this version of the attack logic
    private Coroutine attackRoutine;
    private Animator wolfAnimator;

    //  Attack logic setup to start when the script begins
    private void Awake()
    {
        attackCollider = GetComponentInChildren<BoxCollider>();
        wolfAnimator = transform.root.GetComponent<Animator>();

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

        if (attackRoutine == null)
        {
            attackActive = true;
            attackRoutine = StartCoroutine(AttackCycle(player, attackTime));
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
        wolfAnimator.SetBool("Bite", false);
    }

    // Attack cycle coroutine to handle the timing of the attack
    private IEnumerator AttackCycle(GameObject player, float activeDuration)
    {
        if (player != null)
        {
            transform.root.LookAt(player.transform);
        }

        wolfAnimator.SetBool("Bite", true);
        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.7f);
        else
            yield return null;
        if (attackCollider != null)
        {
            wolfAnimator.SetBool("Bite", true);
            attackCollider.enabled = true;
        }

        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.3f);
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
