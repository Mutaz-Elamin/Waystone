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




void FixedUpdate()
    {

        movement.Move(onFoot.Movement.ReadValue<Vector2>());
        
    }

    void LateUpdate()
    {

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

