using UnityEngine;

public class CameraLook : MonoBehaviour
{
    // NOTE: This MUST be manually connected in the Inspector
    public Camera cam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    private float xRot = 0f;

    // FOV and Bob variables...
    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float fovSmooth = 10f;
    public float bobAmount = 0.05f;
    public float bobSpeed = 12f;

    // Slide and Dodge variables...
    public float slideTilt = 6f;
    public float tiltSmooth = 6f;
    public float dodgePullBack = 0.35f;
    public float dodgeTiltAmount = 8f;
    public float dodgeSmooth = 10f;

    // State Flags (public so PlayerMovement can control them)
    [HideInInspector] public bool isSliding = false;
    [HideInInspector] public bool isDodging = false;
    [HideInInspector] public Vector3 dodgeDirection = Vector3.zero;
    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isMoving = false;

    private Vector3 startLocalPos;
    private Vector3 dodgeOffset = Vector3.zero;

    void Start()
    {
        // CRITICAL: Lock the cursor for FPP
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cam == null)
        {
            Debug.LogError("Camera reference (cam) in CameraLook is missing! Please drag your camera object into the slot.");
            enabled = false;
            return;
        }

        startLocalPos = cam.transform.localPosition;
    }

    public void Look(Vector2 input)
    {
        // Mouse look calculations
        float mouseX = input.x * xSensitivity * Time.deltaTime;
        float mouseY = input.y * ySensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);
        transform.Rotate(Vector3.up * mouseX);

        // Camera positioning (Head Bob / Dodge Pullback)
        Vector3 targetPos = startLocalPos;
        if (isSprinting && isMoving)
            targetPos += new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobAmount, 0f);

        Vector3 targetDodgeOffset = isDodging ? -dodgeDirection.normalized * dodgePullBack : Vector3.zero;
        dodgeOffset = Vector3.Lerp(dodgeOffset, targetDodgeOffset, Time.deltaTime * dodgeSmooth);
        targetPos += dodgeOffset;

        float smooth = (isSliding || isDodging) ? 15f : 8f;
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetPos, Time.deltaTime * smooth);

        // Camera rotation (Pitch and Tilt)
        float tiltZ = isSliding ? slideTilt : 0f;
        float tiltX = 0f;
        // Dodge tilt logic...
        if (isDodging)
        {
            Vector3 dodgeDirNormalized = dodgeDirection.normalized;
            tiltZ += Vector3.Dot(dodgeDirNormalized, transform.right) * dodgeTiltAmount;
            tiltX += Vector3.Dot(dodgeDirNormalized, transform.forward) * dodgeTiltAmount;
        }

        Quaternion targetRotation = Quaternion.Euler(xRot + tiltX, 0f, tiltZ);
        cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, targetRotation, Time.deltaTime * tiltSmooth * (isSliding || isDodging ? 1f : 1.8f));

        // FOV smoothing
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, isSprinting ? sprintFOV : normalFOV, Time.deltaTime * fovSmooth);
    }
}