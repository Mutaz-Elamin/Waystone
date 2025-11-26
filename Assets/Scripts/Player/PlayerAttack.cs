using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public SwordHitbox swordHitbox;

    public float attackActiveTime = 0.25f;
    public float attackCooldown = 0.5f;

    private bool canAttack = true;

    public void LightAttack()
    {
        if (!canAttack) return;
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        animator.SetTrigger("LightAttack");

        // Enable hitbox
        swordHitbox.canHit = true;
        swordHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackActiveTime);

        // Disable hitbox
        swordHitbox.canHit = false;
        swordHitbox.gameObject.SetActive(false);

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    //public void HeavyAttack()

    // { if (Time.time - lastHeavyTime < heavyCooldown) return;

    //lastHeavyTime = Time.time;

    //  animator.SetTrigger("HeavyAttack");
    //    swordHitbox.canHit = true;}

}

