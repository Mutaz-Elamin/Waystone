using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private PlayerStats playerStats; // Required for stat integration

    private bool isGrounded;
    public float gravity = -9.8f;
    public float speed = 5f;
    public float targetSpeed;
    public float jumpHeight = 1.5f;
    public CameraLook camMove; // CRITICAL: Must be connected in Inspector

    // --- Stamina Costs ---
    [Header("Stamina Costs")]
    public float sprintStaminaCostRate = 5f; // Consumed per second
    public float jumpStaminaCost = 10f; // Consumed per jump
    public float slideStaminaCost = 20f;
    public float dodgeStaminaCost = 15f;
    // ---------------------

    // Crouch and Slide logic variables...
    private bool lerpCrouch;
    private bool crouching = false;
    public float crouchTimer = 0;
    private float crouchVisualOffset = 0.5f;
    private Vector3 camStartPos;
    public float crouchLerpSpeed = 6f;

    private bool isSliding = false;
    public float slideSpeed = 6f;
    public float slideDuration = 0.6f;
    private float slideTimer = 0f;
    public float slideHeight = 0.5f;
    public float normalHeight = 2f;

    // Dodge logic variables...
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    public float dodgeDuration = 0.3f;
    public float dodgeSpeed = 12f;
    private Vector3 dodgeDirection = Vector3.zero;

    // Double-tap logic variables...
    private float lastTapTimeForward = -1f;
    private float lastTapTimeBackward = -1f;
    private float lastTapTimeLeft = -1f;
    private float lastTapTimeRight = -1f;
    public float doubleTapThreshold = 0.25f;
    private Vector2 lastInput = Vector2.zero;

    public float lowHungerSpeedPenalty = 0.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();

        if (camMove == null)
        {
            Debug.LogError("PlayerMovement: camMove (CameraLook reference) is MISSING in the Inspector. Movement will fail.");
            enabled = false;
            return;
        }

        if (camMove.cam != null)
        {
            camStartPos = camMove.cam.transform.localPosition;
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats script missing! Stamina features will be disabled.");
        }

        targetSpeed = speed;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        // Crouch Lerp Logic (omitted for brevity)
        // ...

        // Apply Gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (playerVelocity.y < 0)
        {
            playerVelocity.y += gravity * (1.5f - 1f) * Time.deltaTime;
        }

        // Set Stamina Consumption flag for PlayerStats
        bool isCurrentlyConsumingStamina = isSliding || isDodging;

        // Continuous Stamina consumption while sprinting
        if (playerStats != null && camMove.isSprinting)
        {
            isCurrentlyConsumingStamina = true; // Sprinting consumes stamina
            if (!playerStats.ConsumeStamina(sprintStaminaCostRate * Time.deltaTime))
            {
                // If stamina runs out, force out of sprint
                ToggleSprint(forceOff: true);
            }
        }

        // Update the stat manager about consumption state
        if (playerStats != null)
        {
            playerStats.isConsumingStamina = isCurrentlyConsumingStamina;
            playerStats.isJumping = !isGrounded && playerVelocity.y > 0.01f;
        }
    }

    public void Move(Vector2 input)
    {
        // ... (Slide, Dodge logic omitted for brevity) ...

        // --- Normal movement execution ---
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        if (isSliding || isDodging)
        {
            // Handle movement for slide/dodge
            // ... (You will need to manually implement the slide/dodge movement here, as it was omitted for brevity) ...
            return;
        }

        // Apply hunger speed penalty
        float hungerPenalty = 1f;
        if (playerStats != null && playerStats.CurrentHunger <= 20f)
        {
            hungerPenalty = lowHungerSpeedPenalty;
        }

        float currentTargetSpeed = targetSpeed * hungerPenalty;
        speed = Mathf.Lerp(speed, currentTargetSpeed, Time.deltaTime * 5f);
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);

        // Apply Gravity
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;
        controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);

        if (camMove != null) camMove.isMoving = isMoving;
    }

    public void Jump()
    {
        // INTEGRATION: Check and consume stamina for jumping
        if (isGrounded && !crouching && playerStats != null)
        {
            if (playerStats.ConsumeStamina(jumpStaminaCost))
            {
                float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity * 1.5f);
                playerVelocity.y = jumpVelocity;
            }
            else
            {
                Debug.Log("Not enough stamina to jump!");
            }
        }
    }

    public void ToggleSprint(bool forceOff = false)
    {
        if (crouching)
        {
            crouching = false;
            lerpCrouch = true;
        }

        if (camMove == null) return;

        if (forceOff || camMove.isSprinting)
        {
            camMove.isSprinting = false;
            targetSpeed = 5f;
        }
        else if (playerStats != null && playerStats.CurrentStamina > 0)
        {
            camMove.isSprinting = true;
            targetSpeed = 8f;
        }
    }

    public void Crouch(Vector2 input) { /* omitted for brevity */ }
    public void Slide() { /* omitted for brevity */ }
    public void Dodge(Vector3 direction) { /* omitted for brevity */ }
}