using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInput;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerInput.InventoryActions inventory;
    private PlayerInput.HotbarActions hotbars;
    private PlayerMovement movement;
    private CameraLook camLook;
    private InventoryManager inventoryManager;
    private PlayerCollector collector;
    
    public bool interactPressed;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        inventory = playerInput.Inventory;
        movement = GetComponent<PlayerMovement>();
        camLook = GetComponent<CameraLook>();
        inventoryManager = GetComponent<InventoryManager>();
        collector = GetComponent<PlayerCollector>();
        hotbars = playerInput.Hotbar;

        onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();
        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
        onFoot.Interact.performed += ctx =>
        {
            if (ctx.performed && collector != null)
            {
                collector.TryCollect();
            }
        };
        inventory.ToggleInventory.performed += ctx => HandleInventoryToggle();
        inventory.PickSwap.performed += ctx => {
            if (inventoryManager != null && inventoryManager.IsOpen)
            inventoryManager.PickOrSwapItem();
    };

        hotbars.Hotbar1.performed += ctx => TryUseHotbar(1);
        hotbars.Hotbar2.performed += ctx => TryUseHotbar(2);
        hotbars.Hotbar3.performed += ctx => TryUseHotbar(3);
        hotbars.Hotbar4.performed += ctx => TryUseHotbar(4);
        hotbars.Hotbar5.performed += ctx => TryUseHotbar(5);
        hotbars.Hotbar6.performed += ctx => TryUseHotbar(6);



        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    

private void HandleInventoryToggle()
{
    if (inventoryManager == null) return;

    inventoryManager.ToggleInventory();

    bool isOpen = inventoryManager.IsOpen;

    if (isOpen)
    {
        // Inventory open - show mouse, unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }
    else
    {
        // Inventory closed - hide mouse, lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
    private void TryUseHotbar(int index)
    {
        if (inventoryManager == null) return;

        
        if (inventoryManager.IsOpen) return;

        inventoryManager.SelectHotbar(index);
    }






    void FixedUpdate()
    {
        if (inventoryManager != null && inventoryManager.IsOpen)
            
            return;
        movement.Move(onFoot.Movement.ReadValue<Vector2>());
        
    }

    void LateUpdate()
    {
        if (inventoryManager != null && inventoryManager.IsOpen)
            return;
        camLook.Look(onFoot.Look.ReadValue<Vector2>());
    }

    void OnEnable()
    {
        onFoot.Enable();
        inventory.Enable();
    }
    void OnDisable()
    {
        onFoot.Disable();
        inventory.Disable();
    }


}

