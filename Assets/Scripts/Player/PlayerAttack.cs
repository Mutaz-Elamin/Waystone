using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;      
    public SwordHitbox swordHitbox; 

    [Header("Attack Cooldowns")]
    public float lightCooldown = 0.3f;
    public float heavyCooldown = 0.7f;

    private float lastLightTime = -999f;
    private float lastHeavyTime = -999f;

    public void LightAttack()
    {
        if (Time.time - lastLightTime < lightCooldown) return;

        lastLightTime = Time.time;

        animator.SetTrigger("LightAttack");
        swordHitbox.canHit = true; 
    }

    public void HeavyAttack()
    {
        if (Time.time - lastHeavyTime < heavyCooldown) return;

        lastHeavyTime = Time.time;

        animator.SetTrigger("HeavyAttack");
        swordHitbox.canHit = true;
    }
}