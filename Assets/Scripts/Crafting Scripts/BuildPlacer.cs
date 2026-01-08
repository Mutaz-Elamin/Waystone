using UnityEngine;

public class BuildPlacer : MonoBehaviour
{
 
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxPlaceDistance = 4f;
    [SerializeField] private float yOffset = 0.02f;


    [SerializeField] private LayerMask blockedMask;
    [SerializeField] private Vector3 overlapHalfExtents = new Vector3(0.6f, 0.5f, 0.6f);


    [SerializeField] private Vector3 rotationOffsetEuler;

    private Camera cam;
    private InventoryManager inventory;


    private CraftedWorkbenches equippedPlaceable;


    private GameObject previewInstance;

  
    private Vector3 lastPos;
    private Quaternion lastRot;
    private bool hasValidPose;

    void Awake()
    {
        cam = Camera.main;
        inventory = GetComponent<InventoryManager>();
    }


    public void Equip(ItemClass item)
    {
      
        if (item == null || item is not CraftedWorkbenches placeable)
        {
            CancelBuildMode();
            return;
        }

        // If switching to a different placeable, restart preview
        if (equippedPlaceable != placeable)
        {
            equippedPlaceable = placeable;
            CreatePreview();
        }
    }

   
    public void TryPlace()
    {
        if (equippedPlaceable == null) return;

        
        if (inventory != null && inventory.IsOpen) return;

     
        if (!hasValidPose) return;


        if (!IsValidPlacement(lastPos, lastRot)) return;

        // Place it
        if (equippedPlaceable.worldPrefab != null)
        {
            Instantiate(equippedPlaceable.worldPrefab, lastPos, lastRot);
        }

     
        if (inventory != null)
        {
            inventory.Remove(equippedPlaceable, 1);
            inventory.RefreshHotbarUI(); 
        }

        // Stop preview/build mode (prevents placing multiple)
        CancelBuildMode();
    }

    void Update()
    {
        if (equippedPlaceable == null) return;


        if (inventory != null && inventory.IsOpen)
        {
            CancelBuildMode();
            return;
        }

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (previewInstance == null) CreatePreview();
        if (previewInstance == null) return;

        // Raycast from screen center to ground
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, maxPlaceDistance, groundMask))
        {
            previewInstance.SetActive(false);
            hasValidPose = false;
            return;
        }

        previewInstance.SetActive(true);

      
        lastPos = hit.point + Vector3.up * yOffset;
        lastRot = Quaternion.Euler(rotationOffsetEuler);

        // Move preview
        previewInstance.transform.SetPositionAndRotation(lastPos, lastRot);

        
        hasValidPose = IsValidPlacement(lastPos, lastRot);
    }

    private void CreatePreview()
    {
        DestroyPreview();

        if (equippedPlaceable == null) return;
        if (equippedPlaceable.previewPrefab == null) return;

        previewInstance = Instantiate(equippedPlaceable.previewPrefab);
        previewInstance.name = "PlaceablePreview";

        // Keep preview size consistent
        previewInstance.transform.localScale = equippedPlaceable.previewPrefab.transform.localScale;
        previewInstance.transform.rotation = Quaternion.Euler(rotationOffsetEuler);

    
    }

    private void DestroyPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        hasValidPose = false;
    }

    public void CancelBuildMode()
    {
        equippedPlaceable = null;
        DestroyPreview();
    }

    private bool IsValidPlacement(Vector3 pos, Quaternion rot)
    {
        // Overlap check against blockedMask
        Collider[] hits = Physics.OverlapBox(pos, overlapHalfExtents, rot, blockedMask);
        return hits.Length == 0;
    }

    void OnDrawGizmosSelected()
    {
        if (previewInstance == null) return;
        Gizmos.matrix = Matrix4x4.TRS(previewInstance.transform.position, previewInstance.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, overlapHalfExtents * 2f);
    }
}
