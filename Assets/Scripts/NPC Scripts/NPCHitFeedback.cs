using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMaterial = UnityEngine.Material;


[DisallowMultipleComponent]
public sealed class NpcHitFeedback : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;

    [Header("Flash")]
    [SerializeField, Min(0.01f)] private float duration = 0.10f;
    [SerializeField, Range(0f, 1f)] private float flashStrength = 0.25f;
    [SerializeField] private Color flashColor = new Color(1f, 0.1f, 0.1f, 1f);

    [SerializeField] private string[] colorProperties = { "_BaseColor", "_Color", "_MainColor", "_TintColor" };

    private struct ColorSlot
    {
        public Renderer r;
        public int materialIndex;
        public int propId;
        public Color baseColor;
    }

    private readonly List<ColorSlot> slots = new(32);
    private MaterialPropertyBlock mpb;
    private Coroutine routine;

    private void Awake()
    {
        AutoSetup();
        mpb ??= new MaterialPropertyBlock();
        BuildSlots();
    }

    private void OnEnable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }

        AutoSetup();
        mpb ??= new MaterialPropertyBlock();
        BuildSlots();
        ClearFlash();
    }

    private void OnDisable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    public void Play()
    {
        ClearFlash();
        AutoSetup();
        BuildSlots();

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    private void AutoSetup()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void BuildSlots()
    {
        slots.Clear();
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            UMaterial[] mats = r.sharedMaterials;
            if (mats == null) continue;

            for (int mi = 0; mi < mats.Length; mi++)
            {
                UMaterial mat = mats[mi];
                if (mat == null) continue;

                int propId = FindFirstColorProp(mat);
                if (propId == -1) continue;

                Color baseCol = mat.GetColor(propId);

                slots.Add(new ColorSlot
                {
                    r = r,
                    materialIndex = mi,
                    propId = propId,
                    baseColor = baseCol
                });
            }
        }
    }

    private int FindFirstColorProp(UMaterial mat)
    {
        for (int i = 0; i < colorProperties.Length; i++)
        {
            string p = colorProperties[i];
            if (!string.IsNullOrEmpty(p) && mat.HasProperty(p))
                return Shader.PropertyToID(p);
        }
        return -1;
    }

    private IEnumerator Run()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            // 0->1->0 flash
            float punch = Mathf.Sin(u * Mathf.PI);
            ApplyFlash(punch * flashStrength);

            yield return null;
        }

        ClearFlash();
        routine = null;
    }

    private void ApplyFlash(float blend01)
    {
        float t = Mathf.Clamp01(blend01);
        if (t <= 0f) { ClearFlash(); return; }

        for (int i = 0; i < slots.Count; i++)
        {
            ColorSlot s = slots[i];
            if (s.r == null) continue;

            Color blended = Color.Lerp(s.baseColor, flashColor, t);
            blended.a = s.baseColor.a;

            s.r.GetPropertyBlock(mpb, s.materialIndex);
            mpb.SetColor(s.propId, blended);
            s.r.SetPropertyBlock(mpb, s.materialIndex);
        }
    }

    private void ClearFlash()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            int matCount = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
            for (int mi = 0; mi < matCount; mi++)
                r.SetPropertyBlock(null, mi);
        }
    }
}
