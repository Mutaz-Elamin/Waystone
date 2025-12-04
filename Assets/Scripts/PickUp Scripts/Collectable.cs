using TMPro;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public ItemClass itemData;
    [SerializeField] private GameObject prompt;

    void Start()
        
    {

        if (prompt == null)
        {
            
        }
        else
        {
            
            prompt.SetActive(false);
        }
    }

    public void ShowPrompt()
    {
        
        if (prompt != null)
            prompt.SetActive(true);
    }

    public void HidePrompt()
    {
        
        if (prompt != null)
            prompt.SetActive(false);
    }
}
