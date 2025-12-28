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

        if (comboStep == 1) animator.SetTrigger("LightAttack1");
        else if (comboStep == 2) animator.SetTrigger("LightAttack2");
        else if (comboStep == 3) animator.SetTrigger("LightAttack3");
        else { comboStep = 1; animator.SetTrigger("LightAttack1"); }

        StartCoroutine(LightAttackRoutine());
        lastAttackTime = Time.time;
    }

    private void ForceSlam()
    {
        // If already slamming or not charging, ignore
        if (heavyState == HeavyState.Slamming) return;

        Debug.Log("[Club] ForceSlam called. heavyState -> Slamming");
        heavyState = HeavyState.Slamming;

        // Keep IsChargingHeavy true until slam ends, so animator can stay in the windup->slam flow.
        // Trigger the slam animation. Slam cleanup will turn off IsChargingHeavy.
        animator.SetTrigger("HeavyRelease");

        StartCoroutine(SlamHit());
    }

    // -------- HEAVY STAGE 1 (WINDUP HIT) --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending || heavyState != HeavyState.None) return;

        Debug.Log("[Club] StartHeavyCharge");
        canAttack = false;
        heavyState = HeavyState.Charging;

        chargeStartTime = Time.time;
        releaseBuffered = false;
        releaseAllowed = false;

        animator.SetBool("IsChargingHeavy", true);
        animator.SetTrigger("HeavyWindup");

        StartCoroutine(WindupHit());
    }

    // -------- HEAVY STAGE 2 (SLAM HIT) --------
    public override void ReleaseHeavyAttack()
    {
        if (heavyState != HeavyState.Charging) return;

        Debug.Log("[Club] ReleaseHeavyAttack called. releaseAllowed=" + releaseAllowed);

        // If player releases too early, buffer it
        if (!releaseAllowed)
        {
            releaseBuffered = true;
            return;
        }

        // If release allowed, then slam immediately
        ForceSlam();
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

    // ---------------- ROUTINES ----------------

    private IEnumerator LightAttackRoutine()
    {
        canAttack = false;

        hitbox.damage = lightDamage;
        yield return new WaitForSeconds(0.1f);

        attackCollider.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        attackCollider.enabled = false;

        yield return new WaitForSeconds(recoveryTime * 0.4f);
        canAttack = true;
    }

    private IEnumerator WindupHit()
    {
        // Wait the small minimum charge time before doing windup hit (gives the feel of wind-up)
        yield return new WaitForSeconds(minChargeTime);

        // Windup hit
        hitbox.damage = windupDamage;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(windupDuration);
        attackCollider.enabled = false;

        // Now allow release (the forgiving window begins)
        releaseAllowed = true;
        Debug.Log("[Club] releaseAllowed = true");

        // If player already released early, slam immediately
        if (releaseBuffered)
        {
            Debug.Log("[Club] release was buffered -> ForceSlam now");
            releaseBuffered = false;
            ForceSlam();
            yield break; // slam started, stop this coroutine
        }

        // Otherwise wait until max charge time or until ForceSlam/Release triggers slam
        float endTime = chargeStartTime + maxChargeTime;
        while (Time.time < endTime)
        {
            // If ForceSlam was called elsewhere, exit
            if (heavyState == HeavyState.Slamming) yield break;
            yield return null;
        }

        // Auto-slam if player never released
        if (heavyState == HeavyState.Charging)
        {
            Debug.Log("[Club] maxChargeTime reached -> ForceSlam");
            ForceSlam();
        }
    }

    private IEnumerator SlamHit()
    {
        // Small delay so the animation can start and line up with hit window
        yield return new WaitForSeconds(0.1f);

        hitbox.damage = slamDamage;
        attackCollider.enabled = true;

        yield return new WaitForSeconds(slamDuration);
        attackCollider.enabled = false;

        // Slam finished, clear charging visuals/state
        heavyState = HeavyState.None;
        animator.SetBool("IsChargingHeavy", false);
        animator.ResetTrigger("HeavyRelease");

        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;

        Debug.Log("[Club] Slam finished, canAttack = true");
    }

    private void ResetLightTriggers()
    {
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
    }
}