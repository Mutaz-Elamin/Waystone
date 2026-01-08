using UnityEngine;
using TMPro;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Time Settings")]
    [Tooltip("3 = 8 minute real-time day.")]
    [SerializeField] private float daySpeed = 3f; 
    [Range(0, 24)] [SerializeField] private float currentHour = 0f;
    private int dayCount = 1;

    [Header("UI & Animation")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private float animationDuration = 2.5f; 

    private bool isAnimating = false;
    private Vector3 originalTextScale;

    private void Start()
    {
        // Reset everything to start at Day 1, Midnight
        dayCount = 1;
        currentHour = 0f;
        
        if (clockText != null) originalTextScale = clockText.transform.localScale;

        // Initial atmosphere setup
        if (RenderSettings.skybox != null)
            RenderSettings.skybox.SetFloat("_Exposure", 0.05f);
            
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        StartCoroutine(PlayDayAnimation());
    }

    private void Update()
    {
        // FIX 1: Hard-override speed to 3 every frame to prevent Inspector bugs
        daySpeed = 3f;

        // FIX 2: Only advance time if we aren't showing the "Day X" popup
        // This keeps the timer at 00:00 until the animation ends
        if (!isAnimating)
        {
            UpdateTime();
            UpdateClockUI();
        }

        UpdateLightRotation();
        UpdateAtmosphere();
    }

    private void UpdateTime()
    {
        currentHour += (Time.deltaTime * daySpeed) / 60f; 

        if (currentHour >= 24f) 
        {
            currentHour = 0f; // Snap to exactly midnight
            dayCount++;
            StartCoroutine(PlayDayAnimation());
        }
    }

    private void UpdateLightRotation()
    {
        // Calculates rotation based on time (6:00 is sunrise)
        float sunRotation = (currentHour - 6f) * 15f;
        
        if (sunLight) sunLight.transform.localRotation = Quaternion.Euler(sunRotation, 0f, 0f);
        if (moonLight) moonLight.transform.localRotation = Quaternion.Euler(sunRotation + 180f, 0f, 0f);
    }

    private void UpdateAtmosphere()
    {
        float sunRotation = (currentHour - 6f) * 15f;
        float sunHeight = Mathf.Sin(sunRotation * Mathf.Deg2Rad); 
        float exposure = Mathf.Lerp(0.05f, 1.0f, Mathf.Clamp01(sunHeight * 2.0f));

        bool isDay = currentHour >= 6f && currentHour < 18f;

        if (sunLight) sunLight.enabled = isDay;
        if (moonLight) moonLight.enabled = !isDay;

        if (RenderSettings.skybox != null) RenderSettings.skybox.SetFloat("_Exposure", exposure);
        RenderSettings.sun = isDay ? sunLight : moonLight;
        RenderSettings.ambientLight = Color.Lerp(new Color(0.1f, 0.11f, 0.18f), Color.white, exposure);
        
        DynamicGI.UpdateEnvironment();
    }

    IEnumerator PlayDayAnimation()
    {
        if (clockText == null) yield break;

        isAnimating = true;
        float elapsed = 0f;

        // Show ONLY "DAY X" - no timer next to it
        clockText.text = "DAY " + dayCount;
        clockText.color = new Color(1f, 0.85f, 0f); // Nice Game-Gold

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / animationDuration;

            // Simple pulse animation
            float scaleCurve = Mathf.Sin(percent * Mathf.PI) * 1.2f + 1f;
            clockText.transform.localScale = originalTextScale * scaleCurve;

            yield return null;
        }

        // Return everything to normal gameplay state
        clockText.transform.localScale = originalTextScale;
        clockText.color = Color.white;
        isAnimating = false;
    }

    private void UpdateClockUI()
    {
        if (clockText == null) return;

        int h = Mathf.FloorToInt(currentHour);
        int m = Mathf.FloorToInt((currentHour - h) * 60);
        clockText.text = string.Format("Day {0} - {1:00}:{2:00}", dayCount, h, m);
    }
}