using System.Collections;
using UnityEngine;

public class Club : Weapon
{
    [Header("Combo Settings")]
    public float comboResetTime = 1.2f;
    private int comboStep = 0;
    private float lastAttackTime;

    [Header("Timing")]
    public float lightDuration = 0.25f;
    public float windupDuration = 0.25f;
    public float slamDuration = 0.45f;
    public float recoveryTime = 0.6f;
    public float minChargeTime = 0.15f;
    public float maxChargeTime = 1.0f;
    private float chargeStartTime;
    private bool releaseAllowed;
    private bool releaseBuffered;

    [Header("Damage")]
    public int lightDamage = 1;
    public int windupDamage = 2;
    public int slamDamage = 4;

    private bool canAttack = true;
    private bool isDefending = false;

    private enum HeavyState { None, Charging, Slamming }
    private HeavyState heavyState = HeavyState.None;

    private ClubHitbox hitbox;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void Awake()
    {
        hitbox = attackCollider.GetComponent<ClubHitbox>();
        if (hitbox == null) Debug.LogWarning("Club: no ClubHitbox found on attackCollider.");
    }

    // -------- LIGHT COMBO (3 HITS) --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetLightTriggers();

        switch (comboStep)
        {
            case 1:
                animator.SetTrigger("LightAttack1");
                sfx?.Club_LightSwingPlay();
                break;
            case 2:
                animator.SetTrigger("LightAttack2");
                sfx?.Club_Light2SwingPlay();
                break;
            case 3:
                animator.SetTrigger("LightAttack3");
                sfx?.Club_Light3SwingPlay();
                break;
            default:
                comboStep = 1;
                animator.SetTrigger("LightAttack1");
                sfx?.Club_LightSwingPlay();
                break;
        }

        StartCoroutine(LightAttackRoutine());
        lastAttackTime = Time.time;
    }

    private void ForceSlam()
    {
        if (heavyState == HeavyState.Slamming) return;

        heavyState = HeavyState.Slamming;
        animator.SetTrigger("HeavyRelease");
        sfx?.Club_HeavySwing1Play(); // Slam sound
        StartCoroutine(SlamHit());
    }

    // -------- HEAVY STAGE 1 (WINDUP HIT) --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending || heavyState != HeavyState.None) return;

        canAttack = false;
        heavyState = HeavyState.Charging;
        chargeStartTime = Time.time;
        releaseBuffered = false;
        releaseAllowed = false;

        animator.SetBool("IsChargingHeavy", true);
        animator.SetTrigger("HeavyWindup");
        sfx?.Club_HeavySwing1Play(); // Windup sound

        StartCoroutine(WindupHit());
    }

    // -------- HEAVY STAGE 2 (SLAM HIT) --------
    public override void ReleaseHeavyAttack()
    {
        if (heavyState != HeavyState.Charging) return;

        if (!releaseAllowed)
        {
            releaseBuffered = true;
            return;
        }

        ForceSlam();
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Club_DefendPlay();
    }

    public override void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }

    // ---------------- ROUTINES ----------------

    private IEnumerator LightAttackRoutine()
    {
        canAttack = false;

        // Set hitbox damage
        hitbox.damage = lightDamage;
        yield return new WaitForSeconds(0.1f);

        attackCollider.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        attackCollider.enabled = false;

        // Play hit sound
        switch (comboStep)
        {
            case 1: sfx?.Club_Light1HitPlay(); break;
            case 2: sfx?.Club_Light2HitPlay(); break;
            case 3: sfx?.Club_Light3HitPlay(); break;
            default: sfx?.Club_Light1HitPlay(); break;
        }

        yield return new WaitForSeconds(recoveryTime * 0.4f);
        canAttack = true;
    }

    private IEnumerator WindupHit()
    {
        yield return new WaitForSeconds(minChargeTime);

        hitbox.damage = windupDamage;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(windupDuration);
        attackCollider.enabled = false;

        sfx?.Club_HeavyHit1Play(); // Windup hit sound

        releaseAllowed = true;

        if (releaseBuffered)
        {
            releaseBuffered = false;
            ForceSlam();
            yield break;
        }

        float endTime = chargeStartTime + maxChargeTime;
        while (Time.time < endTime)
        {
            if (heavyState == HeavyState.Slamming) yield break;
            yield return null;
        }

        if (heavyState == HeavyState.Charging)
        {
            ForceSlam();
        }
    }

    private IEnumerator SlamHit()
    {
        yield return new WaitForSeconds(0.1f);

        hitbox.damage = slamDamage;
        attackCollider.enabled = true;

        yield return new WaitForSeconds(slamDuration);
        attackCollider.enabled = false;

        sfx?.Club_HeavyHit2Play(); // Slam impact sound

        heavyState = HeavyState.None;
        animator.SetBool("IsChargingHeavy", false);
        animator.ResetTrigger("HeavyRelease");

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
        heavyState = HeavyState.None;
        releaseBuffered = false;
        releaseAllowed = false;

        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");
        animator.SetBool("IsChargingHeavy", false);
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        canAttack = true;
        isDefending = false;
    }
}