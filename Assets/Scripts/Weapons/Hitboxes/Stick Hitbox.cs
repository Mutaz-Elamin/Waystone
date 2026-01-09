using UnityEngine;

public class StickHitbox : MonoBehaviour
{
    [HideInInspector] public int damage;
    [HideInInspector] public bool canHit = false;

    [SerializeField] private EnemiesOnHit enemiesOnHit;
    private WeaponSFX sfx;

    private void Awake()
    {
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- IMPORTANT: only handle hits when allowed ---
        if (!canHit) return;

        // prevent re-entry until next attack window
        canHit = false;

        // find the target
        HealthBasedAsset npc = other.GetComponentInParent<HealthBasedAsset>();
        if (npc != null)
        {
            Stick stick = GetComponentInParent<Stick>();
            if (stick != null)
            {
                bool isHeavy = stick.currentAttack == Stick.AttackType.Heavy;
                int dmg = stick.CalculateDamage(isHeavy);
                npc.TakeDamage(dmg, DamageCause.PlayerAttack);
                damage = dmg; 
            }
        }


        if (sfx != null)
        {
            if (damage == 1) sfx.Stick_LightHitPlay();
            else sfx.Stick_HeavyHitPlay();
        }

        if (enemiesOnHit != null)
        {

            enemiesOnHit.ApplyHitStop(this, 0.08f);

            Renderer rend = other.GetComponentInChildren<Renderer>();
            if (rend != null)
                StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.white, Color.gray, 0.15f));

            enemiesOnHit.ApplyKnockback(other, transform, (damage == 2) ? 1f : 0.5f);
        }
    }
}