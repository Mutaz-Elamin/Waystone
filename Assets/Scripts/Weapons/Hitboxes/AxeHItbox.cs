using System.Collections;
using UnityEngine;

public class AxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Axe Damage Settings")]
    public int damage = 2; // light attack damage by default

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Axe hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);
        }

        canHit = false; // only hit once per swing
    }
}

