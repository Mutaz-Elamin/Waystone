using System.Collections;
using UnityEngine;

public class AxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Axe Damage Settings")]
    public int damage = 2; // light attack damage by default

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Axe hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

            // play hit sound based on damage
            if (damage == 2) sfx?.Axe_Light1HitPlay();
            else if (damage == 5) sfx?.Axe_HeavyHitPlay();
        }

        canHit = false; // only hit once per swing
    }
}
