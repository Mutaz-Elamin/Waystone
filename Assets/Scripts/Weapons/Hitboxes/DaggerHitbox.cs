using UnityEngine;

public class DaggerHitbox : MonoBehaviour
{
    [HideInInspector] public bool canHit = false;
    public WeaponSFX sfx;
    public GameObject bloodPrefab;

    [SerializeField] private EnemiesOnHit enemiesOnHit;

    private void Awake()
    {
        enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
        sfx ??= GetComponentInParent<WeaponSFX>();
    }

    private void OnTriggerEnter(Collider other)
    {

        HealthBasedAsset asset = other.GetComponentInParent<HealthBasedAsset>();
        if (asset != null)
        {
            asset.TakeDamage(2, DamageCause.PlayerAttack);
        }

        if (!canHit || !other.CompareTag("npc")) return;
        canHit = false;

        Dagger dagger = GetComponentInParent<Dagger>();
        if (dagger == null) return;

        GeneralNPC npc = other.GetComponent<GeneralNPC>();
        if (npc != null)
        {
            if (dagger != null)
            {
                int damage = dagger.CalculateDamage();
                npc.TakeDamage(damage, DamageCause.EnemyAttack);
            }
        }

        // Play hit SFX depending on combo step
        switch (dagger.comboStep)
        {
            case 1: sfx?.Dagger_Light1HitPlay(); break;
            case 2: sfx?.Dagger_Light2HitPlay(); break;
            case 3: sfx?.Dagger_Light3HitPlay(); break;
            case 4: sfx?.Dagger_Light4HitPlay(); break;
            default: sfx?.Dagger_Light1HitPlay(); break;
        }

        // HITSTOP
        enemiesOnHit?.ApplyHitStop(dagger, 0.08f);

        // FLASH
        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
            dagger.StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.red, Color.black, 0.15f));

        // KNOCKBACK
        enemiesOnHit?.ApplyKnockback(other, transform, 1f);

        // BLOOD
        SpawnBlood(other, dagger);
    }

    private void SpawnBlood(Collider enemyCollider, Dagger dagger)
    {
        if (bloodPrefab == null) return;

        // Use the closest point to the weapon on the enemy collider
        Vector3 hitPoint = enemyCollider.ClosestPoint(transform.position);

        GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);

        // Orient the blood away from the enemy center
        Vector3 direction = (hitPoint - enemyCollider.transform.position).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = transform.forward;
        blood.transform.rotation = Quaternion.LookRotation(direction);

        // Scale blood by enemy size
        float scale = enemyCollider.transform.localScale.magnitude / 3f;
        blood.transform.localScale = Vector3.one * scale;

        // Destroy particle after its lifetime
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