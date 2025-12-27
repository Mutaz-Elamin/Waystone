using System.Collections.Generic;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private float collectCooldown = 0.2f;
    private InventoryManager inventory; 

    private float lastCollectTime = 0f;
    private readonly List<Collectible> nearbyItems = new List<Collectible>();
    private int collectedCount = 0;

    void Start()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<InventoryManager>();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Collectible item = other.GetComponent<Collectible>();
        if (item != null && !nearbyItems.Contains(item))
        {
            nearbyItems.Add(item);
            item.ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Collectible item = other.GetComponent<Collectible>();
        if (item != null)
        {
            nearbyItems.Remove(item);
            item.HidePrompt();
        }
    }

    // Called by InputManager when Interact (E) is pressed
    public void TryCollect()
    {
        if (inventory == null) return;
        if (nearbyItems.Count == 0) return;

        if (Time.time < lastCollectTime + collectCooldown)
            return;

        // Take the first nearby item
        Collectible target = nearbyItems[0];

        if (target == null)
        {
            nearbyItems.RemoveAt(0);
            return;
        }

        if (target.itemData != null)
        {
            inventory.Add(target.itemData, 1);
            collectedCount++;
            Debug.Log($"Collected {target.itemData.itemName}. Total collected: {collectedCount}");
        }

        // Hide/remove the cube from the world
        target.gameObject.SetActive(false);
        nearbyItems.RemoveAt(0);

        lastCollectTime = Time.time;
    }
}
