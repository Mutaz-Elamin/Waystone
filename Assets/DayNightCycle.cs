using UnityEngine;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    [Header("Celestial References")]
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private ParticleSystem starField;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private TextMeshProUGUI dayAlertText;

    [Header("Cycle Settings")]
    [SerializeField] private float dayLengthInSeconds = 60f;
    [SerializeField] private int totalDaysInGame = 4;
    [SerializeField] [Range(-1f, 1f)] private float nightThreshold = 0.1f;

    private float _currentTime = 0f;
    private int _currentDay = 1;

    // Peers can still read these
    public float CurrentTime => _currentTime;
    public int CurrentDay => _currentDay;
    public bool IsNight => sun != null && sun.transform.forward.y > -nightThreshold;

    private void Start()
    {
        // Reset everything to a safe state on start
        if(sun != null) sun.enabled = true;
        if(moon != null) moon.enabled = false;
        
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
        if (starField != null)
        {
            var emission = starField.emission;
            emission.enabled = IsNight;
        }

        // We use IsNight to swap the lights
        sun.enabled = !IsNight;
        if (moon != null) moon.enabled = IsNight;
        DynamicGI.UpdateEnvironment();
    }

    private void UpdateUI()
    {
        if (clockText == null) return;

        float dayPercent = _currentTime / dayLengthInSeconds;
        int totalMinutes = Mathf.FloorToInt(dayPercent * 1440); 
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        clockText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    private void ShowDayAlert()
    {
        if (dayAlertText == null) return;
        dayAlertText.text = "DAY " + _currentDay;
        CancelInvoke(nameof(HideDayAlert));
        Invoke(nameof(HideDayAlert), 3f);
    }

    private void HideDayAlert() => dayAlertText.text = "";
}