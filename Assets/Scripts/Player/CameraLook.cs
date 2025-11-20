using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("References")]
    public Camera cam;

    [Header("Sensitivity")]
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    private float xRot = 0f;

    [Header("FOV")]
    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float fovSmooth = 10f;

    [Header("Sprinting Bob")]
    public float bobAmount = 0.05f;
    public float bobSpeed = 12f;

    [Header("Slide Camera Tilt")]
    public float slideTilt = 12f; 
    public float tiltSmooth = 6f;

    [Header("Dodge Camera")]
    public float dodgePullBack = 0.35f;
    public float dodgeTiltAmount = 8f; 
    public float dodgeSmooth = 10f;

    [HideInInspector] public bool isSliding = false;
    [HideInInspector] public bool isDodging = false;
    [HideInInspector] public Vector3 dodgeDirection = Vector3.zero;

    [Header("State Flags")]
    public bool isSprinting = false;

    private Vector3 startLocalPos;
    private Vector3 dodgeOffset = Vector3.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startLocalPos = cam.transform.localPosition;
    }

    public void Look(Vector2 input)
    {
        // --- Mouse rotation ---
        float mouseX = input.x * xSensitivity * Time.deltaTime;
        float mouseY = input.y * ySensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        transform.Rotate(Vector3.up * mouseX);

        // --- Camera position (bob + dodge pullback) ---
        Vector3 targetPos = startLocalPos;


        if (isSprinting)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            targetPos += new Vector3(0f, bob, 0f);
        }


        Vector3 targetDodgeOffset = isDodging ? -dodgeDirection.normalized * dodgePullBack : Vector3.zero;
        dodgeOffset = Vector3.Lerp(dodgeOffset, targetDodgeOffset, Time.deltaTime * dodgeSmooth);
        targetPos += dodgeOffset;

        // Smooth camera position
        float activePosSmooth = 15f;   // 
        float returnPosSmooth = 8f;    

        float smooth = (isSliding || isDodging) ? activePosSmooth : returnPosSmooth;
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetPos, Time.deltaTime * smooth);


        // --- Camera rotation (tilt for slide and dodge) ---
        float tiltZ = 0f;
        float tiltX = 0f; 

        if (isSliding) tiltZ = slideTilt;

    
        if (isDodging)
        {
            Vector3 dodgeDirNormalized = dodgeDirection.normalized;
            tiltZ += Vector3.Dot(dodgeDirNormalized, transform.right) * dodgeTiltAmount;   
            tiltX += Vector3.Dot(dodgeDirNormalized, transform.forward) * dodgeTiltAmount; 
        }

        Quaternion targetRotation = Quaternion.Euler(xRot + tiltX, 0f, tiltZ);


        float rotationSmooth = tiltSmooth;
        if (!isSliding && !isDodging) rotationSmooth *= 1.8f;

        cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, targetRotation, Time.deltaTime * rotationSmooth);

        // --- FOV ---
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
    }
}

