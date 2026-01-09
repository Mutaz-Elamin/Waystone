using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class HeatstrokeEffect_BuiltIn : MonoBehaviour
{
    [Range(0f, 1f)]
    public float intensity = 0f;

    [SerializeField] private PostProcessVolume volume;

    private Vignette vignette;
    private ColorGrading colorGrading;
    private ChromaticAberration chromAb;
    private Grain grain;
    private LensDistortion lensDist;

    void Awake()
    {
        if (!volume) volume = GetComponent<PostProcessVolume>();
        if (!volume || !volume.profile) return;

        // Grab settings from the profile
        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out colorGrading);
        volume.profile.TryGetSettings(out chromAb);
        volume.profile.TryGetSettings(out grain);
        volume.profile.TryGetSettings(out lensDist);
    }

    void Update()
    {
        float t = Mathf.Clamp01(intensity);

        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0f, 0.45f, t);
            vignette.smoothness.value = Mathf.Lerp(0.2f, 0.9f, t);
        }

        if (colorGrading != null)
        {
            colorGrading.temperature.value = Mathf.Lerp(0f, 30f, t);
            colorGrading.saturation.value = Mathf.Lerp(0f, -35f, t);
            colorGrading.contrast.value = Mathf.Lerp(0f, 15f, t);
            colorGrading.postExposure.value = Mathf.Lerp(0f, 0.2f, t);
        }

        if (chromAb != null)
            chromAb.intensity.value = Mathf.Lerp(0f, 0.15f, t);

        if (grain != null)
        {
            grain.intensity.value = Mathf.Lerp(0f, 0.25f, t);
            grain.size.value = Mathf.Lerp(1f, 1.4f, t);
        }

        if (lensDist != null)
            lensDist.intensity.value = Mathf.Lerp(0f, -15f, t);
    }

    public void SetHeatstroke(float value) => intensity = Mathf.Clamp01(value);
}
