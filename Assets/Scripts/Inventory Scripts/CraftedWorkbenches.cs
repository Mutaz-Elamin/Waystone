using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftedWorkbenches", menuName = "Inventory/CraftedWorkbenches")]
public class CraftedWorkbenches : ItemClass
{
    public GameObject worldPrefab;   // real workbench placed in the world
    public GameObject previewPrefab; // ghost preview shown while holding

    private GameObject activePreview;

    public override ItemClass GetItem() => this;

    public override void Equip(GameObject user)
    {
        // Spawn preview once
        if (activePreview != null) return;

        activePreview = Instantiate(previewPrefab);
        activePreview.name = "WorkbenchPreview";
        activePreview.transform.localScale = previewPrefab.transform.localScale;
        activePreview.transform.rotation = previewPrefab.transform.rotation;
    }

    public override void UseItem(GameObject user)
    {
        if (activePreview == null) return;

        // Place real prefab once at preview position
        Instantiate(worldPrefab, activePreview.transform.position, activePreview.transform.rotation);

        // Remove from inventory + hotbar
        InventoryManager inv = user.GetComponent<InventoryManager>();
        if (inv != null)
        {
            inv.Remove(this, 1);
        }

        // Destroy preview and stop build mode
        Destroy(activePreview);
        activePreview = null;
        activePreview = null;
    }

    public void UpdatePreview(Vector3 position, Quaternion rotation)
    {
        if (activePreview == null) return;
        activePreview.transform.position = position;
        activePreview.transform.rotation = rotation;
    }
}
