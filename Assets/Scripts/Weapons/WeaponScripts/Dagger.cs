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

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    // Expose combo step for hitbox
    public int ComboStep => comboStep;

    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        switch (comboStep)
        {
            case 1:
                animator.SetTrigger("LightAttack1");
                sfx?.Dagger_Light1SwingPlay();
                break;
            case 2:
                animator.SetTrigger("LightAttack2");
                sfx?.Dagger_Light2SwingPlay();
                break;
            case 3:
                animator.SetTrigger("LightAttack3");
                sfx?.Dagger_Light3SwingPlay();
                break;
            case 4:
                animator.SetTrigger("LightAttack4");
                sfx?.Dagger_Light4SwingPlay();
                break;
            default:
                comboStep = 1;
                animator.SetTrigger("LightAttack1");
                sfx?.Dagger_Light1SwingPlay();
                break;
        }

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
        sfx?.Dagger_DefendPlay();
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
            case 1: sfx?.Dagger_Light1HitPlay(); break;
            case 2: sfx?.Dagger_Light2HitPlay(); break;
            case 3: sfx?.Dagger_Light3HitPlay(); break;
            case 4: sfx?.Dagger_Light4HitPlay(); break;
        }
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

    public override void ResetWeapon()
    {
        comboStep = 0;

        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("LightAttack4");
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        canAttack = true;
        isDefending = false;

    }
}