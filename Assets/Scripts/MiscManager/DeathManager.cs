using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [SerializeField] private GameObject deathUI;
    public bool IsDead { get; private set; } = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (deathUI != null)
            deathUI.SetActive(false);
    }

    public void HandleDeath()
    {
        if (IsDead) return;
        IsDead = true;
        Time.timeScale = 0f;

        if (deathUI != null)
            deathUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        IsDead = false;
        Time.timeScale = 1f;

        if (deathUI != null)
            deathUI.SetActive(false);

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