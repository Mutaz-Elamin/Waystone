using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camMove = GetComponent<CameraLook>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded; 
    }

    public void Move(Vector2 input)
    {
        Vector3 moveDir = Vector3.zero;
        moveDir.x = input.x;
        moveDir.z = input.y;
        controller.Move(transform.TransformDirection(moveDir * speed * Time.deltaTime));
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {

        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void StartSprinting()
    {
        if (isGrounded) {
            speed = 8f;
            camMove.isSprinting = true;
        }
        
    }

    public void StopSprinting()
    {
        speed = 5f;
        camMove.isSprinting = false;
    }
}
