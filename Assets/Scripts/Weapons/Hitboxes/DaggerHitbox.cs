using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Dagger Damage Settings")]
    public int damage = 1; // weaker than spear

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Dagger hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);
        }

        canHit = false;
    }
}