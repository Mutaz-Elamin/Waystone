using UnityEngine;

public class PickaxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Pickaxe Damage Settings")]
    public int damage = 1;

    public WeaponSFX sfx; // assign same WeaponSFX as Pickaxe

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Pickaxe hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

            // Play hit sound effect
            sfx?.Pickaxe_StoneHitPlay();
        }

        canHit = false;
    }
}