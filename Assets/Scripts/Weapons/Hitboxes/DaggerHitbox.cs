using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Dagger Damage Settings")]
    public int damage = 1; // weaker than spear

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Dagger hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

            // Play hit SFX depending on combo step
            Dagger dagger = GetComponentInParent<Dagger>();
            if (dagger != null)
            {
                switch (dagger.ComboStep)
                {
                    case 1: sfx?.Dagger_Light1HitPlay(); break;
                    case 2: sfx?.Dagger_Light2HitPlay(); break;
                    case 3: sfx?.Dagger_Light3HitPlay(); break;
                    case 4: sfx?.Dagger_Light4HitPlay(); break;
                    default: sfx?.Dagger_Light1HitPlay(); break;
                }
            }
        }

        canHit = false;
    }
}