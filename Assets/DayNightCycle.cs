using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Time Settings")]
    private float daySpeed = 1.2f; 
    [Range(0, 24)] [SerializeField] private float currentHour = 0f;
    private int dayCount = 1;
    private int maxDays = 4;

    // --- Added this so the Hunger Disaster can "see" the time ---
    public float CurrentHour => currentHour; 

    [Header("UI & Animation")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private GameObject restartButton; 
    [SerializeField] private float animationDuration = 3f; 

    private bool isAnimating = false;
    private bool gameEnded = false;
    private Vector3 originalTextScale;

    private void Start()
    {
        dayCount = 1;
        currentHour = 0f;
        gameEnded = false;

        if (restartButton != null) restartButton.SetActive(false);
        if (clockText != null) originalTextScale = clockText.transform.localScale;

        if (RenderSettings.skybox != null)
            RenderSettings.skybox.SetFloat("_Exposure", 0.05f);
        
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        StartCoroutine(PlayDayAnimation());
    }

    private void Update()
    {
        if (gameEnded) return;

        daySpeed = 1.2f;

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
            currentHour = 0f;
            dayCount++;

            if (dayCount > maxDays) TriggerEndGame();
            else StartCoroutine(PlayDayAnimation());
        }
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

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    private void UpdateLightRotation()
    {
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

    System.Collections.IEnumerator PlayDayAnimation()
    {
        if (clockText == null) yield break;
        isAnimating = true;
        float elapsed = 0f;
        clockText.text = "DAY " + dayCount;
        clockText.color = new Color(1f, 0.85f, 0f); 

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / animationDuration;
            float scaleCurve = Mathf.Sin(percent * Mathf.PI) * 1.2f + 1f;
            clockText.transform.localScale = originalTextScale * scaleCurve;
            yield return null;
        }

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