using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [SerializeField] private GameObject victoryUI;

    private bool hasWon = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    // Reset state if this object persists or is re-enabled
    private void OnEnable()
    {
        hasWon = false;
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    // Safe call for other scripts
    public static void TriggerVictorySafe()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[VictoryManager] TriggerVictory called but Instance is null.");
            return;
        }
        Instance.TriggerVictory();
    }

    public void TriggerVictory()
    {
        // Optional: if you want death to block victory, check a flag on DeathManager (see below)
        if (DeathManager.Instance != null && DeathManager.Instance.IsDead)
        {
            Debug.Log("[VictoryManager] Won attempted but player already dead. Ignoring.");
            return;
        }

        if (hasWon)
        {
            Debug.Log("[VictoryManager] TriggerVictory called but already won.");
            return;
        }

        hasWon = true;
        Debug.Log("[VictoryManager] Victory triggered.");

        Time.timeScale = 0f;

        if (victoryUI != null)
            victoryUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (victoryUI != null)
            victoryUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(1); // first level
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}