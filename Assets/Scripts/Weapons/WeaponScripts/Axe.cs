using System.Collections;
using UnityEngine;

public class Axe : Weapon
{
    [Header("Combo Settings")]
    public float comboResetTime = 2.2f;
    private int comboStep = 0;
    private float lastAttackTime;

    [Header("Timing")]
    public float lightDuration = 0.3f;
    public float heavyWindupDuration = 0.25f;
    public float heavyDuration = 0.6f;
    public float recoveryTime = 0.5f;
    public float baseAttackSpeed = 1f;
    public float speedIncreasePerHit = 0.3f;
    public float maxAttackSpeed = 2.0f;

    [Header("Damage")]
    public int lightDamage = 2;
    public int heavyDamage = 5;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isChargingHeavy = false;

    private AxeHitbox hitbox;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void Awake()
    {
        hitbox = attackCollider.GetComponent<AxeHitbox>();
        if (hitbox == null) Debug.LogWarning("Axe: No AxeHitbox found on attackCollider.");
    }

    // -------- LIGHT COMBO (3 HITS) --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime)
            comboStep = 0;

        comboStep++;
        ResetLightTriggers();

        // ---- SPEED RAMP ----
        float attackSpeed = baseAttackSpeed + (comboStep - 1) * speedIncreasePerHit;
        attackSpeed = Mathf.Min(attackSpeed, maxAttackSpeed);
        animator.speed = attackSpeed;

        switch (comboStep)
        {
            case 1:
                animator.SetTrigger("LightAttack1");
                sfx?.Axe_Light1SwingPlay();
                break;
            case 2:
                animator.SetTrigger("LightAttack2");
                sfx?.Axe_Light2SwingPlay();
                break;
            case 3:
                animator.SetTrigger("LightAttack3");
                sfx?.Axe_Light3SwingPlay();
                break;
            default:
                comboStep = 1;
                animator.speed = baseAttackSpeed;
                animator.SetTrigger("LightAttack1");
                sfx?.Axe_Light1SwingPlay();
                break;
        }

        StartCoroutine(LightAttackRoutine(attackSpeed));
        lastAttackTime = Time.time;
    }

    // -------- HEAVY ATTACK --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        isChargingHeavy = true;

        animator.SetBool("IsChargingHeavy", true);
        animator.SetTrigger("HeavyWindup");
        sfx?.Axe_HeavySwingPlay(); // heavy windup sound

        StartCoroutine(HeavyAttackRoutine());
    }

    public override void ReleaseHeavyAttack()
    {
        // For this Axe, we do not have a charge release mechanic
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Axe_DefendPlay();
    }

    public override void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }

    // ---------------- ROUTINES ----------------

    private IEnumerator LightAttackRoutine(float attackSpeed)
    {
        canAttack = false;

        hitbox.damage = lightDamage;
        attackCollider.enabled = true;

        yield return new WaitForSeconds(lightDuration / attackSpeed);

        attackCollider.enabled = false;

        // Play hit sound for this swing
        switch (comboStep)
        {
            case 1: sfx?.Axe_Light1HitPlay(); break;
            case 2: sfx?.Axe_Light2HitPlay(); break;
            case 3: sfx?.Axe_Light3HitPlay(); break;
            default: sfx?.Axe_Light1HitPlay(); break;
        }

        yield return new WaitForSeconds(recoveryTime / attackSpeed);

        animator.speed = baseAttackSpeed;
        canAttack = true;
    }

    private IEnumerator HeavyAttackRoutine()
    {
        yield return new WaitForSeconds(heavyWindupDuration);

        hitbox.damage = heavyDamage;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(heavyDuration);
        attackCollider.enabled = false;

        sfx?.Axe_HeavyHitPlay(); // heavy hit sound

        isChargingHeavy = false;
        animator.SetBool("IsChargingHeavy", false);

        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }

    private void ResetLightTriggers()
    {
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
    }

    public override void ResetWeapon()
    {
        comboStep = 0;
        isChargingHeavy = false;

        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyAttack");
        animator.SetBool("IsChargingHeavy", false);
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        canAttack = true;
        isDefending = false;
    }
}