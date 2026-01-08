using UnityEngine;

public class ClubHitbox : MonoBehaviour
{
    [HideInInspector] public bool canHit = false;
    [HideInInspector] public int damage = 1;

    [Header("References")]
    public WeaponSFX sfx; 
    public GameObject bloodPrefab;
    [SerializeField] private EnemiesOnHit enemiesOnHit; 

    private void Awake()
    {
        sfx ??= GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        canHit = false; 

 
        Vector3 hitPoint = other.ClosestPoint(transform.position);


        other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);


        Club club = GetComponentInParent<Club>();
        Club.AttackType dummy = Club.AttackType.None;
  
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

            sfx?.Club_Light1HitPlay();
        }


        enemiesOnHit?.ApplyHitStop(this, 0.08f);


        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
        {

            Color first = Color.white;
            Color second = (damage >= (club?.slamDamage ?? 4)) ? Color.black : Color.gray;
            if (club != null)
                club.StartCoroutine(enemiesOnHit.FlashEnemy(rend, first, second, 0.15f));
        }


        float kb = (club != null && club.currentAttack == Club.AttackType.Heavy && damage >= (club?.slamDamage ?? 4)) ? 4f : 1.5f;
        enemiesOnHit?.ApplyKnockback(other, transform, kb);

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