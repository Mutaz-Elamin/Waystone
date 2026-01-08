using UnityEngine;
using System.Collections;

public class Stick : Weapon
{
    public enum AttackType { None, Light, Heavy }

    [Header("Timing")]
    public float lightDuration = 0.3f;
    public float heavyDuration = 0.5f;
    public float recoveryTime = 0.4f;

    [Header("Damage")]
    public int lightDamage = 1;
    public int heavyDamage = 2;

    [Header("State")]
    public AttackType currentAttack = AttackType.None;

    [Header("References")]
    public WeaponSFX sfx;

    private bool canAttack = true;
    private bool isDefending = false;
    private bool isAttacking = false;

    private StickHitbox hitbox;

    private void Awake()
    {
        if (attackCollider != null)
        {
            hitbox = attackCollider.GetComponent<StickHitbox>();
            attackCollider.enabled = false;
            if (hitbox != null) hitbox.canHit = false;
        }
        else
        {
            Debug.LogError("Stick: AttackCollider is not assigned!");
        }

        sfx ??= GetComponentInParent<WeaponSFX>();
    }

    // ---------------- LIGHT ATTACK ----------------
    public override void LightAttack()
    {
        if (!canAttack || isDefending || isAttacking) return; 

        currentAttack = AttackType.Light;
        canAttack = false;
        isAttacking = true;

        animator.SetTrigger("LightAttack");
        StartCoroutine(LightRoutine());
    }


    private IEnumerator LightRoutine()
    {
        if (hitbox != null) hitbox.damage = lightDamage;

        // Align hit with animation (adjust delay as needed)
        yield return new WaitForSeconds(0.55f);

        sfx?.Stick_Light1SwingPlay();

        EnableHitbox(true);
        yield return new WaitForSeconds(lightDuration);
        EnableHitbox(false);

        currentAttack = AttackType.None;
        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
        isAttacking = false;
    }

    // ---------------- HEAVY ATTACK ----------------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.SetTrigger("HeavyAttack");
        StartCoroutine(HeavyRoutine());
    }

    private IEnumerator HeavyRoutine()
    {
        if (hitbox != null) hitbox.damage = heavyDamage;

        // First hit
        yield return new WaitForSeconds(0.15f);
        sfx?.Stick_HeavySwingPlay();
        EnableHitbox(true);
        yield return new WaitForSeconds(heavyDuration / 2f);
        EnableHitbox(false);

        // Gap between hits
        yield return new WaitForSeconds(0.7f);

        // Second hit
        sfx?.Stick_HeavySwingPlay();
        EnableHitbox(true);
        yield return new WaitForSeconds(heavyDuration / 2f);
        EnableHitbox(false);

        currentAttack = AttackType.None;
        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }

    // ---------------- DEFENSE ----------------
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

    // ---------------- HELPERS ----------------
    private void EnableHitbox(bool enabled)
    {
        if (attackCollider != null)
            attackCollider.enabled = enabled;

        if (hitbox != null)
            hitbox.canHit = enabled;
    }

    public override void ResetWeapon()
    {
        animator.ResetTrigger("LightAttack");
        animator.ResetTrigger("HeavyAttack");
        animator.SetBool("IsDefending", false);

        EnableHitbox(false);

        canAttack = true;
        isDefending = false;
        currentAttack = AttackType.None;
    }
}