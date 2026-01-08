using System.Collections;
using UnityEngine;

public class Club : Weapon
{
    public enum AttackType { None, Light, Heavy }

    [Header("Combo Settings")]
    public float comboResetTime = 1.2f;
    [Header("State")]
    public int comboStep = 0;
    public AttackType currentAttack = AttackType.None;
    private float lastAttackTime;

    [Header("Timing")]
    public float lightDuration = 0.25f;
    public float windupDuration = 0.45f;    // duration of windup hit window
    public float slamDuration = 0.65f;      // duration of slam hit window
    public float recoveryTime = 0.6f;
    public float minChargeTime = 0.15f;
    public float maxChargeTime = 1.0f;

    [Header("Damage")]
    public int lightDamage = 1;
    public int windupDamage = 2;
    public int slamDamage = 4;

    [Header("References")]
    public WeaponSFX sfx; // optional: auto-resolve in Awake
    [HideInInspector] public ClubHitbox hitbox;

    private bool canAttack = true;
    private bool isDefending = false;

    private enum HeavyState { None, Charging, Slamming }
    private HeavyState heavyState = HeavyState.None;

    // buffering
    private float chargeStartTime;
    private bool releaseAllowed;
    private bool releaseBuffered;

    private void Awake()
    {
        // resolve optional references
        sfx ??= GetComponentInParent<WeaponSFX>();
        if (attackCollider != null)
            hitbox = attackCollider.GetComponent<ClubHitbox>();

        if (hitbox == null)
            Debug.LogWarning("Club: no ClubHitbox found on attackCollider.", this);

        // ensure disabled by default
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // ---------------- LIGHT ATTACK ----------------
    public override void LightAttack()
    {
        if (!canAttack || isDefending || heavyState != HeavyState.None) return;

        currentAttack = AttackType.Light;

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

    private IEnumerator LightAttackRoutine()
    {
        canAttack = false;

        // set damage so hitbox knows what to apply
        if (hitbox != null) hitbox.damage = lightDamage;

        // small sync to hit animation
        yield return new WaitForSeconds(0.5f);

        EnableHitbox(true);
        yield return new WaitForSeconds(lightDuration);
        EnableHitbox(false);

        // play hit sound fallback from hitbox or here if you prefer
        switch (comboStep)
        {
            case 1: sfx?.Club_Light1HitPlay(); break;
            case 2: sfx?.Club_Light2HitPlay(); break;
            case 3: sfx?.Club_Light3HitPlay(); break;
            default: sfx?.Club_Light1HitPlay(); break;
        }

        yield return new WaitForSeconds(recoveryTime * 0.4f);
        canAttack = true;
        currentAttack = AttackType.None;
    }

    // ---------------- HEAVY (two-stage) ----------------
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
        sfx?.Club_HeavySwing1Play(); // windup sound

        StartCoroutine(WindupRoutine());
    }

    // call to release slam (player release)
    public override void ReleaseHeavyAttack()
    {
        if (heavyState != HeavyState.Charging) return;

        if (!releaseAllowed)
        {
            releaseBuffered = true;
            return;
        }

        // immediate slam
        ForceSlam();
    }

    private IEnumerator WindupRoutine()
    {
        // allow minimal charge before windup hit
        yield return new WaitForSeconds(minChargeTime);

        // windup hit (single hit window)
        if (hitbox != null) hitbox.damage = windupDamage;
        EnableHitbox(true);
        // mark attack type so hitbox can pick correct sound/knockback
        currentAttack = AttackType.Heavy;
        yield return new WaitForSeconds(windupDuration);
        EnableHitbox(false);

        // play windup hit fallback
        sfx?.Club_HeavyHit1Play();

        // now allow release to slam
        releaseAllowed = true;

        if (releaseBuffered)
        {
            // if player already pressed release, slam immediately
            releaseBuffered = false;
            ForceSlam();
            yield break;
        }

        // wait up to maxChargeTime for auto-slam
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

    private void ForceSlam()
    {
        if (heavyState == HeavyState.Slamming) return;

        heavyState = HeavyState.Slamming;
        animator.SetTrigger("HeavyRelease");
        sfx?.Club_HeavySwing1Play(); // slam sound (or another)
        StartCoroutine(SlamRoutine());
    }

    private IEnumerator SlamRoutine()
    {
        // small delay to line up with animation if needed
        yield return new WaitForSeconds(0.1f);

        if (hitbox != null) hitbox.damage = slamDamage;
        currentAttack = AttackType.Heavy;

        EnableHitbox(true);
        yield return new WaitForSeconds(slamDuration);
        EnableHitbox(false);

        // slam impact fallback
        sfx?.Club_HeavyHit2Play();

        // reset
        heavyState = HeavyState.None;
        animator.SetBool("IsChargingHeavy", false);
        animator.ResetTrigger("HeavyRelease");

        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
        currentAttack = AttackType.None;
    }

    // ---------------- DEFEND ----------------
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

    // ---------------- HELPERS ----------------
    private void EnableHitbox(bool enabled)
    {
        if (attackCollider == null) return;

        attackCollider.enabled = enabled;
        if (hitbox != null)
            hitbox.canHit = enabled;
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

        ResetLightTriggers();
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyRelease");
        animator.SetBool("IsChargingHeavy", false);
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitbox != null)
            hitbox.canHit = false;

        canAttack = true;
        isDefending = false;
        currentAttack = AttackType.None;
    }
}