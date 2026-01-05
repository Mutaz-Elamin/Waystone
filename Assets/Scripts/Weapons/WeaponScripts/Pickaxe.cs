using UnityEngine;
using System.Collections;

public class Pickaxe : Weapon
{
    [Header("Combo Settings")]
    public float lightInitialSpeed = 1f;
    public float lightMaxSpeed = 2.5f;
    public float lightRampTime = 2f;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isHoldingLight = false;
    private float holdStartTime;

    private PickaxeHitbox hitbox;
    public WeaponSFX sfx;

    [Header("SFX")]
    public AudioSource lightLoopSource;
    public AudioClip pickaxeLoopClip;
    public float soundInitialPitch = 0.8f;   // starting pitch of loop
    public float soundMaxPitch = 1.5f;       // max pitch
    public float soundRampTime = 2f;         // how fast sound ramps

    private void Awake()
    {
        hitbox = attackCollider.GetComponent<PickaxeHitbox>();

        // Setup looped AudioSource
        if (lightLoopSource != null && pickaxeLoopClip != null)
        {
            lightLoopSource.clip = pickaxeLoopClip;
            lightLoopSource.loop = true;
        }
    }

    // -------- LIGHT ATTACK LOOP --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        isHoldingLight = true;
        holdStartTime = Time.time;

        animator.SetFloat("LightSpeed", lightInitialSpeed);
        animator.SetBool("LightHold", true);
        attackCollider.enabled = true;
        hitbox.canHit = true;

        // Play looping sound
        if (lightLoopSource != null)
        {
            lightLoopSource.pitch = soundInitialPitch;
            lightLoopSource.Play();
        }

        // Play one-shot swing SFX
        sfx?.Pickaxe_Light1Play();
    }

    public override void StopLightAttack()
    {
        if (!isHoldingLight) return;

        isHoldingLight = false;
        attackCollider.enabled = false;
        hitbox.canHit = false;

        animator.SetBool("LightHold", false);
        animator.SetTrigger("LightRelease");

        // Stop looped sound
        if (lightLoopSource != null)
            lightLoopSource.Stop();
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
            animator.SetFloat("LightSpeed", 1f); // reset when not attacking
        }
    }

    // -------- HEAVY ATTACK --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.SetTrigger("HeavyAttack");
        StartCoroutine(HeavyRoutine());

        // One-shot heavy swing SFX
        sfx?.Pickaxe_HeavySwingPlay();
    }

    private IEnumerator HeavyRoutine()
    {
        hitbox.damage = 3;
        attackCollider.enabled = true;
        hitbox.canHit = true;
        yield return new WaitForSeconds(0.5f);
        attackCollider.enabled = false;
        hitbox.canHit = false;

        // Play heavy hit sound on impact (optional, e.g., on collider trigger instead)
        sfx?.Pickaxe_HeavyHitPlay();

        yield return new WaitForSeconds(0.3f);
        canAttack = true;
    }

    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);

        // Play defend SFX
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

        hitbox.canHit = false;
        isHoldingLight = false;
        canAttack = true;
    }

}