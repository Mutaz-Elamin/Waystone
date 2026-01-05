using System.Collections;
using UnityEngine;
public class Spear : Weapon
{
    [Header("Spear Settings")]
    public float comboResetTime = 1.5f;
    private int comboStep = 0;
    private bool canAttack = true;
    private float lastAttackTime;
    private bool isDefending = false;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    // Expose combo step to hitbox
    public int ComboStep => comboStep;

    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        if (comboStep == 1)
        {
            animator.SetTrigger("LightAttack1");
            sfx?.Spear_Light1SwingPlay();
        }
        else if (comboStep == 2)
        {
            animator.SetTrigger("LightAttack2");
            sfx?.Spear_Light2SwingPlay();
        }
        else
        {
            comboStep = 1;
            animator.SetTrigger("LightAttack1");
            sfx?.Spear_Light1SwingPlay();
        }

        StartCoroutine(AttackWindow(0.35f));
        lastAttackTime = Time.time;
    }

    public override void HeavyAttack()
    {
        if (!canAttack || isDefending) return;
        animator.SetTrigger("HeavyWindup");
        sfx?.Spear_HeavySwingPlay();
        StartCoroutine(HeavyAttackWindow());
    }

    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.ResetTrigger("HeavyRelease");
        animator.SetTrigger("HeavyWindup");
        animator.SetBool("IsChargingHeavy", true);
        sfx?.Spear_HeavyChargePlay();
    }

    public override void ReleaseHeavyAttack()
    {
        if (isDefending) return;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");
        StartCoroutine(HeavyAttackWindow());
        sfx?.Spear_HeavySwingPlay();
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Spear_DefendPlay();
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
        switch (comboStep)
        {
            case 1: sfx?.Spear_Light1HitPlay(); break;
            case 2: sfx?.Spear_Light2HitPlay(); break;
        }
        yield return new WaitForSeconds(0.15f);
        canAttack = true;
    }

    private IEnumerator HeavyAttackWindow()
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        attackCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);
        canAttack = true;
    }

    private void ResetAllAttackTriggers()
    {
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");
    }

    public override void ResetWeapon()
    {
        comboStep = 0;

        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        canAttack = true;
        isDefending = false;

    }


}
