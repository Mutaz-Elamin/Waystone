using UnityEngine;
using UnityEngine.InputSystem.HID;

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

        HealthBasedAsset asset = other.GetComponentInParent<HealthBasedAsset>();
        if (asset != null)
        {
            asset.TakeDamage(damage, DamageCause.PlayerAttack);
        }

        HealthBasedAsset npc = other.GetComponent<HealthBasedAsset>();
        if (npc != null)
        {
            Stick stick = GetComponentInParent<Stick>();
            bool isHeavy = stick.currentAttack == Stick.AttackType.Heavy;
            int damage = stick.CalculateDamage(isHeavy);
            npc.TakeDamage(damage, DamageCause.PlayerAttack);
        }
        if (sfx != null)
        {
            if (damage == 1) sfx.Stick_LightHitPlay();
            else sfx.Stick_HeavyHitPlay();
        }


        if (enemiesOnHit != null)
        {
            enemiesOnHit.ApplyHitStop(GetComponentInParent<Weapon>(), 0.08f);

            Renderer rend = other.GetComponentInChildren<Renderer>();
            if (rend != null)
                StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.white, Color.gray, 0.15f));

    
            enemiesOnHit.ApplyKnockback(other, transform, damage == 2 ? 1f : 0.5f);
        }
    }
}