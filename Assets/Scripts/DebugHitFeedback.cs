using UnityEngine;

public class DebugDamageRaycast : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float range = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private DamageCause cause = DamageCause.PlayerAxe;
    [SerializeField] private KeyCode key = KeyCode.Mouse0;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(key)) return;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, range)) return;

        HealthBasedAsset asset = hit.collider.GetComponentInParent<HealthBasedAsset>();
        if (asset != null)
        {
            asset.TakeDamage(damage, cause);
        }
    }
}
