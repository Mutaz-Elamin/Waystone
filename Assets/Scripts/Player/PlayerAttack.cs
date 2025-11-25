using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour

{
    [Header("References")]
    public Transform attackOrigin; 
    public float attackRange = 1.8f;
    public float attackRadius = 0.4f;
    public LayerMask enemyLayers;

    [Header("Attack Timings")]
    public float lightAttackCooldown = 0.3f;
    public float heavyAttackCooldown = 0.7f;

    private float lastLightAttackTime = -999f;
    private float lastHeavyAttackTime = -999f;

    public void LightAttack()
    {
        if (Time.time - lastLightAttackTime < lightAttackCooldown)
            return;

        lastLightAttackTime = Time.time;
        DoAttack(1);
    }

    public void HeavyAttack()
    {
        if (Time.time - lastHeavyAttackTime < heavyAttackCooldown)
            return;

        lastHeavyAttackTime = Time.time;
        DoAttack(3); 
    }

    void DoAttack(int damage)
    {
        Debug.Log("ATTACK! Damage = " + damage);

        Collider[] hits = Physics.OverlapSphere(
            attackOrigin.position,
            attackRadius,
            enemyLayers
        );

        foreach (Collider hit in hits)
        {
            Debug.Log("Hit enemy: " + hit.name);
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
    }
}
