using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;


    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    Resolution[] resolutions;

    private void Start()
    {
        LoadSettings();
        SetupResolutionDropdown();
    }

    // ================= AUDIO =================

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

   
    // ================= GRAPHICS =================

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }


    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    // ================= SETUP =================

    void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    // ================= SAVE / LOAD =================

    void LoadSettings()
    {
        // Audio
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
 

        masterVolumeSlider.value = master;

        SetMasterVolume(master);

        // Graphics
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = fullscreen;
        Screen.fullScreen = fullscreen;

    }
}