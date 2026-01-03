using TMPro;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public ItemClass itemData;
    [SerializeField] private GameObject prompt;

    void Start()
        
    {
        

        prompt.GetComponent<TextMeshPro>().text = itemData.itemName;

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
    void LateUpdate()
    {
        if (Camera.main == null) return;

        // Always face the camera
        transform.LookAt(Camera.main.transform.position);

        // OPTIONAL: lock the X/Z rotation so text stays upright
        Vector3 euler = transform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        euler.y += 180; // Face the camera directly
        transform.eulerAngles = euler;
    }
}
