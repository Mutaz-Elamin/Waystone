using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAttack : MonoBehaviour
{
    protected float attackDamage;
    protected float attackTime;
    protected float attackCooldown;
    public float AttackCooldown { get { return attackCooldown; } }
    protected float lastAttackTime;
    public float LastAttackTime
    {
        set { lastAttackTime = value; }
        get { return lastAttackTime; }
    }
    protected float attackRange;
    public float AttackRange { get { return attackRange; } }

    public virtual void TriggerAttack()
    {
        // Implementation of attack behavior
    }
}
