using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInput;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerMovement movement;
    private CameraLook camLook;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        movement = GetComponent<PlayerMovement>();
        camLook = GetComponent<CameraLook>();

        onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();
        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
    }

    void FixedUpdate()
    {
        movement.Move(onFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        camLook.Look(onFoot.Look.ReadValue<Vector2>());
    }

    void OnEnable() => onFoot.Enable();
    void OnDisable() => onFoot.Disable();
}