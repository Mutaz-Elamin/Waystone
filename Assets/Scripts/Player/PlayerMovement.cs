using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    private bool isGrounded;
    public float gravity = -9.8f;
    public float speed = 5f;
    public float targetSpeed;
    public float jumpHeight = 1.5f;
    public CameraLook camMove;

    // Crouch
    private bool lerpCrouch;
    private bool crouching = false;
    public float crouchTimer = 0;
    private float crouchVisualOffset = 0.5f;
    private Vector3 camStartPos;
    public float crouchLerpSpeed = 6f;

    // Slide
    private bool isSliding = false;
    public float slideSpeed = 6f;
    public float slideDuration = 0.6f;
    private float slideTimer = 0f;
    public float slideHeight = 0.5f;
    public float normalHeight = 2f;
    public float slideStaminaCost = 20f; // New: Stamina cost for a slide

    // Dodge
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    public float dodgeDuration = 0.3f;
    public float dodgeSpeed = 12f;
    private Vector3 dodgeDirection = Vector3.zero;
    public float dodgeStaminaCost = 15f; // New: Stamina cost for a dodge

    // Double-tap dodge
    private float lastTapTimeForward = -1f;
    private float lastTapTimeBackward = -1f;
    private float lastTapTimeLeft = -1f;
    private float lastTapTimeRight = -1f;
    public float doubleTapThreshold = 0.25f;
    private Vector2 lastInput = Vector2.zero;

    // --- Survival Stats Integration ---
    private PlayerStats playerStats;
    public float sprintStaminaCostRate = 5f; // New: Stamina consumed per second while sprinting
    public float lowHungerSpeedPenalty = 0.5f; // New: Speed multiplier when hunger is low
    // -----------------------------------

    void Start()
    {
        targetSpeed = speed;
        controller = GetComponent<CharacterController>();
        camMove = GetComponent<CameraLook>();
        camStartPos = camMove.cam.transform.localPosition;

        // INTEGRATION: Get the PlayerStats component
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats script missing! Stamina features will be disabled.");
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = Mathf.Clamp01(crouchTimer);

            Vector3 targetCamPos = crouching
                ? camStartPos - new Vector3(0, crouchVisualOffset, 0)
                : camStartPos;

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
        playerVelocity.y += gravity * Time.deltaTime;
        if (playerVelocity.y < 0)
        {
            playerVelocity.y += gravity * (1.5f - 1f) * Time.deltaTime;
        }

        // INTEGRATION: Continuous Stamina consumption while sprinting
        if (playerStats != null && camMove.isSprinting)
        {
            if (!playerStats.ConsumeStamina(sprintStaminaCostRate * Time.deltaTime))
            {
                // If stamina runs out, force out of sprint
                ToggleSprint();
            }
        }
    }

    public void Move(Vector2 input)
    {
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        // --- Double-tap dodge ---
        if (!isSliding && !isDodging)
        {
            // ... (existing double-tap logic) ...

            if (input.x > 0.5f && lastInput.x <= 0.5f)
            {
                if (Time.time - lastTapTimeRight < doubleTapThreshold)
                    Dodge(transform.right);
                lastTapTimeRight = Time.time;
            }
            else if (input.x < -0.5f && lastInput.x >= -0.5f)
            {
                if (Time.time - lastTapTimeLeft < doubleTapThreshold)
                    Dodge(-transform.right);
                lastTapTimeLeft = Time.time;
            }

            if (input.y > 0.5f && lastInput.y <= 0.5f)
            {
                if (Time.time - lastTapTimeForward < doubleTapThreshold)
                    Dodge(transform.forward);
                lastTapTimeForward = Time.time;
            }
            else if (input.y < -0.5f && lastInput.y >= -0.5f)
            {
                if (Time.time - lastTapTimeBackward < doubleTapThreshold)
                    Dodge(-transform.forward);
                lastTapTimeBackward = Time.time;
            }

            lastInput = input;
        }

        // --- Slide ---
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);

            if (slideTimer <= 0f)
            {
                isSliding = false;
                controller.height = normalHeight;
                playerVelocity = Vector3.zero;
                camMove.isSliding = false;
            }
            return;
        }

        // --- Dodge ---
        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;
            controller.Move(dodgeDirection * Time.deltaTime);
            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);

            if (dodgeTimer <= 0f)
            {
                isDodging = false;
                dodgeDirection = Vector3.zero;
                camMove.isDodging = false;
                camMove.dodgeDirection = Vector3.zero;
            }
            return;
        }

        // --- Normal movement ---

        // INTEGRATION: Apply hunger speed penalty
        float hungerPenalty = 1f;
        if (playerStats != null && playerStats.currentHunger <= 20f)
        {
            hungerPenalty = lowHungerSpeedPenalty;
        }

        float currentTargetSpeed = targetSpeed * hungerPenalty;
        speed = Mathf.Lerp(speed, currentTargetSpeed, Time.deltaTime * 5f);
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);

        // Gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;
        controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);

        camMove.isMoving = isMoving;
    }

    public void Jump()
    {
        // INTEGRATION: Can add a stamina cost for jumping here if needed
        if (isGrounded && !crouching)
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

        // INTEGRATION: Only allow toggling ON sprint if stamina is available or if turning OFF sprint
        if (!camMove.isSprinting || (playerStats != null && playerStats.currentStamina > 0))
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
            if (camMove.isSprinting && isMoving)
            {
                Slide();
                return;
            }

            crouching = !crouching;
            crouchTimer = 0f;
            lerpCrouch = true;

            if (crouching)
            {
                targetSpeed = 2f;
            }
            else
            {
                targetSpeed = 5f;
            }
        }
    }

    public void Slide()
    {
        // INTEGRATION: Check and consume stamina before sliding
        if (isGrounded && camMove.isSprinting && !isSliding)
        {
            if (playerStats != null && playerStats.ConsumeStamina(slideStaminaCost))
            {
                isSliding = true;
                slideTimer = slideDuration;
                controller.height = slideHeight;
                Vector3 forward = transform.forward;
                playerVelocity = forward * slideSpeed;
                playerVelocity.y = -2f;
                camMove.isSliding = true;
                ToggleSprint(); // Optional: Stop sprinting after sliding
            }
            else
            {
                Debug.Log("Not enough stamina to slide!");
            }
        }
    }

    public void Dodge(Vector3 direction)
    {
        // INTEGRATION: Check and consume stamina before dodging
        if (isDodging || !isGrounded) return;

        if (playerStats != null && playerStats.ConsumeStamina(dodgeStaminaCost))
        {
            isDodging = true;
            dodgeTimer = dodgeDuration;
            dodgeDirection = direction.normalized * dodgeSpeed;
            camMove.isDodging = true;
            camMove.dodgeDirection = direction;
        }
        else
        {
            Debug.Log("Not enough stamina to dodge!");
        }
    }
}