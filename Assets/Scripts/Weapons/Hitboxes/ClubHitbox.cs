using UnityEngine;

public class ClubHitbox : MonoBehaviour
{
    [HideInInspector]
    public int damage;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("npc")) return;

        other.GetComponent<GeneralNPC>()
            ?.TakeDamage(damage, DamageCause.EnemyAttack);

        // Play hit sound based on damage type
        if (damage == 1) sfx?.Club_Light1HitPlay();
        else if (damage == 2) sfx?.Club_HeavyHit1Play();
        else if (damage >= 4) sfx?.Club_HeavyHit2Play();
    }
}
