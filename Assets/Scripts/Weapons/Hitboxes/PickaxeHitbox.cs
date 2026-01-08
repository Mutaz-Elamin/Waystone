using UnityEngine;
using System.Collections.Generic;

public class PickaxeHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("Pickaxe Damage Settings")]
    public int damage = 1;

    [Header("References")]
    public WeaponSFX sfx;             // optional: will try to auto-find in Awake
    public GameObject bloodPrefab;    // optional
    [SerializeField] private EnemiesOnHit enemiesOnHit; // optional helper

    [Header("Hit cadence / timing (fallback)")]
    public float fallbackHitInterval = 0.25f; // used if pickaxe not found

    // per-collider last hit time so each enemy has its own cadence
    private Dictionary<int, float> lastHitTimes = new Dictionary<int, float>();

    private void Awake()
    {
        // try to auto-resolve references so prefab->player assignment is less fragile
        if (sfx == null) sfx = GetComponentInParent<WeaponSFX>();
        if (enemiesOnHit == null) enemiesOnHit = GetComponentInParent<EnemiesOnHit>();
    }

    // allow pickaxe to tell us the desired rate while holding
    private float forcedHitInterval = -1f;
    public void SetHitInterval(float seconds)
    {
        forcedHitInterval = seconds;
    }

    // cleanup helper used by Pickaxe.ResetWeapon
    public void ClearLastHitTimes()
    {
        lastHitTimes.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        // If pickaxe is doing heavy, do single immediate hit here
        Pickaxe pick = GetComponentInParent<Pickaxe>();
        if (pick != null && pick.IsPerformingHeavy)
        {
            int id = other.gameObject.GetInstanceID();
            float last = lastHitTimes.ContainsKey(id) ? lastHitTimes[id] : -999f;
            if (Time.time - last < 0.01f) return; // already processed
            lastHitTimes[id] = Time.time;

            RegisterHit(other, pick);
        }
        // else: for light holds we rely on OnTriggerStay cadence
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canHit) return;
        if (!other.CompareTag("npc")) return;

        // determine hit interval and holdElapsed (if pick exists)
        Pickaxe pick = GetComponentInParent<Pickaxe>();
        float hitInterval = forcedHitInterval > 0f ? forcedHitInterval : fallbackHitInterval;
        float holdElapsed = pick != null ? pick.HoldElapsed : Mathf.Infinity;
        float startupDelay = pick != null ? pick.startupDelay : 0f;

        int id = other.gameObject.GetInstanceID();
        float last = lastHitTimes.ContainsKey(id) ? lastHitTimes[id] : -999f;

        // ensure startup delay has passed for hold/light attacks
        if (holdElapsed < startupDelay) return;

        // cadence check
        if (Time.time - last < hitInterval) return; // already hit too recently

        // register hit time
        lastHitTimes[id] = Time.time;

        RegisterHit(other, pick);
    }

    private void RegisterHit(Collider other, Pickaxe pick)
    {
        // capture hit point BEFORE knockback because enemy will move
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // Apply damage
        other.GetComponent<GeneralNPC>()?.TakeDamage(damage, DamageCause.EnemyAttack);

        // Play stone-hit / pick hit sfx
        (sfx ??= GetComponentInParent<WeaponSFX>())?.Pickaxe_StoneHitPlay();

        // Hit stop
        enemiesOnHit?.ApplyHitStop(this, 0.06f);

        // Flash enemy
        Renderer rend = other.GetComponentInChildren<Renderer>();
        if (rend != null && pick != null)
            pick.StartCoroutine(enemiesOnHit.FlashEnemy(rend, Color.white, Color.gray, 0.12f));

        // Small knockback (light only)
        enemiesOnHit?.ApplyKnockback(other, transform, 2f);

        // Spawn blood, scaled and with duration based on hold time at moment of hit
        if (bloodPrefab != null)
        {
            GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity);
            blood.transform.LookAt(hitPoint + (hitPoint - other.transform.position));

            float scale = Mathf.Clamp(other.transform.localScale.magnitude / 3f, 0.1f, 3f);
            blood.transform.localScale = Vector3.one * scale;

            ParticleSystem ps = blood.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Stop first before editing main module
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = ps.main;

                // Compute safe lifetimes
                float wantedDuration = Mathf.Clamp(pick != null ? pick.HoldElapsed : 0.1f, 0.05f, 2.5f);
                main.duration = wantedDuration;

                // Use constantMax as baseline for scaling lifetime
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
        // Optional: clear per-enemy timer state to allow immediate re-hit when re-entering
        int id = other.gameObject.GetInstanceID();
        if (lastHitTimes.ContainsKey(id))
            lastHitTimes.Remove(id);
    }
}
