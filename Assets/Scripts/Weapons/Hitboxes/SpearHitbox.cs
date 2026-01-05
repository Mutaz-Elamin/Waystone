using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Spear Damage Settings")]
    public int damage = 2;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Spear hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

            // Play hit SFX depending on combo step
            Spear spear = GetComponentInParent<Spear>();
            if (spear != null)
            {
                switch (spear.ComboStep)
                {
                    case 1: sfx?.Spear_Light1HitPlay(); break;
                    case 2: sfx?.Spear_Light2HitPlay(); break;
                    default: sfx?.Spear_HeavyHitPlay(); break;
                }
            }
        }

        canHit = false;
    }
}