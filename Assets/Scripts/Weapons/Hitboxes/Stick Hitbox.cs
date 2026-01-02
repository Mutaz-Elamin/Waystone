using UnityEngine;

public class StickHitbox : MonoBehaviour
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
