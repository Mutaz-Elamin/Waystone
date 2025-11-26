using UnityEngine;

public class Collectible : MonoBehaviour
{
    public ItemClass itemData;
    [SerializeField] private GameObject prompt;

    void Start()
    {
        if (prompt == null)
        {
            Debug.LogWarning($"Collectible on {gameObject.name} has no prompt assigned.");
        }
        else
        {
            prompt.SetActive(false);
        }
    }

    public void ShowPrompt()
    {
        Debug.Log($"ShowPrompt called on {gameObject.name}");
        if (prompt != null)
            prompt.SetActive(true);
    }

    public void HidePrompt()
    {
        Debug.Log($"HidePrompt called on {gameObject.name}");
        if (prompt != null)
            prompt.SetActive(false);
    }
}
