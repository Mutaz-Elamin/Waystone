using UnityEngine;

public class AxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Axe Damage Settings")]
    public int damage = 2; // light attack damage by default

    [Header("References")]
    public WeaponSFX sfx;           // will try to auto-resolve in Awake
    public GameObject bloodPrefab;  // optional
    [SerializeField] private EnemiesOnHit enemiesOnHit;

    private void Awake()
    {
        // Attempt to find references up the hierarchy so prefab issues are less brittle
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();

        if (sfx == null)
            Debug.LogWarning("AxeHitbox: WeaponSFX not found in parents.", this);
        if (enemiesOnHit == null)
            Debug.LogWarning("AxeHitbox: EnemiesOnHit not found in parents.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        // Prevent double hits in this swing
        canHit = false;

        Axe axe = GetComponentInParent<Axe>();
        if (axe == null)
        {
            Debug.LogWarning("AxeHitbox: parent Axe not found.", this);
            return;
        }

        Debug.Log("Axe hit: " + other.gameObject.name);

        // apply damage
        other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

        // play hit SFX based on attack type
        if (axe.currentAttack == Axe.AttackType.Light)
        {
            // choose hit SFX based on damage or combo if you prefer - we pick based on damage as default
            if (damage == axe.lightDamage) sfx?.Axe_Light1HitPlay();
            else sfx?.Axe_Light1HitPlay();
        }
        else if (axe.currentAttack == Axe.AttackType.Heavy)
        {
            sfx?.Axe_HeavyHitPlay();
        }
        else
        {
            // fallback
            sfx?.Axe_Light1HitPlay();
        }

        // Compute precise hit point on the collider to spawn particles there (avoids post-knockback mismatch)
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // Hit stop
        enemiesOnHit?.ApplyHitStop(this, 0.08f);

        // Flash the enemy mesh
        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
            StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.red, Color.black, 0.18f));

        // Knockback (axe heavier push)
        float kbForce = (axe.currentAttack == Axe.AttackType.Heavy) ? 2f : 1f;
        enemiesOnHit?.ApplyKnockback(other, transform, kbForce);

        // Spawn blood at the exact contact point so knockback doesn't move it
        SpawnBloodAtPoint(hitPoint, other.transform);
    }

    private void SpawnBloodAtPoint(Vector3 hitPoint, Transform enemy)
    {
        if (bloodPrefab == null) return;

        GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);

        // orient blood to spray away from enemy center (approx)
        blood.transform.LookAt(hitPoint + (hitPoint - enemy.position));

        float scale = enemy.localScale.magnitude / 3f;
        blood.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        ParticleSystem ps = blood.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            // scale down lifetime and clamp to short durations
            main.startLifetime = Mathf.Clamp(main.startLifetime.constant * scale, 0.05f, 0.6f);
            Destroy(blood, (main.duration + main.startLifetime.constantMax) * 0.5f);
        }
        else
        {
            Destroy(blood, 0.5f);
        }
    }
}
