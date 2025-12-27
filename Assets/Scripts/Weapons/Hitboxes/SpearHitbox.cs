using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Spear Damage Settings")]
    public int damage = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Spear hit: " + other.gameObject.name);

            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);
        }

        canHit = false; 
    }
}
