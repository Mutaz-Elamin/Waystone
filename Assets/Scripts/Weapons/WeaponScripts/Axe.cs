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

        if (comboStep == 1) animator.SetTrigger("LightAttack1");
        else if (comboStep == 2) animator.SetTrigger("LightAttack2");
        else if (comboStep == 3) animator.SetTrigger("LightAttack3");
        else
        {
            comboStep = 1;
            animator.speed = baseAttackSpeed;
            animator.SetTrigger("LightAttack1");
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

        StartCoroutine(HeavyAttackRoutine());
    }

    public override void ReleaseHeavyAttack()
    {
       
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

    private IEnumerator LightAttackRoutine(float attackSpeed)
    {
        canAttack = false;

        hitbox.damage = lightDamage;
        attackCollider.enabled = true;

        yield return new WaitForSeconds(lightDuration / attackSpeed);

        attackCollider.enabled = false;

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
}
