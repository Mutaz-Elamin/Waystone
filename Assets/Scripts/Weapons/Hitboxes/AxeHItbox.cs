using System;
using UnityEngine;

public class AxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Axe Damage Settings")]
    public int damage = 2;

    [Header("References")]
    public WeaponSFX sfx;     
    public GameObject bloodPrefab; 
    [SerializeField] private EnemiesOnHit enemiesOnHit;

    private void Awake()
    {

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

        canHit = false;

        Axe axe = GetComponentInParent<Axe>();
        if (axe == null)
        {
            Debug.LogWarning("AxeHitbox: parent Axe not found.", this);
            return;
        }

        HealthBasedAsset npc = other.GetComponent<HealthBasedAsset>();
        if (npc != null)
        {
            if (axe != null)
            {
                bool isHeavy = axe.currentAttack == Axe.AttackType.Heavy;
                int damage = axe.CalculateDamage(isHeavy);
                npc.TakeDamage(damage, DamageCause.PlayerAxe);
            }
        }


        if (axe.currentAttack == Axe.AttackType.Light)
        {

            if (damage == axe.lightDamage) sfx?.Axe_Light1HitPlay();
            else sfx?.Axe_Light1HitPlay();
        }
        else if (axe.currentAttack == Axe.AttackType.Heavy)
        {
            sfx?.Axe_HeavyHitPlay();
        }
        else
        {

            sfx?.Axe_Light1HitPlay();
        }


        Vector3 hitPoint = other.ClosestPoint(transform.position);

   
        enemiesOnHit?.ApplyHitStop(this, 0.08f);


        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
            StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.red, Color.black, 0.18f));


        float kbForce = (axe.currentAttack == Axe.AttackType.Heavy) ? 2f : 1f;
        enemiesOnHit?.ApplyKnockback(other, transform, kbForce);

        SpawnBloodAtPoint(hitPoint, other.transform);
    }

    private void SpawnBloodAtPoint(Vector3 hitPoint, Transform enemy)
    {
        if (bloodPrefab == null) return;

        GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);


        blood.transform.LookAt(hitPoint + (hitPoint - enemy.position));

        float scale = enemy.localScale.magnitude / 3f;
        blood.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        ParticleSystem ps = blood.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startLifetime = Mathf.Clamp(main.startLifetime.constant * scale, 0.05f, 0.6f);
            Destroy(blood, (main.duration + main.startLifetime.constantMax) * 0.5f);
        }
        else
        {
            Destroy(blood, 0.5f);
        }
    }
}
