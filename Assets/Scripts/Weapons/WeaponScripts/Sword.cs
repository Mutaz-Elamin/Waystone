using System.Collections;
using UnityEngine;

public class Sword : Weapon
{
    [Header("Sword Settings")]
    public float comboResetTime = 1f;

    private int comboStep = 0;
    private bool canAttack = true;
    private float lastAttackTime;
    private bool isDefending = false;

    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        if (comboStep == 1) animator.SetTrigger("LightAttack");
        else if (comboStep == 2) animator.SetTrigger("LightAttack2");
        else if (comboStep == 3) animator.SetTrigger("LightAttack3");
        else { comboStep = 1; animator.SetTrigger("LightAttack"); }

        StartCoroutine(AttackWindow(0.25f));
        lastAttackTime = Time.time;
    }

    public override void HeavyAttack()
    {
        if (!canAttack || isDefending) return;
        animator.SetTrigger("HeavyWindup");
        StartCoroutine(HeavyAttackWindow());
    }

    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.ResetTrigger("HeavyRelease");
        animator.SetTrigger("HeavyWindup");
        animator.SetBool("IsChargingHeavy", true);
    }

    public override void ReleaseHeavyAttack()
    {
        if (isDefending) return;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");
        StartCoroutine(HeavyAttackWindow());
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
    }

    public override void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }

    // --- Internal Coroutines ---
    private IEnumerator AttackWindow(float duration)
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(duration);
        attackCollider.enabled = false;
        yield return new WaitForSeconds(0.1f);
        canAttack = true;
    }

    private IEnumerator HeavyAttackWindow()
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.3f);
        attackCollider.enabled = false;
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
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