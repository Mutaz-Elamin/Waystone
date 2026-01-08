using UnityEngine;

public class StickHitbox : MonoBehaviour
{
    [HideInInspector] public int damage;
    [HideInInspector] public bool canHit = false;

    [SerializeField] private EnemiesOnHit enemiesOnHit;
    private WeaponSFX sfx;

    private void Awake()
    {
        // Find WeaponSFX and EnemiesOnHit in parent if not assigned
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit || !other.CompareTag("npc")) return;
        canHit = false;

        // Damage
        other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

        // Play SFX
        if (sfx != null)
        {
            if (damage == 1) sfx.Stick_LightHitPlay();
            else sfx.Stick_HeavyHitPlay();
        }

        // Hitstop & flash
        if (enemiesOnHit != null)
        {
            enemiesOnHit.ApplyHitStop(GetComponentInParent<Weapon>(), 0.08f);

            Renderer rend = other.GetComponentInChildren<Renderer>();
            if (rend != null)
                StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.white, Color.gray, 0.15f));

            // Knockback
            enemiesOnHit.ApplyKnockback(other, transform, damage == 2 ? 1f : 0.5f);
        }
    }
}