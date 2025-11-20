using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour

{
    public Camera cam;
    private float xRota = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float sprintOffset = 2.2f;
    public float sprintSmooth = 5f;
    public bool isSprinting = false;

    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float fovSmooth = 5f;
    public float sprintBobAmount = 1f;
    public float sprintBobSpeed = 10f;

    private Vector3 originalPos;


    public float shakeAmount = 0.03f;
    public float shakeDuration = 0.1f;

    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 shakeTarget = Vector3.zero;
    private float shakeTimer = 0f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRota = 0f;
        cam.transform.rotation = Quaternion.identity;
        originalPos = cam.transform.localPosition;
    }



        public void Look(Vector2 input) {

        float mouseX = input.x * xSensitivity * Time.deltaTime;
        float mouseY = input.y * ySensitivity * Time.deltaTime;

        xRota -= mouseY;
        xRota = Mathf.Clamp(xRota, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRota, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        Vector3 targetPos = originalPos;
        if (isSprinting)
        {
            targetPos += new Vector3(0f, 0f, sprintOffset);
            float bobOffset = Mathf.Sin(Time.time * sprintBobSpeed) * sprintBobAmount;
            targetPos += new Vector3(0f, bobOffset, 0f);

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                if (Vector3.Distance(shakeOffset, shakeTarget) < 0.01f)
                {
                    shakeTarget = new Vector3(Random.Range(-shakeAmount, shakeAmount),
                        Random.Range(-shakeAmount, shakeAmount), 0f);
                }
                shakeOffset = Vector3.Lerp(shakeOffset, shakeTarget, Time.deltaTime * 10f);
            }
            else
            {
                shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, Time.deltaTime * 5f);
            }
        }

        targetPos += shakeOffset;
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetPos, Time.deltaTime * sprintSmooth);

        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);

    }
}


