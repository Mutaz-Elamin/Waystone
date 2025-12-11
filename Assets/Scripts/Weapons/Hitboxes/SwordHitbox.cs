using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SwordHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("npc"))
        {
            Debug.Log("Test NPC Attack Hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(1, DamageCause.EnemyAttack);
        }

        canHit = false;
    }
}
