using System.Collections.Generic;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private float collectCooldown = 0.2f;
    private InventoryManager inventory; 

    private float lastCollectTime = 0f;
    private readonly List<Collectible> nearbyItems = new List<Collectible>();
    private int collectedCount = 0;
    private CraftingStation stationInRange;
    void Start()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<InventoryManager>();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        CraftingStation station = other.GetComponent<CraftingStation>();
        if (station != null)
        {
            stationInRange = station;
            return;
        }
        Collectible item = other.GetComponent<Collectible>();
        if (item != null && !nearbyItems.Contains(item))
        {
            nearbyItems.Add(item);
            item.ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CraftingStation station = other.GetComponent<CraftingStation>();
        if (station != null && station == stationInRange)
        {
            stationInRange = null;
            return;
        }
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

        if (stationInRange != null)
        {
            stationInRange.Interact(this);
            lastCollectTime = Time.time;
            return;
        }


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
            
        }

        // remove the cube from the world
        Destroy(target.gameObject);
        nearbyItems.RemoveAt(0);
        

        lastCollectTime = Time.time;
    }
}
