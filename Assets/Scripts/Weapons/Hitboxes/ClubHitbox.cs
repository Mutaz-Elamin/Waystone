using UnityEngine;

public class ClubHitbox : MonoBehaviour
{
    [HideInInspector]
    public int damage;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("npc")) return;

        other.GetComponent<GeneralNPC>()
            ?.TakeDamage(damage, DamageCause.EnemyAttack);
    }
}