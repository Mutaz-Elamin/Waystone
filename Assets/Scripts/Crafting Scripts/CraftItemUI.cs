using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftItemUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private CraftingRecipe recipe;

    [Header("UI refs")]
    [SerializeField] private Image itemIcon;                 // ItemImage/Image
    [SerializeField] private TextMeshProUGUI itemNameText;   // ItemName
    [SerializeField] private TextMeshProUGUI requirementsText; // Requirements
    [SerializeField] private Button craftButton;             // CraftButton
    [SerializeField] private TextMeshProUGUI craftButtonText; // CraftButton/Text (TMP) optional

    private InventoryManager inventory;

    private void Start()
    {
        // InventoryManager is on the Player, so just find it in scene
        inventory = FindAnyObjectByType<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("CraftItemUI: Could not find InventoryManager in the scene.", this);
            return;
        }

        if (recipe == null)
        {
            Debug.LogError("CraftItemUI: Recipe not assigned.", this);
            return;
        }

        if (craftButton == null)
        {
            Debug.LogError("CraftItemUI: CraftButton ref not assigned.", this);
            return;
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(TryCraft);

      
    }


    

    private void TryCraft()
    {
        Debug.Log("Craft button clicked!", this);

        if (inventory == null || recipe == null) return;

        // Check materials
        foreach (var req in recipe.inputItems)
        {
            if (req == null || req.GetItem() == null) continue;

            if (inventory.GetItemCount(req.GetItem()) < req.GetQuantity())
            {
                
                return;
            }
        }

        // Remove materials
        foreach (var req in recipe.inputItems)
        {
            if (req == null || req.GetItem() == null) continue;

            bool ok = inventory.Remove(req.GetItem(), req.GetQuantity());
            if (!ok)
            {
                
                return;
            }
        }

        // Add output
        ItemClass outItem = recipe.outputItem.GetItem();
        int outQty = recipe.outputItem.GetQuantity();
        inventory.Add(outItem, outQty);

        
    }
}
