using System.Collections;
using UnityEngine;

public class Pickaxe : Weapon
{
    [Header("Combo Settings")]
    public float lightInitialSpeed = 1f;       // animation start speed
    public float lightMaxSpeed = 2.5f;         // animation max speed while holding
    public float lightRampTime = 2f;           // how long to ramp animation speed

    [Header("Hold / hit timing")]
    public float startupDelay = 0.15f;         // delay from hold start -> first possible hit
    public float hitInterval = 0.25f;          // time between successive hits while holding
    public float releaseCooldown = 0.12f;      // small cooldown after releasing before you can start again

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isHoldingLight = false;
    private float holdStartTime;

    [HideInInspector] public PickaxeHitbox hitbox;
    public WeaponSFX sfx;

    [Header("SFX")]
    public AudioSource lightLoopSource;
    public AudioClip pickaxeLoopClip;
    public float soundInitialPitch = 0.8f;
    public float soundMaxPitch = 1.5f;
    public float soundRampTime = 2f;

    // expose hold state so hitbox can query timing
    public bool IsHoldingLight => isHoldingLight;
    public float HoldElapsed => isHoldingLight ? Time.time - holdStartTime : 0f;

    // heavy flag so hitbox can treat the collision as a single heavy hit
    public bool IsPerformingHeavy { get; private set; } = false;

    private void Awake()
    {
        hitbox = attackCollider != null ? attackCollider.GetComponent<PickaxeHitbox>() : null;

        // Setup looped AudioSource
        if (lightLoopSource != null && pickaxeLoopClip != null)
        {
            lightLoopSource.clip = pickaxeLoopClip;
            lightLoopSource.loop = true;
        }

        // try to resolve sfx if not assigned
        sfx ??= GetComponentInParent<WeaponSFX>();

        // ensure collider starts disabled
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // -------- LIGHT ATTACK LOOP --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending || isHoldingLight) return;

        // lock starting new holds to avoid spam at startup
        canAttack = false;
        isHoldingLight = true;
        holdStartTime = Time.time;

        animator.SetFloat("LightSpeed", lightInitialSpeed);
        animator.SetBool("LightHold", true);

        // do not enable collider immediately — use startupDelay so animation lines up
        StartCoroutine(HoldStartupCoroutine());

        // Start looped sound
        if (lightLoopSource != null)
        {
            lightLoopSource.pitch = soundInitialPitch;
            lightLoopSource.Play();
        }

        // One-shot accent SFX
        sfx?.Pickaxe_Light1Play();
    }

    private IEnumerator HoldStartupCoroutine()
    {
        // wait for the small startup delay (animation leads)
        yield return new WaitForSeconds(startupDelay);

        // if player already released, do nothing
        if (!isHoldingLight) yield break;

        if (attackCollider != null) attackCollider.enabled = true;
        if (hitbox != null)
        {
            hitbox.canHit = true;
            hitbox.SetHitInterval(hitInterval); // inform hitbox of cadence
        }
    }

    public override void StopLightAttack()
    {
        if (!isHoldingLight) return;

        isHoldingLight = false;

        if (attackCollider != null) attackCollider.enabled = false;
        if (hitbox != null) hitbox.canHit = false;

        animator.SetBool("LightHold", false);
        animator.SetTrigger("LightRelease");

        // stop looped sound
        if (lightLoopSource != null && lightLoopSource.isPlaying)
            lightLoopSource.Stop();

        // small cooldown before allowing next LightAttack to prevent spam
        StartCoroutine(ReleaseCooldownRoutine());
    }

    private IEnumerator ReleaseCooldownRoutine()
    {
        yield return new WaitForSeconds(releaseCooldown);
        canAttack = true;
    }

    private void Update()
    {
        if (isHoldingLight)
        {
            // Ramp animation speed
            float elapsed = Time.time - holdStartTime;
            float t = Mathf.Clamp01(elapsed / lightRampTime);
            float speed = Mathf.Lerp(lightInitialSpeed, lightMaxSpeed, t);
            animator.SetFloat("LightSpeed", speed);

            // Ramp looped sound pitch
            if (lightLoopSource != null)
            {
                float st = Mathf.Clamp01(elapsed / soundRampTime);
                lightLoopSource.pitch = Mathf.Lerp(soundInitialPitch, soundMaxPitch, st);
            }
        }
        else
        {
            // Reset animator speed when not holding
            animator.SetFloat("LightSpeed", 1f);
        }
    }

    // -------- HEAVY ATTACK (single hit via OnTriggerEnter) --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.SetTrigger("HeavyAttack");
        StartCoroutine(HeavyRoutine());

        sfx?.Pickaxe_HeavySwingPlay();
    }

    private IEnumerator HeavyRoutine()
    {
        // mark heavy so hitbox uses OnTriggerEnter immediate behavior
        IsPerformingHeavy = true;

        if (hitbox != null) hitbox.damage = 3;
        if (attackCollider != null) attackCollider.enabled = true;
        if (hitbox != null) hitbox.canHit = true;

        // wait briefly to allow OnTriggerEnter to fire for overlapping enemies
        yield return new WaitForSeconds(0.5f);

        // disable
        if (attackCollider != null) attackCollider.enabled = false;
        if (hitbox != null) hitbox.canHit = false;

        // finish heavy
        IsPerformingHeavy = false;

        // Play heavy hit sound (optional fallback)
        sfx?.Pickaxe_HeavyHitPlay();

        yield return new WaitForSeconds(0.3f);
        canAttack = true;
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Pickaxe_DefendPlay();
    }

    public override void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }

    public override void ResetWeapon()
    {
        // Stop the looped sound
        if (lightLoopSource != null && lightLoopSource.isPlaying)
        {
            lightLoopSource.Stop();
            lightLoopSource.pitch = soundInitialPitch;
        }

        // Reset animator
        if (animator != null)
        {
            animator.SetBool("LightHold", false);
            animator.SetFloat("LightSpeed", 1f);
            animator.ResetTrigger("LightRelease");
        }

        // Reset collider and flags
        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitbox != null)
        {
            hitbox.canHit = false;
            hitbox.ClearLastHitTimes();
        }

        isHoldingLight = false;
        canAttack = true;
        IsPerformingHeavy = false;
    }
}
