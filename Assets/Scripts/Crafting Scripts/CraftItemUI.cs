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
    private void Update()
    {
        CanCraft();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (recipe == null) return;
        // Update item icon and name
        ItemClass outItem = recipe.outputItem.GetItem();
        if (itemIcon != null && outItem != null && outItem.itemIcon != null)
        {
            itemIcon.sprite = outItem.itemIcon;
        }
        if (itemNameText != null && outItem != null)
        {
            itemNameText.text = outItem.itemName;
        }
        // Update requirements text
        if (requirementsText != null)
        {
            string reqText = "";
            foreach (var req in recipe.inputItems)
            {
                
                reqText += $"{req.GetItem().itemName}: {inventory.GetItemCount(req.GetItem())} / {req.GetQuantity()} \n";
            }
            requirementsText.text = reqText;
        }
        // Update craft button text
    


    }

    private void CanCraft()
    {

        if (inventory == null || recipe == null) return;
        if (inventory.IsFull())
        {
            craftButton.gameObject.SetActive(false);
            return;
        }

        // Check materials
        foreach (var req in recipe.inputItems)
        {
            if (req == null || req.GetItem() == null) continue;

            if (inventory.GetItemCount(req.GetItem()) < req.GetQuantity())
            {
                craftButton.gameObject.SetActive(false);
                return;
            }
            
        }
        craftButton.gameObject.SetActive(true);
    }

    

    private void TryCraft()
    {
        

        if (inventory == null || recipe == null) return;
        if (inventory.IsFull())
        {
          
            return;
        }

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
