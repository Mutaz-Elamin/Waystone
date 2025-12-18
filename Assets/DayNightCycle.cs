using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public Light sun;

    [Header("Settings")]
    public float dayLengthInSeconds = 60f; // full 24h cycle in game

    private void Update()
    {
        if (sun == null || dayLengthInSeconds <= 0f) return;

        // 360 degrees per full day
        float rotationPerSecond = 360f / dayLengthInSeconds;
        sun.transform.Rotate(Vector3.right * rotationPerSecond * Time.deltaTime);
    }
}
