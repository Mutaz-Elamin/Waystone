using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class HitFeedback : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField, Min(0.01f)] private float duration = 0.12f;
    [SerializeField, Range(0f, 0.5f)] private float shrinkAmount = 0.12f;
    [SerializeField, Min(0f)] private float tiltDegrees = 10f;

    [SerializeField] private bool usePositionShake = true;
    [SerializeField, Min(0f)] private float positionShake = 0.05f;

    private Vector3 baseScale;
    private Quaternion baseRot;
    private Vector3 baseLocalPos;

    private Coroutine running;

    private void Awake()
    {
        AutoPickTarget();
        CacheBase();
    }

    private void OnEnable()
    {
        if (running != null) { StopCoroutine(running); running = null; }
        AutoPickTarget();
        CacheBase();
        ApplyBase();
    }

    private void OnDisable()
    {
        if (running != null) { StopCoroutine(running); running = null; }
    }

    public void Play()
    {
        AutoPickTarget();

        if (running != null)
        {
            ApplyBase();
        }

        CacheBase();
        ApplyBase();

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PlayRoutine());
    }

    private void AutoPickTarget()
    {
        if (target != null) return;

        Renderer r = GetComponentInChildren<Renderer>(true);
        target = r != null ? r.transform : transform;
    }

    private void CacheBase()
    {
        if (target == null) target = transform;
        baseScale = target.localScale;
        baseRot = target.localRotation;
        baseLocalPos = target.localPosition;
    }

    private void ApplyBase()
    {
        if (target == null) return;
        target.localScale = baseScale;
        target.localRotation = baseRot;
        target.localPosition = baseLocalPos;
    }

    private IEnumerator PlayRoutine()
    {
        float sign = Random.value < 0.5f ? -1f : 1f;

        Vector3 shakeDir = Random.insideUnitSphere;
        shakeDir.y = 0f;
        if (shakeDir.sqrMagnitude < 0.0001f) shakeDir = Vector3.right;
        shakeDir.Normalize();

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            float punch = Mathf.Sin(u * Mathf.PI);

            float s = 1f - shrinkAmount * punch;
            target.localScale = baseScale * s;

            float ang = tiltDegrees * punch * sign;
            target.localRotation = baseRot * Quaternion.Euler(ang, 0f, 0f);

            if (usePositionShake && positionShake > 0f)
                target.localPosition = baseLocalPos + shakeDir * (positionShake * punch);

            yield return null;
        }

        ApplyBase();
        running = null;
    }
}
