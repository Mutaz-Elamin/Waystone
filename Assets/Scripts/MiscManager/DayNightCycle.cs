using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Time")]
    [Tooltip("In-game minutes that pass per real second. 1 = 1 game minute/sec (a full day takes 24 real minutes).")]
    [SerializeField] private float gameMinutesPerSecond = 1.2f;

    [Tooltip("Starting time (0..24).")]
    [Range(0f, 24f)]
    [SerializeField] private float startHour = 8f;

    [SerializeField] private int maxDays = 4;

    private float minuteOfDay;
    private int dayCount = 1;

    public float CurrentHour => minuteOfDay / 60f;

    [Header("Skybox")]
    [Tooltip("Assign your gameplay skybox material here so it doesn't inherit from a menu scene.")]
    [SerializeField] private UnityEngine.Material skyboxMaterialOverride;

    [SerializeField] private bool rotateSkybox = true;
    [SerializeField] private float skyboxRotationOffset = 0f;
    [SerializeField] private float skyboxRotationMultiplier = 1f;

    private UnityEngine.Material runtimeSkybox;

    [Header("UI & End Game")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private float dayBannerDuration = 3f;

    private bool isShowingBanner = false;
    private bool gameEnded = false;
    private Vector3 originalTextScale;

    [Header("GI Update")]
    [Tooltip("Updating GI every frame is expensive. 0.25–1.0 is usually fine.")]
    [SerializeField] private float giUpdateInterval = 0.5f;
    private float giTimer = 0f;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (restartButton != null) restartButton.SetActive(false);
        if (clockText != null) originalTextScale = clockText.transform.localScale;
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterOneFrame(playBanner: true));
    }

    private IEnumerator InitializeAfterOneFrame(bool playBanner)
    {
        yield return null;

        dayCount = 1;
        gameEnded = false;
        minuteOfDay = Mathf.Repeat(startHour, 24f) * 60f;

        SetupSkyboxMaterial();

        ApplyLightingAndSky();
        UpdateClockUI(forceClock: true);

        if (playBanner)
            StartCoroutine(PlayDayBanner());
    }

    private void SetupSkyboxMaterial()
    {
        UnityEngine.Material source = skyboxMaterialOverride != null
            ? skyboxMaterialOverride
            : RenderSettings.skybox;

        if (source == null) return;

        if (runtimeSkybox == null || runtimeSkybox.shader != source.shader)
            runtimeSkybox = new UnityEngine.Material(source);
        else
            runtimeSkybox.CopyPropertiesFromMaterial(source);

        RenderSettings.skybox = runtimeSkybox;
    }

    private void Update()
    {
        if (gameEnded) return;

        AdvanceTime();
        ApplyLightingAndSky();

        if (!isShowingBanner)
            UpdateClockUI(forceClock: false);
    }

    private void AdvanceTime()
    {
        minuteOfDay += Time.deltaTime * gameMinutesPerSecond;

        if (minuteOfDay >= 1440f)
        {
            minuteOfDay -= 1440f;
            dayCount++;

            if (dayCount > maxDays) TriggerEndGame();
            else StartCoroutine(PlayDayBanner());
        }
    }

    private void ApplyLightingAndSky()
    {
        float hour = CurrentHour;

        float sunRotationX = (hour - 6f) * 15f;
        if (sunLight) sunLight.transform.localRotation = Quaternion.Euler(sunRotationX, 0f, 0f);
        if (moonLight) moonLight.transform.localRotation = Quaternion.Euler(sunRotationX + 180f, 0f, 0f);

        bool isDay = hour >= 6f && hour < 18f;
        if (sunLight) sunLight.enabled = isDay;
        if (moonLight) moonLight.enabled = !isDay;

        RenderSettings.sun = isDay ? sunLight : moonLight;
        RenderSettings.ambientMode = AmbientMode.Flat;

        float sunHeight = Mathf.Sin(sunRotationX * Mathf.Deg2Rad);
        float sunHeight01 = Mathf.Clamp01((sunHeight + 0.1f) / 1.1f);
        float exposure = Mathf.Lerp(0.05f, 1.0f, sunHeight01);

        RenderSettings.ambientLight = Color.Lerp(new Color(0.1f, 0.11f, 0.18f), Color.white, exposure);

        if (runtimeSkybox != null)
        {
            if (runtimeSkybox.HasProperty("_Exposure"))
                runtimeSkybox.SetFloat("_Exposure", exposure);

            if (rotateSkybox && runtimeSkybox.HasProperty("_Rotation"))
            {
                float rot = (minuteOfDay / 1440f) * 360f;
                rot = (rot * skyboxRotationMultiplier) + skyboxRotationOffset;
                runtimeSkybox.SetFloat("_Rotation", Mathf.Repeat(rot, 360f));
            }
        }

        giTimer -= Time.deltaTime;
        if (giTimer <= 0f)
        {
            giTimer = Mathf.Max(0.05f, giUpdateInterval);
            DynamicGI.UpdateEnvironment();
        }
    }

    private void UpdateClockUI(bool forceClock)
    {
        if (clockText == null) return;
        if (!forceClock && isShowingBanner) return;

        int totalMinutes = Mathf.FloorToInt(minuteOfDay);
        int h = (totalMinutes / 60) % 24;
        int m = totalMinutes % 60;

        clockText.text = $"Day {dayCount} - {h:00}:{m:00}";
        clockText.color = Color.white;
        clockText.transform.localScale = originalTextScale;
    }

    private IEnumerator PlayDayBanner()
    {
        if (clockText == null) yield break;

        isShowingBanner = true;

        float elapsed = 0f;
        clockText.text = $"DAY {dayCount}";
        clockText.color = new Color(1f, 0.85f, 0f);

        while (elapsed < dayBannerDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dayBannerDuration);
            float scaleCurve = Mathf.Sin(t * Mathf.PI) * 1.2f + 1f;
            clockText.transform.localScale = originalTextScale * scaleCurve;
            yield return null;
        }

        isShowingBanner = false;
        UpdateClockUI(forceClock: true);
    }

    private void TriggerEndGame()
    {
        gameEnded = true;

        if (clockText != null)
        {
            clockText.text = "END";
            clockText.color = Color.red;
            clockText.transform.localScale = originalTextScale * 2f;
        }

        if (restartButton != null) restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (runtimeSkybox != null)
        {
            Destroy(runtimeSkybox);
            runtimeSkybox = null;
        }
    }
}
