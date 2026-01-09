using System.Collections;
using UnityEngine;

public class Pickaxe : Weapon
{
    [Header("Combo Settings")]
    public float lightInitialSpeed = 1f;       
    public float lightMaxSpeed = 2.5f;        
    public float lightRampTime = 2f;           

    [Header("Hold / hit timing")]
    public float startupDelay = 0.15f;         
    public float hitInterval = 0.25f;          
    public float releaseCooldown = 0.12f;      

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

    public bool IsHoldingLight => isHoldingLight;
    public float HoldElapsed => isHoldingLight ? Time.time - holdStartTime : 0f;


    public bool IsPerformingHeavy { get; private set; } = false;

    private void Awake()
    {
        hitbox = attackCollider != null ? attackCollider.GetComponent<PickaxeHitbox>() : null;


        if (lightLoopSource != null && pickaxeLoopClip != null)
        {
            lightLoopSource.clip = pickaxeLoopClip;
            lightLoopSource.loop = true;
        }

        sfx ??= GetComponentInParent<WeaponSFX>();


        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // -------- LIGHT ATTACK LOOP --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending || isHoldingLight) return;


        canAttack = false;
        isHoldingLight = true;
        holdStartTime = Time.time;

        animator.SetFloat("LightSpeed", lightInitialSpeed);
        animator.SetBool("LightHold", true);

        StartCoroutine(HoldStartupCoroutine());


        if (lightLoopSource != null)
        {
            lightLoopSource.pitch = soundInitialPitch;
            lightLoopSource.Play();
        }


        sfx?.Pickaxe_Light1Play();
    }

    private IEnumerator HoldStartupCoroutine()
    {

        yield return new WaitForSeconds(startupDelay);


        if (!isHoldingLight) yield break;

        if (attackCollider != null) attackCollider.enabled = true;
        if (hitbox != null)
        {
            hitbox.canHit = true;
            hitbox.SetHitInterval(hitInterval);
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


        if (lightLoopSource != null && lightLoopSource.isPlaying)
            lightLoopSource.Stop();

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

            float elapsed = Time.time - holdStartTime;
            float t = Mathf.Clamp01(elapsed / lightRampTime);
            float speed = Mathf.Lerp(lightInitialSpeed, lightMaxSpeed, t);
            animator.SetFloat("LightSpeed", speed);

            if (lightLoopSource != null)
            {
                float st = Mathf.Clamp01(elapsed / soundRampTime);
                lightLoopSource.pitch = Mathf.Lerp(soundInitialPitch, soundMaxPitch, st);
            }
        }
        else
        {

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

        IsPerformingHeavy = true;

        if (hitbox != null) hitbox.damage = 3;
        if (attackCollider != null) attackCollider.enabled = true;
        if (hitbox != null) hitbox.canHit = true;


        yield return new WaitForSeconds(0.5f);


        if (attackCollider != null) attackCollider.enabled = false;
        if (hitbox != null) hitbox.canHit = false;


        IsPerformingHeavy = false;


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

        if (lightLoopSource != null && lightLoopSource.isPlaying)
        {
            lightLoopSource.Stop();
            lightLoopSource.pitch = soundInitialPitch;
        }

 
        if (animator != null)
        {
            animator.SetBool("LightHold", false);
            animator.SetFloat("LightSpeed", 1f);
            animator.ResetTrigger("LightRelease");
        }

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
