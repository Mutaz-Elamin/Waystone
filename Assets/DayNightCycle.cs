using UnityEngine;
using TMPro;
using System.Collections;

public class DayNightManager : MonoBehaviour
{
    [Header("Celestial References")]
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private ParticleSystem starField;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private TextMeshProUGUI dayAlertText;
    [SerializeField] private CanvasGroup dayAlertCanvasGroup;

    [Header("Cycle Settings")]
    [SerializeField] private float dayLengthInSeconds = 60f;
    [SerializeField] private int totalDaysInGame = 4;

    private float _currentTime = 0f;
    private int _currentDay = 1;

    // FIX: Optimized check to see if the sun is below the horizon line
    public bool IsNight => sun != null && (sun.transform.localEulerAngles.x > 175 || sun.transform.localEulerAngles.x < 5);

    private void Start()
    {
        if (dayAlertCanvasGroup != null) dayAlertCanvasGroup.alpha = 0;
        
        // Ensure initial lighting state
        UpdateCelestialVisuals();
        ShowDayAlert();
    }

    private void Update()
    {
        if (sun == null) return;

        HandleTimeAndRotation();
        UpdateCelestialVisuals();
        UpdateUI();
    }

    private void HandleTimeAndRotation()
    {
        float rotationPerSecond = 360f / dayLengthInSeconds;
        sun.transform.Rotate(Vector3.right * rotationPerSecond * Time.deltaTime);

        if (moon != null)
        {
            // Keeps moon exactly opposite to the sun
            moon.transform.rotation = sun.transform.rotation * Quaternion.Euler(180f, 0f, 0f);
        }

        _currentTime += Time.deltaTime;

        if (_currentTime >= dayLengthInSeconds)
        {
            _currentTime = 0;
            _currentDay++;
            if (_currentDay <= totalDaysInGame) ShowDayAlert();
        }
    }

    private void UpdateCelestialVisuals()
    {
        bool isNight = IsNight;

        // 1. Toggle the Light Objects
        sun.enabled = !isNight;
        if (moon != null) moon.enabled = isNight;

        // 2. Toggle the Stars
        if (starField != null)
        {
            var emission = starField.emission;
            emission.enabled = isNight;
        }

        // 3. FORCE DARKNESS (The Brightness Fix)
        if (isNight)
        {
            // This forces the skybox to follow the moon's position instead of the sun
            RenderSettings.sun = moon; 
            
            // This kills the 'Skybox Glow' that was keeping your scene bright in the video
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.03f); // Very dark blue/black
            RenderSettings.ambientIntensity = 0f; 
        }
        else
        {
            // Restore daylight settings
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
        }

        // 4. Update the actual environment visuals
        DynamicGI.UpdateEnvironment();
    }

    private void UpdateUI()
    {
        if (clockText == null) return;
        float dayPercent = _currentTime / dayLengthInSeconds;
        int hours = Mathf.FloorToInt(dayPercent * 24);
        int minutes = Mathf.FloorToInt((dayPercent * 1440) % 60);
        clockText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    private void ShowDayAlert()
    {
        if (dayAlertCanvasGroup == null) return;
        StopAllCoroutines();
        StartCoroutine(AnimateDayAlert());
    }

    private IEnumerator AnimateDayAlert()
    {
        dayAlertText.text = "DAY " + _currentDay;
        float duration = 0.5f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            dayAlertCanvasGroup.alpha = timer / duration;
            dayAlertText.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, timer / duration);
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            dayAlertCanvasGroup.alpha = 1f - (timer / duration);
            yield return null;
        }
    }
}