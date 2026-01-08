using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DamageCause
{
    PlayerAttack,
    PlayerPickaxe,
    PlayerAxe,
    EnemyAttack,
    Environment,
    Other,
    None
}

public class HealthBasedAsset : MonoBehaviour
{

    [SerializeField] private int startHealth;
    protected int StartHealth { get { return startHealth; } }
    protected int health;
    protected int Health { get { return health; } }

    [Header("Visual Target (scale this, NOT the agent root)")]
    [SerializeField] private Transform visualRoot;

    [Header("Spawn FX")]
    [SerializeField] private bool playSpawnFx = true;
    [SerializeField] private GameObject spawnFxPrefab;
    [SerializeField] private float spawnFxScale = 1f;
    [SerializeField, Min(0.01f)] private float spawnDuration = 0.20f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool disableGameplayDuringSpawn = true;

    [Header("Death FX")]
    [SerializeField] private bool playDeathFx = true;
    [SerializeField] private GameObject deathFxPrefab;
    [SerializeField] private float deathFxScale = 1f;
    [SerializeField, Min(0.01f)] private float deathDuration = 0.20f;
    [SerializeField] private AnimationCurve deathCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool disableGameplayDuringDeath = true;

    private Vector3 visualBaseScale;
    private Coroutine spawnRoutine;
    private Coroutine deathRoutine;
    private bool dying;

    protected virtual void Awake()
    {
        health = startHealth;
        ResolveVisualRoot();
        visualBaseScale = visualRoot.localScale;
    }

    protected virtual void OnEnable()
    {
        health = startHealth;
        dying = false;

        ResolveVisualRoot();
        if (visualBaseScale.sqrMagnitude < 0.0001f)
            visualBaseScale = visualRoot.localScale.sqrMagnitude > 0.0001f ? visualRoot.localScale : Vector3.one;

        if (spawnRoutine != null) { StopCoroutine(spawnRoutine); spawnRoutine = null; }
        if (deathRoutine != null) { StopCoroutine(deathRoutine); deathRoutine = null; }

        if (playSpawnFx)
            spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    // Method to apply damage
    public virtual void TakeDamage(int damage, DamageCause cause)
    {
        if (dying) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }


    // Method to kill asset (may be changed public if a feature to autokill npcs is added)
    protected virtual void Die()
    {
        if (dying) return;
        dying = true;

        DropResources();

        if (spawnRoutine != null) { StopCoroutine(spawnRoutine); spawnRoutine = null; }

        if (playDeathFx)
            deathRoutine = StartCoroutine(DeathRoutine());
        else
            CompleteDeath();
    }


    // Method to set the resources that are dropped upon death and how this works
    // By default, die function will call this method but this may not be true for all assets
    protected virtual void DropResources()
    {
        // Resource dropping logic
    }

    private IEnumerator SpawnRoutine()
    {
        if (disableGameplayDuringSpawn)
            SetGameplayEnabled(false);
        visualRoot.localScale = Vector3.zero;

        if (spawnFxPrefab != null)
        {
            GameObject obj = Instantiate(spawnFxPrefab, transform.position, spawnFxPrefab.transform.rotation);
            obj.transform.localScale = Vector3.one * spawnFxScale;
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, spawnDuration);

        while (t < dur)
        {
            if (dying) yield break;

            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            float k = spawnCurve != null ? spawnCurve.Evaluate(u) : u;
            visualRoot.localScale = visualBaseScale * Mathf.Clamp01(k);

            yield return null;
        }

        visualRoot.localScale = visualBaseScale;

        if (disableGameplayDuringSpawn && !dying)
            SetGameplayEnabled(true);

        spawnRoutine = null;
    }

    private IEnumerator DeathRoutine()
    {
        if (disableGameplayDuringDeath)
            SetGameplayEnabled(false);

        if (deathFxPrefab != null)
        {
            GameObject obj = Instantiate(deathFxPrefab, transform.position, deathFxPrefab.transform.rotation);
            obj.transform.localScale = Vector3.one * deathFxScale;
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, deathDuration);

        Vector3 startScale = visualRoot.localScale.sqrMagnitude > 0.0001f ? visualRoot.localScale : visualBaseScale;

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            float k = deathCurve != null ? deathCurve.Evaluate(u) : (1f - u);
            visualRoot.localScale = startScale * Mathf.Clamp01(k);

            yield return null;
        }

        visualRoot.localScale = Vector3.zero;

        CompleteDeath();
    }

    private void CompleteDeath()
    {
        ClusterMember member = GetComponent<ClusterMember>();
        if (member != null)
        {
            member.Despawn(reduceTarget: true);
            return;
        }

        Destroy(gameObject);
    }

    private void ResolveVisualRoot()
    {
        if (visualRoot != null) return;

        Renderer r = GetComponentInChildren<Renderer>(true);
        if (r != null) visualRoot = r.transform;
        else visualRoot = transform;
    }

    private void SetGameplayEnabled(bool enabled)
    {
        var cols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = enabled;

        var agent = GetComponentInChildren<NavMeshAgent>(true);
        if (agent != null) agent.enabled = enabled;

        var anim = GetComponentInChildren<Animator>(true);
        if (anim != null) anim.enabled = enabled;
    }

}
