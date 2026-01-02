using UnityEngine;
using System.Collections;

public class Stick : Weapon
{
    [Header("Timing")]
    public float lightDuration = 0.3f;
    public float heavyDuration = 0.5f;
    public float recoveryTime = 0.4f;

    [Header("Damage")]
    public int lightDamage = 1;
    public int heavyDamage = 2;

    private bool canAttack = true;
    private bool isDefending = false;

    private StickHitbox hitbox;

    private void Awake()
    {
        hitbox = attackCollider.GetComponent<StickHitbox>();
    }

    // -------- LIGHT ATTACK --------
    public override void LightAttack()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.SetTrigger("LightAttack");
        StartCoroutine(LightRoutine());
    }

    private IEnumerator LightRoutine()
    {
        hitbox.damage = lightDamage;
        yield return new WaitForSeconds(0.1f);

        attackCollider.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        attackCollider.enabled = false;

        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }

    // -------- HEAVY ATTACK --------
    public override void StartHeavyCharge()
    {
        if (!canAttack || isDefending) return;

        canAttack = false;
        animator.SetTrigger("HeavyAttack");
        StartCoroutine(HeavyRoutine());
    }

    private IEnumerator HeavyRoutine()
    {
        hitbox.damage = heavyDamage;
        yield return new WaitForSeconds(0.15f);

        attackCollider.enabled = true;
        yield return new WaitForSeconds(heavyDuration);
        attackCollider.enabled = false;

        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }

    // -------- DEFEND --------
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
}