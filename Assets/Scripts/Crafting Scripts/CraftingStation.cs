// =======================
// CraftingStation.cs
// Attach this to Workbench / Forge GameObjects in the scene.
// Make sure the station has a trigger collider (Is Trigger = true).
// =======================
using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    [SerializeField] private CraftingMenu.StationType stationType = CraftingMenu.StationType.Workbench;

    public void Interact(PlayerCollector player)
    {
        if (player == null) return;

        InventoryManager inv = player.GetComponent<InventoryManager>();
        if (inv == null) return;

        inv.OpenInventoryAndCrafting(stationType);

        // Make cursor match inventory open state using InputManager logic
        InputManager input = player.GetComponent<InputManager>();
        if (input != null) input.SyncCursorToInventory();
    }
}
