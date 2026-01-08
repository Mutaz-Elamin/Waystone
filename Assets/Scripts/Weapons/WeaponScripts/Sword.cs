using System.Collections;
using UnityEngine;

public class Sword : Weapon
{
    public enum AttackType
    {
        None,
        Light,
        Heavy
    }



    [Header("Sword Settings")]
    public float comboResetTime = 1f;

    [Header("State")]
    public int comboStep = 0;
    public AttackType currentAttack = AttackType.None;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isChargingHeavy = false;
    private float lastAttackTime;

    [Header("Particles")]
    public ParticleSystem swingParticles;

    [Header("Sword SFX")]
    private WeaponSFX sfx;

    private void Awake()
    {
        sfx = GetComponentInParent<WeaponSFX>();

        if (sfx == null)
            Debug.LogWarning("WeaponSFX not found for SwordHitbox", this);
    }

    // =========================
    // LIGHT ATTACK
    // =========================
    public override void LightAttack()
    {
        if (!canAttack || isDefending || isChargingHeavy) return;

        currentAttack = AttackType.Light;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime)
            comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        switch (comboStep)
        {
            case 1:
                animator.SetTrigger("LightAttack");
                sfx?.Sword_Light1SwingPlay();
                break;
            case 2:
                animator.SetTrigger("LightAttack2");
                sfx?.Sword_Light2SwingPlay();
                break;
            case 3:
                animator.SetTrigger("LightAttack3");
                sfx?.Sword_Light3SwingPlay();
                break;
            default:
                comboStep = 1;
                animator.SetTrigger("LightAttack");
                sfx?.Sword_Light1SwingPlay();
                break;
        }

        StartCoroutine(LightAttackWindow(0.25f));
        lastAttackTime = Time.time;
    }

    // =========================
    // HEAVY ATTACK
    // =========================
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        isChargingHeavy = true;

        animator.SetBool("IsChargingHeavy", true);
        animator.SetTrigger("HeavyWindup");
        sfx?.Sword_HeavyChargePlay();
    }

    public override void ReleaseHeavyAttack()
    {
        if (isDefending || !isChargingHeavy) return;

        isChargingHeavy = false;
        currentAttack = AttackType.Heavy;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");

        sfx?.Sword_HeavySwingPlay();
        StartCoroutine(HeavyAttackWindow());
    }

    // =========================
    // DEFENSE
    // =========================
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

    // =========================
    // ATTACK WINDOWS
    // =========================
    private IEnumerator LightAttackWindow(float duration)
    {
        canAttack = false;

        EnableHitbox(true);
        yield return new WaitForSeconds(duration);
        EnableHitbox(false);

        currentAttack = AttackType.None;

        yield return new WaitForSeconds(0.1f);
        canAttack = true;
    }

    private IEnumerator HeavyAttackWindow()
    {
        canAttack = false;

        yield return new WaitForSeconds(0.4f);

        EnableHitbox(true);
        yield return new WaitForSeconds(0.3f);
        EnableHitbox(false);

        currentAttack = AttackType.None;

        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }

    // =========================
    // HELPERS
    // =========================
    private void EnableHitbox(bool enabled)
    {
        if (attackCollider == null) return;

        attackCollider.enabled = enabled;
        SwordHitbox hitbox = attackCollider.GetComponent<SwordHitbox>();
        if (hitbox != null)
            hitbox.canHit = enabled;
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
        currentAttack = AttackType.None;

        ResetAllAttackTriggers();
        animator.SetBool("IsDefending", false);
        animator.SetBool("IsChargingHeavy", false);

        EnableHitbox(false);

        canAttack = true;
        isDefending = false;
        isChargingHeavy = false;

        if (swingParticles != null)
            swingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}