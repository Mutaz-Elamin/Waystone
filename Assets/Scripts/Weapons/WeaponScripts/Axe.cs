using System.Collections;
using UnityEngine;

public class Axe : Weapon
{
    public enum AttackType { None, Light, Heavy }

    [Header("Combo Settings")]
    public float comboResetTime = 1f;

    [Header("State")]
    public int comboStep = 0;
    public AttackType currentAttack = AttackType.None;

    [Header("Timing")]
    public float lightDuration = 0.3f;               // Hit window length (at base speed)
    public float heavyWindupDuration = 2.55f;        // fallback/time-budget only
    public float heavyDuration = 0.6f;               // heavy hit window length
    public float recoveryTime = 0.5f;
    public float baseAttackSpeed = 1f;
    public float speedIncreasePerHit = 0.3f;
    public float maxAttackSpeed = 2.0f;

    [Header("Damage")]
    public int lightDamage = 2;
    public int heavyDamage = 5;

    [Header("References")]
    public WeaponSFX sfx; // optional: will try to find in parent
    [HideInInspector] public AxeHitbox hitbox;

    [Header("Animation sync (tweak if needed)")]
    [Tooltip("Normalized time in the light attack animation where the hit should happen (0..1).")]
    public float lightHitNormalizedTime = 0.25f;
    [Tooltip("Normalized time in the heavy windup/release flow where the heavy hit should occur (0..1).")]
    public float heavyHitNormalizedTime = 0.65f;
    [Tooltip("Maximum seconds to wait for the expected animation state before falling back.")]
    public float animationWaitTimeout = 1.5f;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isChargingHeavy = false;
    private float lastAttackTime;

    private void Awake()
    {
        // try to resolve references
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (attackCollider != null)
            hitbox = attackCollider.GetComponent<AxeHitbox>();

        if (hitbox == null)
            Debug.LogWarning("Axe: No AxeHitbox found on attackCollider.", this);
        if (sfx == null)
            Debug.LogWarning("Axe: WeaponSFX not found in parents.", this);
    }

    // ---------------- LIGHT ATTACK ----------------
    public override void LightAttack()
    {
        if (!canAttack || isDefending || isChargingHeavy) return;

        currentAttack = AttackType.Light;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetLightTriggers();

        // speed ramp (animation)
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

    private IEnumerator LightAttackRoutine(float attackSpeed)
    {
        canAttack = false;

        if (hitbox != null) hitbox.damage = lightDamage;

        // Determine target state name (store locally since comboStep may change)
        int step = comboStep;
        string stateName = $"LightAttack{step}";

        // Wait for the animator to enter that state and reach threshold normalized time
        yield return StartCoroutine(WaitForAnimationStateAndNormalizedTime(stateName, lightHitNormalizedTime, animationWaitTimeout));

        // Enable hitbox for the configured window (scaled by attackSpeed)
        EnableHitbox(true);
        yield return new WaitForSeconds(lightDuration / attackSpeed);
        EnableHitbox(false);

        // restore animator speed & state
        animator.speed = baseAttackSpeed;
        currentAttack = AttackType.None;

        // small recovery then allow next attack
        yield return new WaitForSeconds(recoveryTime / Mathf.Max(0.0001f, attackSpeed));
        canAttack = true;
    }

    // ---------------- HEAVY ATTACK ----------------
    // For this axe we keep the single windup->attack flow (no release mechanic)
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        isChargingHeavy = true;

        animator.SetBool("IsChargingHeavy", true);
        animator.SetTrigger("HeavyWindup");
        sfx?.Axe_HeavySwingPlay(); // windup/charge sound

        StartCoroutine(HeavyAttackRoutine());
    }

    public override void ReleaseHeavyAttack()
    {
        // This axe uses automatic windup flow; implement release behavior here if you later add it.
    }

    private IEnumerator HeavyAttackRoutine()
    {
        currentAttack = AttackType.Heavy;

        // Wait for the animation to actually get to the release/hit portion.
        // Prefer to wait for the windup state to reach heavyHitNormalizedTime.
        // Use timeout fallback so coroutine never loops forever.
        yield return StartCoroutine(WaitForAnimationStateAndNormalizedTime("HeavyWindup", heavyHitNormalizedTime, animationWaitTimeout));

        // set heavy damage and enable hit
        if (hitbox != null) hitbox.damage = heavyDamage;
        EnableHitbox(true);

        // keep hitbox for heavyDuration
        yield return new WaitForSeconds(heavyDuration);

        EnableHitbox(false);

        // play heavy hit (hitbox or hitbox.OnHit should also attempt sfx)
        sfx?.Axe_HeavyHitPlay();

        isChargingHeavy = false;
        animator.SetBool("IsChargingHeavy", false);

        // recovery
        yield return new WaitForSeconds(recoveryTime);
        currentAttack = AttackType.None;
        canAttack = true;
    }

    // ---------------- DEFEND ----------------
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

    // ---------------- HELPERS ----------------
    private void EnableHitbox(bool enabled)
    {
        if (attackCollider == null) return;

        attackCollider.enabled = enabled;
        if (hitbox != null) hitbox.canHit = enabled;
    }

    private IEnumerator WaitForAnimationStateAndNormalizedTime(string stateName, float normalizedThreshold, float timeout)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            yield break;
        }

        float timer = 0f;

        // Wait for the animator to enter the expected state
        while (timer < timeout)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName))
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        // If state didn't appear, bail out
        if (timer >= timeout)
            yield break;

        // Now wait until the normalized time passes threshold (or timeout)
        timer = 0f;
        while (timer < timeout)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName) && info.normalizedTime >= normalizedThreshold)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        yield break;
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
        currentAttack = AttackType.None;

        ResetLightTriggers();
        animator.ResetTrigger("HeavyWindup");
        animator.ResetTrigger("HeavyAttack");
        animator.SetBool("IsChargingHeavy", false);
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitbox != null)
            hitbox.canHit = false;

        animator.speed = baseAttackSpeed;
        canAttack = true;
        isDefending = false;
    }
}