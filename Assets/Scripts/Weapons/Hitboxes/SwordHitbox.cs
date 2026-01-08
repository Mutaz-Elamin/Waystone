using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [HideInInspector] public bool canHit = false;

    [Header("References")]
    private WeaponSFX sfx;
    public GameObject bloodPrefab;
    [SerializeField] private EnemiesOnHit enemiesOnHit;

    private void Awake()
    {
        // Find WeaponSFX on player / parent / root
        sfx = GetComponentInParent<WeaponSFX>();
        enemiesOnHit = GetComponentInParent<EnemiesOnHit>();


        if (sfx == null)
            Debug.LogWarning("WeaponSFX not found for SwordHitbox", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        canHit = false;

        Sword sword = GetComponentInParent<Sword>();
        if (sword == null) return;

        Debug.Log("Sword hit: " + other.name);


        GeneralNPC npc = other.GetComponent<GeneralNPC>();
        if (npc != null)
        {
            if (sword != null)
            {
                bool isHeavy = sword.currentAttack == Sword.AttackType.Heavy;
                int damage = sword.CalculateDamage(isHeavy);
                npc.TakeDamage(damage, DamageCause.EnemyAttack);
            }
        }



        if (sword.currentAttack == Sword.AttackType.Light)
        {
            switch (sword.comboStep)
            {
                case 1: sfx?.Sword_Light1HitPlay(); break;
                case 2: sfx?.Sword_Light2HitPlay(); break;
                case 3: sfx?.Sword_Light3HitPlay(); break;
            }
        }
        else if (sword.currentAttack == Sword.AttackType.Heavy)
        {
            sfx?.Sword_HeavyHitPlay();
        }


        enemiesOnHit?.ApplyHitStop(this, 0.08f);


        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null)
            StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.red, Color.black, 0.18f));


        enemiesOnHit?.ApplyKnockback(other, transform,
            sword.currentAttack == Sword.AttackType.Heavy ? 4f : 2f);


        SpawnBlood(other.transform);
    }

    private void SpawnBlood(Transform enemy)
    {
        if (bloodPrefab == null) return;

        GameObject blood = Instantiate(bloodPrefab, transform.position, Quaternion.identity);
        blood.transform.LookAt(enemy.position);

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