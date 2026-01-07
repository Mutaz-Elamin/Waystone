using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    public Light sunLight;
    public Light moonLight;

    [Header("Visual Orbs")]
    public GameObject sunOrb;
    public GameObject moonOrb;

    [Header("Time Control")]
    public float daySpeed = 20f; 
    [Range(0, 24)] public float currentHour = 6f;

    [Header("UI")]
    public TextMeshProUGUI clockText;

    private void Start()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    private void Update()
    {
        currentHour += (Time.deltaTime * daySpeed) / 60f; 
        if (currentHour >= 24f) currentHour = 0f;

        float sunAngle = (currentHour / 24f) * 360f - 90f;
        float moonAngle = sunAngle + 180f;

        if (sunLight != null) sunLight.transform.localRotation = Quaternion.Euler(sunAngle, 0f, 0f);
        if (moonLight != null) moonLight.transform.localRotation = Quaternion.Euler(moonAngle, 0f, 0f);

        bool isDay = currentHour >= 6f && currentHour < 18f;
        UpdateAtmosphere(isDay);
        UpdateClockUI();
    }

    void UpdateAtmosphere(bool isDay)
    {
        sunLight.enabled = isDay;
        moonLight.enabled = !isDay;

        if (sunOrb != null) sunOrb.SetActive(isDay);
        if (moonOrb != null) moonOrb.SetActive(!isDay);

        // --- THE SMOOTH FADE LOGIC ---
        // Get the current exposure. We want to "Lerp" (Linear Interpolate)
        // toward the target brightness so it doesn't just snap.
        float currentExposure = RenderSettings.skybox.GetFloat("_Exposure");
        float targetExposure = isDay ? 1.0f : 0.05f;
        
        // This line creates the "Fade" effect over time
        float smoothExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
        RenderSettings.skybox.SetFloat("_Exposure", smoothExposure);

        // Smoothly fade the ambient light color too
        Color targetAmbient = isDay ? Color.white : new Color(0.1f, 0.1f, 0.25f);
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbient, Time.deltaTime * 2f);
        
        RenderSettings.sun = isDay ? sunLight : moonLight;
    }

    void UpdateClockUI()
    {
        if (clockText != null)
        {
            int h = Mathf.FloorToInt(currentHour);
            int m = Mathf.FloorToInt((currentHour - h) * 60);
            clockText.text = string.Format("{0:00}:{1:00}", h, m);
        }
    }
}