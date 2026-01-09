using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OgreKickAttack : NpcAttack
{
    // Basic override of attack stats
    public override float attackTime => 2.7375f;
    public override float attackCooldown => 7f;
    public override float attackRange => 15f;
    public override float attackRangeMin => 8f;


    private BoxCollider attackCollider;

    // Attack fields necessary for this version of the attack logic
    private Coroutine attackRoutine;
    private Animator ogreAnimator;

    //  Attack logic setup to start when the script begins
    private void Awake()
    {
        attackCollider = GetComponentInChildren<BoxCollider>();
        ogreAnimator = transform.parent.parent.parent.parent.parent.GetComponent<Animator>();

        // Ignore collisions between the attack hitbox and all colliders on the same NPC
        var ownerCols = transform.parent.parent.parent.parent.parent.GetComponentsInChildren<Collider>(true);
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
        ogreAnimator.SetBool("KickAttack", false);
        ogreAnimator.SetFloat("KickAttackSpeed", 1f);
    }

    // Attack cycle coroutine to handle the timing of the attack
    private IEnumerator AttackCycle(GameObject player, float activeDuration)
    {
        if (player != null)
        {
            transform.parent.parent.parent.parent.parent.LookAt(player.transform);
        }

        ogreAnimator.SetBool("KickAttack", true);
        ogreAnimator.SetFloat("KickAttackSpeed", 1f);

        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.28571f);
        else
            yield return null;


        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        ogreAnimator.SetFloat("KickAttackSpeed", 2f);
        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.14286f);
        else
            yield return null;

        ogreAnimator.SetFloat("KickAttackSpeed", 1f);
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        if (activeDuration > 0f)
            yield return new WaitForSeconds(activeDuration * 0.57143f);
        else
            yield return null;
        StopAttack();
    }

    // OnTriggerEnter to handle hit detection for the attack
    private void OnTriggerEnter(Collider other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(30);
        }

        HealthBasedAsset asset = other.GetComponent<HealthBasedAsset>();
        if (asset != null)
        {
            asset.TakeDamage(200, DamageCause.EnemyAttack);
            return;
        }
    }
}
