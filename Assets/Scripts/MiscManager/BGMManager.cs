using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    public AudioSource audioSource;

    [Header("Tracks")]
    public AudioClip menuTrack;
    public AudioClip forestTrack;
    public AudioClip desertTrack;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.volume = 0.186f;
    }

    public void PlayMenu()
    {
        PlayTrack(menuTrack);
    }

    public void PlayForest()
    {
        PlayTrack(forestTrack);
    }

    public void PlayDesert()
    {
        PlayTrack(desertTrack);
    }

    private void PlayTrack(AudioClip clip)
    {
        if (clip == null || audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}