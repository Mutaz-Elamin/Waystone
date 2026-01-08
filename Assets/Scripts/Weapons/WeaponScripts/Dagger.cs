using System.Collections;
using UnityEngine;

public class Dagger : Weapon
{
    public enum AttackType { None, Light }

    [Header("Dagger Settings")]
    public float comboResetTime = 1f;
    public int comboStep = 0;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isDefending = false;
    private float lastAttackTime;

    [Header("References")]
    public WeaponSFX sfx;
    public GameObject bloodPrefab;

    [Header("State")]
    public AttackType currentAttack = AttackType.None;

    private DaggerHitbox hitbox;

    private void Awake()
    {
        if (attackCollider != null)
        {
            hitbox = attackCollider.GetComponent<DaggerHitbox>();
            attackCollider.enabled = false;
            if (hitbox != null)
                hitbox.canHit = false;
        }
        else
        {
            Debug.LogError("Dagger: AttackCollider is not assigned!");
        }

        sfx ??= GetComponentInParent<WeaponSFX>();
    }

    // ---------------- LIGHT ATTACK ----------------
    public override void LightAttack()
    {
        if (!canAttack || isAttacking || isDefending) return;

        currentAttack = AttackType.Light;
        canAttack = false;
        isAttacking = true;

        float timeSinceLast = Time.time - lastAttackTime;
        if (timeSinceLast > comboResetTime) comboStep = 0;

        comboStep++;
        ResetAllAttackTriggers();

        switch (comboStep)
        {
            case 1: animator.SetTrigger("LightAttack1"); sfx?.Dagger_Light1SwingPlay(); break;
            case 2: animator.SetTrigger("LightAttack2"); sfx?.Dagger_Light2SwingPlay(); break;
            case 3: animator.SetTrigger("LightAttack3"); sfx?.Dagger_Light3SwingPlay(); break;
            case 4: animator.SetTrigger("LightAttack4"); sfx?.Dagger_Light4SwingPlay(); break;
            default: comboStep = 1; animator.SetTrigger("LightAttack1"); sfx?.Dagger_Light1SwingPlay(); break;
        }

        StartCoroutine(LightAttackRoutine());
        lastAttackTime = Time.time;
    }

    private IEnumerator LightAttackRoutine()
    {
        if (hitbox != null) hitbox.canHit = true;
        yield return new WaitForSeconds(0.55f);
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.15f); // sync with animation
        attackCollider.enabled = false;
        if (hitbox != null) hitbox.canHit = false;

        // Optional: spawn blood when hit happens inside DaggerHitbox
        // Blood handled there, no need to do it here

        currentAttack = AttackType.None;
        yield return new WaitForSeconds(0.05f); // recovery buffer
        canAttack = true;
        isAttacking = false;
    }

    // ---------------- DEFENSE ----------------
    public override void StartDefend()
    {
        isDefending = true;
        animator.SetBool("IsDefending", true);
        sfx?.Dagger_DefendPlay();
    }

    public override void StopDefend()
    {
        isDefending = false;
        animator.SetBool("IsDefending", false);
    }

    // ---------------- HELPERS ----------------
    private void ResetAllAttackTriggers()
    {
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
        animator.ResetTrigger("LightAttack4");
    }

    public override void ResetWeapon()
    {
        comboStep = 0;
        currentAttack = AttackType.None;

        ResetAllAttackTriggers();
        animator.SetBool("IsDefending", false);

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitbox != null)
            hitbox.canHit = false;

        canAttack = true;
        isAttacking = false;
        isDefending = false;
    }
}