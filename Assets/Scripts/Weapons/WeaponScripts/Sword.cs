using System.Collections;
using UnityEditor.VersionControl;
using UnityEngine;

public class Sword : Weapon
{
    [Header("Sword Settings")]
    public float comboResetTime = 1f;

    public int comboStep = 0;
    private bool canAttack = true;
    private float lastAttackTime;
    private bool isDefending = false;

    [Header("Sword SFX")]
    public WeaponSFX sfx; // assign your PlayerSFX here in inspector

    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        if (comboStep == 1)
        {
            animator.SetTrigger("LightAttack");
            sfx?.Sword_Light1SwingPlay();
        }
        else if (comboStep == 2)
        {
            animator.SetTrigger("LightAttack2");
            sfx?.Sword_Light2SwingPlay();
        }
        else if (comboStep == 3)
        {
            animator.SetTrigger("LightAttack3");
            sfx?.Sword_Light3SwingPlay();
        }
        else
        {
            comboStep = 1;
            animator.SetTrigger("LightAttack");
            sfx?.Sword_Light1SwingPlay();
        }

        StartCoroutine(AttackWindow(0.25f));
        lastAttackTime = Time.time;
    }

    public override void HeavyAttack()
    {
        if (!canAttack || isDefending) return;
        animator.SetTrigger("HeavyWindup");
        sfx?.Sword_HeavyChargePlay();
        StartCoroutine(HeavyAttackWindow());
    }

    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.ResetTrigger("HeavyRelease");
        animator.SetTrigger("HeavyWindup");
        animator.SetBool("IsChargingHeavy", true);
        sfx?.Sword_HeavyChargePlay();
    }

    public override void ReleaseHeavyAttack()
    {
        if (isDefending) return;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");
        sfx?.Sword_HeavySwingPlay(); // swing sound
        StartCoroutine(HeavyAttackWindow());
        // Note: hit sound should be called from Hitbox or animation event
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Sword_DefendPlay();
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
        // Play hit sounds here if you want them to trigger on contact
        switch (comboStep)
        {
            case 1: sfx?.Sword_Light1HitPlay(); break;
            case 2: sfx?.Sword_Light2HitPlay(); break;
            case 3: sfx?.Sword_Light3HitPlay(); break;
        }

        yield return new WaitForSeconds(0.1f);
        canAttack = true;
    }

    private IEnumerator HeavyAttackWindow()
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.3f);

        attackCollider.enabled = false;
        sfx?.Sword_HeavyHitPlay(); // play heavy hit when collider disables

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

    public override void ResetWeapon()
    {

        comboStep = 0;


        animator.ResetTrigger("LightAttack");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");

        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        canAttack = true;
        isDefending = false;
    }
}
