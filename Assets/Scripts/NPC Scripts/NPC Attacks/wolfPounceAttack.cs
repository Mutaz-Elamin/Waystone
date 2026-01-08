using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// Basic wolf NPC attack which is the primitive bite attack for the wolf NPC
// This attack largely exists to test how animations/attacks will work in the NPC system as well as for an NPC with more than one attack and how well this works
public class WolfPounceAttack : NpcAttack
{
    // Basic override of attack stats
    public override float attackTime => 0.575f;
    public override float attackCooldown => 3f;
    public override float attackRange => 14f;
    public override float attackRangeMin => 5f;


    private BoxCollider attackCollider;

    // Attack fields necessary for this version of the attack logic
    private Coroutine attackRoutine;
    private Animator wolfAnimator;
    private float probeUp = 3f;
    private float probeDown = 6f;
    private LayerMask groundMask = ~0;
    private float attackDistance = 8f;
    private float currentAttackDuration = 2.3f;
    private NavMeshAgent agent;
    private Vector3 startPos;

    //  Attack logic setup to start when the script begins
    private void Awake()
    {
        attackCollider = GetComponentInChildren<BoxCollider>();
        wolfAnimator = transform.parent.parent.parent.parent.parent.parent.GetComponent<Animator>();

        // Ignore collisions between the attack hitbox and all colliders on the same NPC
        var ownerCols = transform.parent.parent.parent.parent.parent.parent.GetComponentsInChildren<Collider>(true);
        foreach (var c in ownerCols)
        {
            if (c && attackCollider && c != attackCollider)
                Physics.IgnoreCollision(attackCollider, c, true);
        }
    }

    // Method to trigger the attack - override of abstract method in parent class
    public override void TriggerAttack(NavMeshAgent agent, GameObject player)
    {
        this.agent = agent;

        if (attackRoutine == null)
        {
            attackActive = true;
            attackRoutine = StartCoroutine(AttackCycle(agent, player, attackTime));
        }
    }


    // Method to stop the attack - for switching modes or clearing the necessary info about the attack
    public override void StopAttack()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 0.5f, agent.areaMask))
            agent.Warp(hit.position);
        else
            agent.Warp(startPos);
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.ResetPath();

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
        wolfAnimator.SetBool("Pounce", false);
    }

    // Attack cycle coroutine to handle the timing of the attack
    private IEnumerator AttackCycle(NavMeshAgent agent, GameObject player, float activeDuration)
    {
        yield return new WaitForSeconds(0.2f);
        float multiplier = 1f;
        attackDistance = Vector3.Distance(transform.position, player.transform.position);
        attackDistance = Vector3.Magnitude(new Vector3(transform.position.x - player.transform.position.x, 0f, transform.position.z - player.transform.position.z));
        if (attackDistance < attackRange)
        {
            multiplier = attackDistance/attackRange;
        }
        currentAttackDuration = activeDuration * multiplier;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        startPos = transform.parent.parent.parent.parent.parent.parent.position;
        Vector3 endPos = GetFrontGroundPoint(player.transform, 2.4f, agent);
        if (multiplier == 1f)
        {
            Vector3 normalizedVector = (endPos - startPos).normalized;
            endPos = startPos + normalizedVector * attackRange;
        }

        if (player != null)
        {
            transform.parent.parent.parent.parent.parent.parent.LookAt(player.transform);
        }

        wolfAnimator.SetFloat("Pounce Speed", 2f * (1/multiplier));

        wolfAnimator.SetBool("Pounce", true);
        if (currentAttackDuration > 0f)
            yield return new WaitForSeconds(currentAttackDuration * 0.075f);
        else
            yield return null;
        wolfAnimator.SetFloat("Pounce Speed", 0.1f * (1/multiplier));
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
        Vector3 midPosition = (startPos + endPos) / 2f + Vector3.up * 1.5f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (activeDuration/2);
            
            transform.parent.parent.parent.parent.parent.parent.position = Vector3.Lerp(startPos, midPosition, t);
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (activeDuration/2);
            transform.parent.parent.parent.parent.parent.parent.position = Vector3.Lerp(midPosition, endPos, t);
            yield return null;
        }
        wolfAnimator.SetFloat("Pounce Speed", 3f * (1/multiplier));
        if (currentAttackDuration > 0f)
            yield return new WaitForSeconds(currentAttackDuration * 0.1f);
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
            other.GetComponent<GeneralNPC>()?.TakeDamage(this.attackDamage, DamageCause.EnemyAttack);
        }
    }

    private Vector3 GetFrontGroundPoint(Transform target, float stopDistanceFront, NavMeshAgent agent)
    {
        Vector3 direction = transform.parent.parent.parent.parent.parent.parent.position - target.position;

        direction.Normalize();

        Vector3 raw = target.position + direction * stopDistanceFront;

        Vector3 targetPos = raw;
        if (Physics.Raycast(raw, Vector3.down, out RaycastHit hitInfo, probeUp + probeDown, groundMask, QueryTriggerInteraction.Ignore))
        {
            targetPos = hitInfo.point;
        }
        else
        {
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit nHit, 2f, agent ? agent.areaMask : NavMesh.AllAreas))
            {
                targetPos = nHit.position;
            }
        }

        return targetPos;
    }

}
