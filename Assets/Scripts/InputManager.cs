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
    private PlayerAttack attack;


    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        movement = GetComponent<PlayerMovement>();
        camLook = GetComponent<CameraLook>();
        attack = GetComponent<PlayerAttack>();

        onFoot.Jump.performed += ctx => movement.Jump();
        onFoot.Sprint.performed += ctx => movement.ToggleSprint();
        onFoot.Crouch.performed += ctx => movement.Crouch(onFoot.Movement.ReadValue<Vector2>());
        onFoot.LightAttack.performed += ctx => attack.LightAttack();

 // onFoot.HeavyAttack.performed += ctx => attack.HeavyAttack();
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