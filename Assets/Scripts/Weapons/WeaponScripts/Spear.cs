using System.Collections;
using UnityEngine;

public class Spear : Weapon
{
    public enum AttackType
    {
        None,
        Light,
        Heavy
    }

    [Header("Spear Settings")]
    public float comboResetTime = 1.5f;

    [Header("State")]
    public int comboStep = 0;
    public AttackType currentAttack = AttackType.None;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isChargingHeavy = false;
    private float lastAttackTime;

    [Header("SFX")]
    private WeaponSFX sfx;

    private void Awake()
    {
        sfx = GetComponentInParent<WeaponSFX>();

        if (sfx == null)
            Debug.LogWarning("WeaponSFX not found for Spear", this);
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
                animator.SetTrigger("LightAttack1");
                sfx?.Spear_Light1SwingPlay();
                break;
            case 2:
                animator.SetTrigger("LightAttack2");
                sfx?.Spear_Light2SwingPlay();
                break;
            default:
                comboStep = 1;
                animator.SetTrigger("LightAttack1");
                sfx?.Spear_Light1SwingPlay();
                break;
        }

        StartCoroutine(LightAttackWindow(0.75f));
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
        sfx?.Spear_HeavyChargePlay();
    }

    public override void ReleaseHeavyAttack()
    {
        if (!isChargingHeavy || isDefending) return;

        isChargingHeavy = false;
        currentAttack = AttackType.Heavy;

        animator.SetBool("IsChargingHeavy", false);
        animator.SetTrigger("HeavyRelease");

        sfx?.Spear_HeavySwingPlay();
        StartCoroutine(HeavyAttackWindow());
    }

    // =========================
    // DEFENSE
    // =========================
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

    // =========================
    // ATTACK WINDOWS
    // =========================
    private IEnumerator LightAttackWindow(float duration)
    {
        canAttack = false;
        yield return new WaitForSeconds(0.85f);
        EnableHitbox(true);
        yield return new WaitForSeconds(duration);
        EnableHitbox(false);

        currentAttack = AttackType.None;

        yield return new WaitForSeconds(0.15f);
        canAttack = true;
    }

    private IEnumerator HeavyAttackWindow()
    {
        canAttack = false;

        yield return new WaitForSeconds(0.45f); 

        EnableHitbox(true);
        yield return new WaitForSeconds(0.3f);
        EnableHitbox(false);

        currentAttack = AttackType.None;

        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    // =========================
    // HELPERS
    // =========================
    private void EnableHitbox(bool enabled)
    {
        if (attackCollider == null) return;

        attackCollider.enabled = enabled;
        SpearHitbox hitbox = attackCollider.GetComponent<SpearHitbox>();
        if (hitbox != null)
            hitbox.canHit = enabled;
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
        currentAttack = AttackType.None;

        ResetAllAttackTriggers();
        animator.SetBool("IsDefending", false);
        animator.SetBool("IsChargingHeavy", false);

        EnableHitbox(false);

        canAttack = true;
        isDefending = false;
        isChargingHeavy = false;
    }
}
