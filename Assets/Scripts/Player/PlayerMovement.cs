using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    private CharacterController controller;
    private Vector3 playerVelocity;

    private bool isGrounded;
    public float gravity = -9.8f;
    public float speed = 2f;
    public float jumpHeight = 1.5f;
    CameraLook camMove;

    //Crouch Related Variables
    private bool lerpCrouch;
    private bool crouching = false;
    public float crouchTimer = 0;

    //Slide Related Vars
    private bool isSliding = false;
    public float slideSpeed = 6f;
    public float slideDuration = 0.6f;
    private float slideTimer = 0f;
    public float slideHeight = 0.5f;
    public float normalHeight = 2f;
    public float slideControlRed = 0.5f;


    private bool isDodging = false;
    private float dodgeTimer = 0f;
    public float dodgeDuration = 0.3f;
    public float dodgeSpeed = 12f;
    private Vector3 dodgeDirection = Vector3.zero;

    private float lastTapTimeForward = -1f;
    private float lastTapTimeBackward = -1f;
    private float lastTapTimeLeft = -1f;
    private float lastTapTimeRight = -1f;
    public float doubleTapThreshold = 0.25f;
    private Vector2 lastInput = Vector2.zero;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        camMove = GetComponent<CameraLook>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= p;
            if (crouching)
            {
                controller.height = Mathf.Lerp(controller.height, 1, p);
                speed = 1.7f;
            }
            else
            {
                controller.height = Mathf.Lerp(controller.height, 2, p);
                speed = 2;
            }

            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0f;

            }

        }

    }

    public void Move(Vector2 input)
    {
        // --- 0. Double-tap dodge detection ---

        if (!isSliding && !isDodging)
        {
            // Right

            if (input.x > 0.5f && lastInput.x <= 0.5f)
            {
                if (Time.time - lastTapTimeRight < doubleTapThreshold)
                    Dodge(transform.right);
                lastTapTimeRight = Time.time;
            }

            // Left
            else if (input.x < -0.5f && lastInput.x >= -0.5f)
            {
                if (Time.time - lastTapTimeLeft < doubleTapThreshold)
                    Dodge(-transform.right);
                lastTapTimeLeft = Time.time;
            }

            // Forward
            if (input.y > 0.5f && lastInput.y <= 0.5f)
            {
                if (Time.time - lastTapTimeForward < doubleTapThreshold)
                    Dodge(transform.forward);
                lastTapTimeForward = Time.time;
            }

            // Backward
            else if (input.y < -0.5f && lastInput.y >= -0.5f)
            {
                if (Time.time - lastTapTimeBackward < doubleTapThreshold)
                    Dodge(-transform.forward);
                lastTapTimeBackward = Time.time;
            }

            lastInput = input; 
        }

        // --- 1. Slide ---
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

            return; // skip normal movement while sliding
        }

        // --- 2. Dodge ---
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

        // --- 3. Normal movement ---
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);

        // Gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);
    }


    public void Jump()
    {

        if (isGrounded && !crouching)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void StartSprinting()
    {
        if (isGrounded && !crouching)
        {
            speed = 8f;
            camMove.isSprinting = true;

        }

    }

    public void StopSprinting()
    {
        speed = 5f;
        camMove.isSprinting = false;
    }

    public void Crouch()
    {
        if (isGrounded)
        {
            if (camMove.isSprinting)
            {
                Slide();
                return;
            }
   
            
                crouching = !crouching;
                crouchTimer = 0f;
                lerpCrouch = true;
            }


    }

    public void Slide()
    {
        if (isGrounded && camMove.isSprinting && !isSliding)
        {

            isSliding = true;
            slideTimer = slideDuration;

            controller.height = slideHeight;

            Vector3 forward = transform.forward;
            playerVelocity = forward * slideSpeed;
            playerVelocity.y = -2f;

            camMove.isSliding = true;

        }
    }


    public void Dodge(Vector3 direction)
    {
        if (isDodging || !isGrounded) return; // only once per dodge
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeDirection = direction.normalized * dodgeSpeed;

        camMove.isDodging = true;
        camMove.dodgeDirection = direction;
    }
}


