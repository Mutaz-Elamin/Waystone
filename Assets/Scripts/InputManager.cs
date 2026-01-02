using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerInput.InventoryActions inventory;
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
        weaponsManager = GetComponent<WeaponsManager>();

        if (movement == null || camLook == null)
        {
            Debug.LogError("InputManager is missing PlayerMovement or CameraLook components on the GameObject.");
            enabled = false;
            return;
        }


        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();

        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
        onFoot.LightAttack.performed += ctx => attack.LightAttack();
        onFoot.LightAttack.canceled += ctx => attack.StopLightAttack();

        onFoot.HeavyAttack.performed += ctx => attack.StartHeavyCharge();
        onFoot.HeavyAttack.canceled += ctx => attack.ReleaseHeavyAttack();

        onFoot.Defend.performed += ctx => attack.StartDefend();
        onFoot.Defend.canceled += ctx => attack.StopDefend();
        onFoot.ToggleWeapon.performed += ctx => weaponsManager.ToggleWeapon();

        onFoot.Interact.performed += ctx =>
        {
            if (ctx.performed && collector != null)
            {
                collector.TryCollect();
            }
        };
        inventory.ToggleInventory.performed += ctx => inventoryManager.ToggleInventory();
    }

    void FixedUpdate()
    {
        movement.Move(onFoot.Movement.ReadValue<Vector2>());
        
    }

    void LateUpdate()
    {
 
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
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            onFoot.Disable();
        }
        inventory.Disable();
    }
}
