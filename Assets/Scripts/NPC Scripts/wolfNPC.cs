using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfNPC : EnemyNPC
{
    private Animator WolfAnimator;

    protected override void Awake()
    {
        base.Awake();
        WolfAnimator = GetComponent<Animator>();
    }

    protected override void WanderMovementScript()
    {
        WolfAnimator.SetBool("Chasing", false);
        if (!desPointSet)
        {
            WolfAnimator.SetBool("Walking", false);
        }
        else
        {
            WolfAnimator.SetBool("Walking", true);
        }
        base.WanderMovementScript();
    }

    protected override void ChasingMovementScript()
    {
        WolfAnimator.SetBool("Chasing", true);
        WolfAnimator.SetBool("Attacking", false);
        base.ChasingMovementScript();
    }

    protected override void AttackingMovementScript()
    {
        WolfAnimator.SetBool("Chasing", false);
        WolfAnimator.SetBool("Walking", false);
        WolfAnimator.SetBool("Attacking", true);
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
