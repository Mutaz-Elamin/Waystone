using UnityEngine;

public class SpearHitbox : MonoBehaviour
{
    [HideInInspector] public bool canHit = false;

    [Header("References")]
    public GameObject bloodPrefab;

    private WeaponSFX sfx;
    private EnemiesOnHit enemiesOnHit;

    private void Awake()
    {
        sfx = GetComponentInParent<WeaponSFX>();
        enemiesOnHit = GetComponentInParent<EnemiesOnHit>();

        if (sfx == null)
            Debug.LogWarning("WeaponSFX not found for SpearHitbox", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        canHit = false;

        Spear spear = GetComponentInParent<Spear>();
        if (spear == null) return;

        Debug.Log("Spear hit: " + other.name);

        // DAMAGE
        other.GetComponent<GeneralNPC>()?.TakeDamage(1, DamageCause.EnemyAttack);

        // HIT SFX (ONLY ON HIT)
        if (spear.currentAttack == Spear.AttackType.Light)
        {
            switch (spear.comboStep)
            {
                case 1: sfx?.Spear_Light1HitPlay(); break;
                case 2: sfx?.Spear_Light2HitPlay(); break;
            }
        }
        else if (spear.currentAttack == Spear.AttackType.Heavy)
        {
            sfx?.Spear_HeavyHitPlay();
        }


        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // HIT STOP
        enemiesOnHit?.ApplyHitStop(this, 0.08f);

        // FLASH
        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
            StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.red, Color.black, 0.18f));

        // KNOCKBACK (spear = more forward force)
        enemiesOnHit?.ApplyKnockback(
            other,
            transform,
            spear.currentAttack == Spear.AttackType.Heavy ? 5f : 3f
        );

        // BLOOD
        SpawnBlood(hitPoint, other.transform);

    }

    private void SpawnBlood(Vector3 hitPoint, Transform enemy)
    {
        if (bloodPrefab == null) return;

        GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);

        blood.transform.LookAt(hitPoint + (hitPoint - enemy.position));

        float scale = enemy.localScale.magnitude / 3f;
        blood.transform.localScale = Vector3.one * scale;

        ParticleSystem ps = blood.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startLifetime = Mathf.Clamp(main.startLifetime.constant * scale, 0.1f, 0.5f);
            Destroy(blood, (main.duration + main.startLifetime.constantMax) * 0.5f);
        }
        else
        {
            Destroy(blood, 0.5f);
        }
    }
}