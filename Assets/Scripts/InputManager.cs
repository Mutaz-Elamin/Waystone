using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement movement;
    private CameraLook camLook;

    void Awake()
    {
        // Get references to external scripts
        movement = GetComponent<PlayerMovement>();
        camLook = GetComponent<CameraLook>();

        if (movement == null || camLook == null)
        {
            Debug.LogError("InputManager is missing PlayerMovement or CameraLook components on the GameObject.");
            enabled = false;
            return;
        }

        // Initialize Input System components
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        // Subscribe to Instantaneous Actions
        onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();

        // Pass the current Movement stick/key value to Crouch for the slide check
        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
    }

    void FixedUpdate()
    {
        // Continuous reading for physics-based movement
        movement.Move(onFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        // Continuous reading for camera look (runs after all movement)
        camLook.Look(onFoot.Look.ReadValue<Vector2>());
    }

    // --- FIX APPLIED HERE ---
    void OnEnable()
    {
        // If Awake() ran, onFoot is guaranteed to be assigned.
        // We only need to check if playerInput itself is assigned for safety.
        if (playerInput != null)
        {
            onFoot.Enable();
        }
    }

    void OnDisable()
    {
        // Same logic: if playerInput is assigned, onFoot is too.
        if (playerInput != null)
        {
            onFoot.Disable();
        }
    }
    // ------------------------
}