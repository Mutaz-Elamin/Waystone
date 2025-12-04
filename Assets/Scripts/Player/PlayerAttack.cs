using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Collider attackCollider;

    [Header("Combo Settings")]
    public float comboResetTime = 1f;
    private int comboStep = 0;
    private bool canAttack = true;

    private float lastAttackTime;

    // Flags
    private bool isDefending = false;

    // --- LIGHT ATTACKS ---
    public void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;

        ResetAllAttackTriggers();

        if (comboStep == 1) animator.SetTrigger("LightAttack");
        else if (comboStep == 2) animator.SetTrigger("LightAttack2");
        else if (comboStep == 3) animator.SetTrigger("LightAttack3");
        else
        {
            comboStep = 1;
            animator.SetTrigger("LightAttack");
        }

        StartCoroutine(AttackWindow());
        lastAttackTime = Time.time;
    }

    private IEnumerator AttackWindow()
    {
        canAttack = false;

        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.25f); // active frames
        attackCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);
        canAttack = true;
    }

    // --- HEAVY ATTACK ---
    public void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;

        ResetAllAttackTriggers();
        animator.ResetTrigger("HeavyRelease");
        animator.SetTrigger("HeavyWindup");
        animator.SetBool("IsChargingHeavy", true);
    }

    public void ReleaseHeavyAttack()
    {
        if (isDefending) return;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");

        StartCoroutine(HeavyAttackWindow());
    }

    private IEnumerator HeavyAttackWindow()
    {
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.3f);
        attackCollider.enabled = false;

        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }

    // --- DEFEND ---
    public void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
    }

    public void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }


    private void ResetAllAttackTriggers()
    {
        animator.ResetTrigger("LightAttack");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");
    }
}





