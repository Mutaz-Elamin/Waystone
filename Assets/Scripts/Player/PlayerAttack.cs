using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Collider attackCollider;

    [Header("Combo Settings")]
    public float comboResetTime = 1.0f; 
    private int comboStep = 0;           
    private bool canAttack = true;

    private float lastAttackTime;

    public void LightAttack()
    {
        if (!canAttack) return;

        float timeSinceLast = Time.time - lastAttackTime;

        // Reset combo if too slow
        if (timeSinceLast > comboResetTime)
        {
            comboStep = 0;
        }

        comboStep++;

        if (comboStep == 1)
        {
            ResetAllLightAttackTriggers();
            animator.SetTrigger("LightAttack");
        }
        else if (comboStep == 2)
        {
            ResetAllLightAttackTriggers();
            animator.SetTrigger("LightAttack2");
        }
        else if (comboStep == 3)
        {
            ResetAllLightAttackTriggers();
            animator.SetTrigger("LightAttack3");
        }
        else
        {
            comboStep = 1;
            ResetAllLightAttackTriggers();
            animator.SetTrigger("LightAttack");
        }

        StartCoroutine(AttackWindow());

        lastAttackTime = Time.time;
    }

    IEnumerator AttackWindow()
    {
        canAttack = false;

        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.25f);  // active frames

        attackCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);
        canAttack = true;
    }
    void ResetAllLightAttackTriggers()
    {
        animator.ResetTrigger("LightAttack");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
    }
}


    //public void HeavyAttack()

    // { if (Time.time - lastHeavyTime < heavyCooldown) return;

    //lastHeavyTime = Time.time;

    //  animator.SetTrigger("HeavyAttack");
    //    swordHitbox.canHit = true;}



