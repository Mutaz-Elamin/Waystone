using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [SerializeField] private int startSeed = 1;

    [Header("Level Scenes (by name)")]
    [SerializeField] private string[] levelScenes;

    private int levelIndex;
    private int seed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        levelIndex = 0;
        seed = startSeed;
    }

    public void StartRun()
    {
        levelIndex = 0;
        seed = startSeed;
        SceneManager.LoadScene(levelScenes[0], LoadSceneMode.Single);
    }


    public void NextLevel()
    {
        levelIndex++;

        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogWarning("RunManager: No levelScenes assigned.");
            return;
        }

        int sceneIdx = Mathf.Clamp(levelIndex, 0, levelScenes.Length - 1);
        string sceneName = levelScenes[sceneIdx];

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("RunManager: Scene name is empty at index " + sceneIdx);
            return;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
