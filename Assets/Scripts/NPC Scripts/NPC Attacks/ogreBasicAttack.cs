using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OgreBasicAttack : NpcAttack
{
    // Basic override of attack stats
    public override float attackTime => 3f;
    public override float attackCooldown => 4f;
    public override float attackRange => 9f;


    private BoxCollider attackCollider;

    // Attack fields necessary for this version of the attack logic
    private Coroutine attackRoutine;
    private Animator ogreAnimator;

    //  Attack logic setup to start when the script begins
    private void Awake()
    {
        attackCollider = GetComponentInChildren<BoxCollider>();
        ogreAnimator = transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetComponent<Animator>();

        // Ignore collisions between the attack hitbox and all colliders on the same NPC
        var ownerCols = transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetComponentsInChildren<Collider>(true);
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
        ogreAnimator.SetBool("BasicAttack", false);
        ogreAnimator.SetFloat("BasicAttackSpeed", 1f);
    }

    // Attack cycle coroutine to handle the timing of the attack
    private IEnumerator AttackCycle(GameObject player, float activeDuration)
    {
        if (player != null)
        {
            transform.parent.parent.parent.parent.parent.parent.parent.parent.parent.LookAt(player.transform);
        }

        ogreAnimator.SetBool("BasicAttack", true);
        ogreAnimator.SetFloat("BasicAttackSpeed", 5f);
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.175f);
        else
            yield return null;

        ogreAnimator.SetFloat("BasicAttackSpeed", 1f);
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.825f);
        else
            yield return null;
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
