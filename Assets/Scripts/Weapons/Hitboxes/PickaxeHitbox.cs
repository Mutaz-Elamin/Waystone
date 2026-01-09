using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PickaxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Pickaxe Damage Settings")]
    public int damage = 1;

    [Header("References")]
    public WeaponSFX sfx;             
    public GameObject bloodPrefab;  
    [SerializeField] private EnemiesOnHit enemiesOnHit; 

    [Header("Hit cadence / timing (fallback)")]
    public float fallbackHitInterval = 0.25f; 


    private Dictionary<int, float> lastHitTimes = new Dictionary<int, float>();

    private void Awake()
    {

        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }


    private float forcedHitInterval = -1f;
    public void SetHitInterval(float seconds)
    {
        forcedHitInterval = seconds;
    }


    public void ClearLastHitTimes()
    {
        lastHitTimes.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        Pickaxe pick = GetComponentInParent<Pickaxe>();
        if (pick != null && pick.IsPerformingHeavy)
        {
            int id = other.gameObject.GetInstanceID();
            float last = lastHitTimes.ContainsKey(id) ? lastHitTimes[id] : -999f;
            if (Time.time - last < 0.01f) return; 
            lastHitTimes[id] = Time.time;

            RegisterHit(other, pick);
        }

    }

    private void OnTriggerStay(Collider other)
    {

        HealthBasedAsset asset = other.GetComponentInParent<HealthBasedAsset>();
        if (asset != null)
        {
            asset.TakeDamage(damage, DamageCause.PlayerPickaxe);
        }

        if (!canHit) return;

        Pickaxe pick = GetComponentInParent<Pickaxe>();
        float hitInterval = forcedHitInterval > 0f ? forcedHitInterval : fallbackHitInterval;
        float holdElapsed = pick != null ? pick.HoldElapsed : Mathf.Infinity;
        float startupDelay = pick != null ? pick.startupDelay : 0f;

        int id = other.gameObject.GetInstanceID();
        float last = lastHitTimes.ContainsKey(id) ? lastHitTimes[id] : -999f;


        if (holdElapsed < startupDelay) return;

        if (Time.time - last < hitInterval) return;

        lastHitTimes[id] = Time.time;

        RegisterHit(other, pick);
    }

    private void RegisterHit(Collider other, Pickaxe pick)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        GeneralNPC npc = other.GetComponent<GeneralNPC>();
        if (npc == null) return;


        bool isHeavy = pick != null && pick.IsPerformingHeavy;

        int applyDamage = damage;
        if (pick != null)
        {

            applyDamage = pick.CalculateDamage(isHeavy);
        }

        npc.TakeDamage(applyDamage, DamageCause.EnemyAttack);


        (sfx ??= GetComponentInParent<WeaponSFX>())?.Pickaxe_StoneHitPlay();


        enemiesOnHit?.ApplyHitStop(this, 0.06f);


        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null && pick != null)
            pick.StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.white, Color.gray, 0.12f));


        enemiesOnHit?.ApplyKnockback(other, transform, 2f);


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

                float wantedDuration = Mathf.Clamp(pick != null ? pick.HoldElapsed : 0.1f, 0.05f, 2.5f);
                main.duration = wantedDuration;

                float lifetimeMax = main.startLifetime.constantMax;
                main.startLifetime = Mathf.Clamp(lifetimeMax * scale, 0.05f, 0.6f);

                ps.Play();
                Destroy(blood, (main.duration + main.startLifetime.constantMax + 0.05f)/ 3)  ;
            }
            else
            {
                Destroy(blood, Mathf.Clamp(pick != null ? pick.HoldElapsed : 0.1f, 0.05f, 2.5f));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int id = other.gameObject.GetInstanceID();
        if (lastHitTimes.ContainsKey(id))
            lastHitTimes.Remove(id);
    }
}
