using UnityEngine;

public class ClubHitbox : MonoBehaviour
{
    [HideInInspector] public bool canHit = false;
    [HideInInspector] public int damage = 1;

    [Header("References")]
    public WeaponSFX sfx; // optional (auto-resolve)
    public GameObject bloodPrefab; // optional
    [SerializeField] private EnemiesOnHit enemiesOnHit; // optional helper

    private void Awake()
    {
        sfx ??= GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        canHit = false; // single hit semantics

        // capture hit point before knockback moves the enemy
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // apply damage
        other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

        // determine weapon/attack context if needed
        Club club = GetComponentInParent<Club>();
        Club.AttackType dummy = Club.AttackType.None; // not used, kept for clarity
        // play sfx depending on damage (consistent with previous patterns)
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();

        if (damage == club?.lightDamage)
        {
            sfx?.Club_Light1HitPlay();
        }
        else if (damage == club?.windupDamage)
        {
            sfx?.Club_HeavyHit1Play();
        }
        else if (damage == club?.slamDamage)
        {
            sfx?.Club_HeavyHit2Play();
        }
        else
        {
            // fallback
            sfx?.Club_Light1HitPlay();
        }

        // hit stop
        enemiesOnHit?.ApplyHitStop(this, 0.08f);

        // flash enemy
        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            // choose colors based on hit strength
            Color first = Color.white;
            Color second = (damage >= (club?.slamDamage ?? 4)) ? Color.black : Color.gray;
            if (club != null)
                club.StartCoroutine(enemiesOnHit.FlashEnemy(rend, first, second, 0.15f));
        }

        // knockback - heavier for slam
        float kb = (club != null && club.currentAttack == Club.AttackType.Heavy && damage >= (club?.slamDamage ?? 4)) ? 4f : 1.5f;
        enemiesOnHit?.ApplyKnockback(other, transform, kb);

        // Spawn blood at hitPoint, scale by enemy size, adjust particle safely
        if (bloodPrefab != null)
        {
            GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);
            blood.transform.LookAt(hitPoint + (hitPoint - other.transform.position));

            float scale = Mathf.Clamp(other.transform.localScale.magnitude / 3f, 0.1f, 3f);
            blood.transform.localScale = Vector3.one * scale;

            ParticleSystem ps = blood.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = ps.main;

                // safe lifetime/duration adjustments
                float wantedDuration = 0.2f + (damage * 0.05f);
                main.duration = Mathf.Clamp(wantedDuration, 0.05f, 2f);
                float lifetimeMax = main.startLifetime.constantMax;
                main.startLifetime = Mathf.Clamp(lifetimeMax * scale, 0.05f, 0.8f);

                ps.Play();
                Destroy(blood, main.duration + main.startLifetime.constantMax + 0.05f);
            }
            else
            {
                Destroy(blood, 0.5f);
            }
        }
    }
}