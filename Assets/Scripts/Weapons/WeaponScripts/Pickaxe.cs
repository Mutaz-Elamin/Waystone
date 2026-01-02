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

    private void Awake()
    {
        hitbox = attackCollider.GetComponent<PickaxeHitbox>();
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
    }

    public override void StopLightAttack()
    {
        if (!isHoldingLight) return;

        isHoldingLight = false;
        attackCollider.enabled = false;

        animator.SetBool("LightHold", false);   
        animator.SetTrigger("LightRelease");    
    }

    private void Update()
    {
        if (isHoldingLight)
        {
            float elapsed = Time.time - holdStartTime;
            float t = Mathf.Clamp01(elapsed / lightRampTime);
            float speed = Mathf.Lerp(lightInitialSpeed, lightMaxSpeed, t);
            animator.SetFloat("LightSpeed", speed);
        }
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
        hitbox.damage = 3; // heavy damage
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.5f); // adjust for your animation length
        attackCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);
        canAttack = true;
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
}