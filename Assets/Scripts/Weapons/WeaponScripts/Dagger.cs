using System.Collections;
using UnityEngine;

public class Dagger : Weapon
{
    [Header("Dagger Settings")]
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

        if (comboStep == 1) animator.SetTrigger("LightAttack1");
        else if (comboStep == 2) animator.SetTrigger("LightAttack2");
        else if (comboStep == 3) animator.SetTrigger("LightAttack3");
        else if (comboStep == 4) animator.SetTrigger("LightAttack4");
        else { comboStep = 1; animator.SetTrigger("LightAttack1"); }

        StartCoroutine(AttackWindow(0.15f)); 
        lastAttackTime = Time.time;
    }

    // Daggers have no heavy attack
    public override void HeavyAttack() { }
    public override void StartHeavyCharge() { }
    public override void ReleaseHeavyAttack() { }

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

    private IEnumerator AttackWindow(float duration)
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(duration);
        attackCollider.enabled = false;
        yield return new WaitForSeconds(0.05f); 
        canAttack = true;
    }

    private void ResetAllAttackTriggers()
    {
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("LightAttack4");
    }
}