using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OgreNPC : EnemyNPC
{
    private Animator OgreAnimator;

    protected override void Awake()
    {
        base.Awake();
        OgreAnimator = GetComponent<Animator>();
    }

    protected override void WanderMovementScript()
    {
        if (!desPointSet)
        {
            OgreAnimator.SetBool("Walking", false);
        }
        else
        {
            OgreAnimator.SetBool("Walking", true);
        }
        base.WanderMovementScript();
    }

    protected override void ChasingMovementScript()
    {
        OgreAnimator.SetBool("Chasing", true);
        OgreAnimator.SetBool("Attacking", false);
        base.ChasingMovementScript();
    }

    protected override void AttackingMovementScript()
    {
        OgreAnimator.SetBool("Chasing", false);
        OgreAnimator.SetBool("Attacking", true);
        base.AttackingMovementScript();
    }

    protected override void SelectAttack()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > attacks[1].attackRangeMin && (Time.time - attacks[1].lastAttackTime > attacks[1].attackCooldown))
        {
            currentAttack = attacks[1];
            currentAttackRange = attackRanges[1];
        }
        else
        {
            currentAttack = attacks[0];
            currentAttackRange = attackRanges[0];
        }
    }
}
