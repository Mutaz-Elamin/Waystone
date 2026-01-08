using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SummonBossLandmark : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Altar Shrink")]
    [SerializeField] private Transform shrinkTarget;
    [SerializeField, Min(0.01f)] private float shrinkDuration = 0.6f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("FX (Optional)")]
    [SerializeField] private GameObject summonFxPrefab;
    [SerializeField] private bool spawnFxAtSpawnPoint = true;

    [Header("After Summon")]
    [SerializeField, Min(0f)] private float delayBeforeBossSpawn = 0.1f;
    [SerializeField] private bool destroyLandmarkAfterSummon = true;

    private bool summoned;
    private Vector3 baseScale;

    private void Awake()
    {
        if (spawnPoint == null) spawnPoint = transform;
        if (shrinkTarget == null) shrinkTarget = transform;

        baseScale = shrinkTarget.localScale;
    }

    public void SummonBoss()
    {
        if (summoned) return;
        summoned = true;

        if (bossPrefab == null)
        {
            Debug.LogWarning($"{name}: No bossPrefab assigned.");
            return;
        }

        StartCoroutine(SummonRoutine());
    }

    private IEnumerator SummonRoutine()
    {
        // Optional FX
        if (summonFxPrefab != null)
        {
            Transform fxT = (spawnFxAtSpawnPoint && spawnPoint != null) ? spawnPoint : transform;
            Instantiate(summonFxPrefab, fxT.position, summonFxPrefab.transform.rotation);
        }

        // Shrink animation
        float t = 0f;
        float dur = Mathf.Max(0.01f, shrinkDuration);

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            float s = shrinkCurve != null ? shrinkCurve.Evaluate(u) : (1f - u);
            shrinkTarget.localScale = baseScale * Mathf.Max(0f, s);

            yield return null;
        }

        shrinkTarget.localScale = Vector3.zero;

        if (delayBeforeBossSpawn > 0f)
            yield return new WaitForSeconds(delayBeforeBossSpawn);

        // Spawn boss
        Transform sp = spawnPoint != null ? spawnPoint : transform;
        Instantiate(bossPrefab, sp.position, sp.rotation);

        // Landmark is one-time use, so either destroy it or leave it shrunk.
        if (destroyLandmarkAfterSummon)
        {
            Destroy(gameObject);
        }
    }
}
