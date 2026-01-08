using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        // Always face the camera
        transform.LookAt(Camera.main.transform.position);

        // OPTIONAL: lock the X/Z rotation so text stays upright
        Vector3 euler = transform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        euler.y += 180; // Face the camera directly
        transform.eulerAngles = euler;
    }
}
