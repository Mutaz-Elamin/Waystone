using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerInput.InventoryActions inventory;
    private PlayerInput.HotbarActions hotbars;
    private PlayerMovement movement;
    private CameraLook camLook;
    private PlayerAttack attack;
    private InventoryManager inventoryManager;
    private PlayerCollector collector;
    
    public bool interactPressed;
    private WeaponsManager weaponsManager;


    void Awake()
    {

        movement = GetComponent<PlayerMovement>();
        camLook = GetComponent<CameraLook>();
        attack = GetComponent<PlayerAttack>();
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        inventory = playerInput.Inventory;
        inventoryManager = GetComponent<InventoryManager>();
        collector = GetComponent<PlayerCollector>();

        hotbars = playerInput.Hotbar;

        weaponsManager = GetComponent<WeaponsManager>();


        if (movement == null || camLook == null)
        {
            Debug.LogError("InputManager is missing PlayerMovement or CameraLook components on the GameObject.");
            enabled = false;
            return;
        }


        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

       // onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Jump.performed += ctx => weaponsManager.SpawnTestWeapon(weaponsManager.stickPrefab);
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();

        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
        onFoot.LightAttack.performed += ctx => attack.LightAttack();
        onFoot.LightAttack.canceled += ctx => attack.StopLightAttack();

        onFoot.HeavyAttack.performed += ctx => attack.StartHeavyCharge();
        onFoot.HeavyAttack.canceled += ctx => attack.ReleaseHeavyAttack();

        onFoot.Defend.performed += ctx => attack.StartDefend();
        onFoot.Defend.canceled += ctx => attack.StopDefend();
        onFoot.ToggleWeapon.performed += ctx => attack.ToggleWeaponDraw();

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

        hotbars.Hotbar1.performed += ctx => TryUseHotbar(0);
        hotbars.Hotbar2.performed += ctx => TryUseHotbar(1);
        hotbars.Hotbar3.performed += ctx => TryUseHotbar(2);
        hotbars.Hotbar4.performed += ctx => TryUseHotbar(3);
        hotbars.Hotbar5.performed += ctx => TryUseHotbar(4);
        hotbars.Hotbar6.performed += ctx => TryUseHotbar(5);
        BuildPlacer placer = GetComponent<BuildPlacer>();

        hotbars.Use.performed += ctx =>
        {
            if (inventoryManager == null) return;
            if (inventoryManager.IsOpen) return;

            inventoryManager.UseSelectedHotbarItem();
        };
        inventory.Drop.performed += ctx =>
        {
            if (inventoryManager == null) return;
            if (!inventoryManager.IsOpen) return;

            inventoryManager.DropClosestSlot(1); // drop 1
        };



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
    public void SyncCursorToInventory()
    {
        if (inventoryManager == null) return;

        if (inventoryManager.IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
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
<<<<<<< HEAD:Assets/Scripts/InputManager.cs
        if (inventoryManager != null && inventoryManager.IsOpen)
            return;
=======
 
>>>>>>> TieredItemsAndMusic:Assets/Scripts/MiscManager/InputManager.cs
        camLook.Look(onFoot.Look.ReadValue<Vector2>());
    }

    // --- FIX APPLIED HERE ---
    void OnEnable()
    {
        
        if (playerInput != null)
        {
            onFoot.Enable();
        }
        inventory.Enable();
        hotbars.Enable();
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            onFoot.Disable();
        }
        inventory.Disable();
        hotbars.Disable();
    }
}
