using UnityEngine;

[DisallowMultipleComponent]
public class CameraClipToChunks : MonoBehaviour
{
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private Camera targetCamera;

    [Header("Tuning")]
    [SerializeField] private bool useCornerDistance = false;
    [SerializeField, Range(0.1f, 2f)] private float distanceMultiplier = 1f;
    [SerializeField, Min(0f)] private float padding = 0f;
    [SerializeField, Min(0f)] private float maxFarClip = 500f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (worldGrid == null)
            worldGrid = Object.FindFirstObjectByType<WorldGrid>();

        if (worldGrid == null || targetCamera == null) return;

        float chunkSize = worldGrid.ChunkSize;
        int radius = worldGrid.ActiveChunkRadius;
        float edgeDistance = (radius + 1) * chunkSize;
        float baseDistance = useCornerDistance ? (Mathf.Sqrt(2f) * edgeDistance) : edgeDistance;
        float desiredFar = baseDistance * distanceMultiplier + padding;

        if (maxFarClip > 0f) desiredFar = Mathf.Min(desiredFar, maxFarClip);
        desiredFar = Mathf.Max(desiredFar, targetCamera.nearClipPlane + 1f);

        targetCamera.farClipPlane = desiredFar;
    }
}
