using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneOnceOnFirstEnter : MonoBehaviour
{
    // Static survives scene reloads during the same app session
    private static bool reloadedThisSession = false;

    [SerializeField] private bool onlyInBuild = true;

    private IEnumerator Start()
    {
        if (onlyInBuild && Application.isEditor) yield break;

        if (reloadedThisSession) yield break;
        reloadedThisSession = true;

        // Wait 1 frame so the scene fully becomes active (prevents some load-order weirdness)
        yield return null;

        int idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx, LoadSceneMode.Single);
    }
}
