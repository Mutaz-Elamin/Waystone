using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    private bool isGrounded;
    private bool movementEnabled = false;

    [Header("Movement")]
    public float gravity = -9.8f;
    public float speed = 5f;
    private float targetSpeed;
    public float jumpHeight = 1.5f;
    public CameraLook camMove;

    [Header("Crouch")]
    private bool lerpCrouch;
    private bool crouching = false;
    private float crouchTimer = 0;
    private float crouchVisualOffset = 0.5f;
    private Vector3 camStartPos;
    public float crouchLerpSpeed = 6f;
    public float slideHeight = 0.5f;
    public float normalHeight = 2f;

    [Header("Slide")]
    private bool isSliding = false;
    public float slideSpeed = 6f;
    public float slideDuration = 0.6f;
    private float slideTimer = 0f;

    [Header("Dodge")]
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    public float dodgeDuration = 0.3f;
    public float dodgeSpeed = 12f;
    private Vector3 dodgeDirection = Vector3.zero;

    // Double-tap dodge
    private float lastTapTimeForward = -1f;
    private float lastTapTimeBackward = -1f;
    private float lastTapTimeLeft = -1f;
    private float lastTapTimeRight = -1f;
    public float doubleTapThreshold = 0.25f;
    private Vector2 lastInput = Vector2.zero;

    [Header("Startup / Grounding")]
    [Tooltip("If true, player waits for terrain/collider before moving")]
    public bool waitForGroundAtStart = true;
    public float startGroundCheckDistance = 20f; // raycast distance downward
    public LayerMask groundLayer = ~0;           // terrain layer(s)
    public float startWaitTimeout = 5f;          // seconds max to wait
    public float spawnHeightOffset = 1f;         // player spawns slightly above terrain

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.enabled = false; // disable until terrain ready
        camMove = GetComponent<CameraLook>();
        camStartPos = camMove?.cam?.transform?.localPosition ?? Vector3.zero;
        targetSpeed = speed;
    }

    IEnumerator Start()
    {
        if (waitForGroundAtStart)
        {
            float startTime = Time.time;
            Collider groundCollider = null;

            while (Time.time - startTime < startWaitTimeout)
            {
                Physics.SyncTransforms(); // ensure procedural colliders are registered

                // Look for Terrain collider first
                Terrain t = Terrain.activeTerrain != null ? Terrain.activeTerrain : Object.FindFirstObjectByType<Terrain>();
                if (t != null) groundCollider = t.GetComponent<TerrainCollider>();

                // fallback to any MeshCollider
                if (groundCollider == null) groundCollider = Object.FindFirstObjectByType<MeshCollider>();

                if (groundCollider != null) break;

                yield return null; // wait for next frame
            }

            if (groundCollider != null)
            {
                Vector3 safePos = groundCollider.bounds.center + Vector3.up * (groundCollider.bounds.extents.y + spawnHeightOffset);
                transform.position = safePos;
            }
            else
            {
                Debug.LogWarning("No terrain collider found; player may fall through!");
                transform.position += Vector3.up * spawnHeightOffset; // still raise player
            }
        }

        controller.enabled = true;
        movementEnabled = true;
    }

    void Update()
    {
        if (!movementEnabled) return;

        isGrounded = CheckGrounded();

        // Gravity
        if (isGrounded && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f; // keep player snapped to ground
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // Crouch interpolation
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            Vector3 targetCamPos = crouching ? camStartPos - new Vector3(0, crouchVisualOffset, 0) : camStartPos;
            if (camMove?.cam != null)
                camMove.cam.transform.localPosition = Vector3.Lerp(camMove.cam.transform.localPosition, targetCamPos, Time.deltaTime * crouchLerpSpeed);

            float targetHeight = crouching ? slideHeight : normalHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchLerpSpeed);

            if (Vector3.Distance(camMove.cam.transform.localPosition, targetCamPos) < 0.01f &&
                Mathf.Abs(controller.height - targetHeight) < 0.01f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    public void Move(Vector2 input)
    {
        if (!movementEnabled) return;

        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        // Double-tap dodge detection
        if (!isSliding && !isDodging)
        {
            HandleDoubleTap(input);
        }

        // Sliding
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
            if (slideTimer <= 0f)
            {
                isSliding = false;
                controller.height = normalHeight;
                playerVelocity = Vector3.zero;
                if (camMove != null) camMove.isSliding = false;
            }
            return;
        }

        // Dodging
        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;
            Vector3 dodgeVel = dodgeDirection + Vector3.up * playerVelocity.y;
            controller.Move(dodgeVel * Time.deltaTime);
            if (dodgeTimer <= 0f)
            {
                isDodging = false;
                dodgeDirection = Vector3.zero;
                if (camMove != null)
                {
                    camMove.isDodging = false;
                    camMove.dodgeDirection = Vector3.zero;
                }
            }
            return;
        }

        // Normal movement
        speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * 5f);
        Vector3 horizontal = transform.TransformDirection(moveDir) * speed;
        Vector3 finalMove = (horizontal + Vector3.up * playerVelocity.y) * Time.deltaTime;
        controller.Move(finalMove);

        if (camMove != null) camMove.isMoving = isMoving;
    }

    private void HandleDoubleTap(Vector2 input)
    {
        if (input.x > 0.5f && lastInput.x <= 0.5f)
        {
            if (Time.time - lastTapTimeRight < doubleTapThreshold) Dodge(transform.right);
            lastTapTimeRight = Time.time;
        }
        else if (input.x < -0.5f && lastInput.x >= -0.5f)
        {
            if (Time.time - lastTapTimeLeft < doubleTapThreshold) Dodge(-transform.right);
            lastTapTimeLeft = Time.time;
        }

        if (input.y > 0.5f && lastInput.y <= 0.5f)
        {
            if (Time.time - lastTapTimeForward < doubleTapThreshold) Dodge(transform.forward);
            lastTapTimeForward = Time.time;
        }
        else if (input.y < -0.5f && lastInput.y >= -0.5f)
        {
            if (Time.time - lastTapTimeBackward < doubleTapThreshold) Dodge(-transform.forward);
            lastTapTimeBackward = Time.time;
        }

        lastInput = input;
    }

    public void Jump()
    {
        if ((isGrounded || CheckGroundedAllowance()) && !crouching)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity * 1.5f);
            playerVelocity.y = jumpVelocity;
        }
    }

    public void ToggleSprint()
    {
        if (crouching)
        {
            crouching = false;
            lerpCrouch = true;
        }
        if (camMove != null)
        {
            camMove.isSprinting = !camMove.isSprinting;
            targetSpeed = camMove.isSprinting ? 8f : 5f;
        }
    }

    public void Crouch(Vector2 input)
    {
        bool isMoving = input.sqrMagnitude > 0.01f;
        if (isGrounded)
        {
            if (camMove != null && camMove.isSprinting && isMoving)
            {
                Slide();
                return;
            }
            crouching = !crouching;
            crouchTimer = 0f;
            lerpCrouch = true;
            targetSpeed = crouching ? 2f : 5f;
        }
    }

    public void Slide()
    {
        if (isGrounded && camMove != null && camMove.isSprinting && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            controller.height = slideHeight;
            playerVelocity = transform.forward * slideSpeed + Vector3.up * -2f;
            camMove.isSliding = true;
        }
    }

    public void Dodge(Vector3 direction)
    {
        if (isDodging || !isGrounded) return;
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeDirection = direction.normalized * dodgeSpeed;
        if (camMove != null)
        {
            camMove.isDodging = true;
            camMove.dodgeDirection = direction;
        }
    }

    private bool CheckGrounded()
    {
        if (controller.isGrounded) return true;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, 0.2f, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private bool CheckGroundedAllowance()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, 0.7f, groundLayer, QueryTriggerInteraction.Ignore);
    }
}