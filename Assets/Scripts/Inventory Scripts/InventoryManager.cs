using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject slotHolder;
    [SerializeField] private GameObject hotbarslotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject selector;
    [SerializeField] private int selectedHotbarIndex = 0;
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private Button craftingUIButton;
    [SerializeField] private Button exitButton;
    public bool IsOpen = false;
    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot; 
    bool isMovingItem = false;

    [SerializeField] private SlotClass[] startingItems;
    private SlotClass[] items;
    private SlotClass[] hotbarItems;


    private GameObject[] slots ;
    private GameObject[] hotbarSlots ;
    private SlotClass sourceSlot;


    void Start()
    {
        



        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];
        hotbarSlots = new GameObject[hotbarslotHolder.transform.childCount];


        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i] = hotbarslotHolder.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new SlotClass();
        }

        for (int i = 0; i < startingItems.Length; i++)
        {
            items[i] = startingItems[i];
        }

        hotbarItems = new SlotClass[hotbarSlots.Length];
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            hotbarItems[i] = new SlotClass();
        }


        for (int i = 0; i < slotHolder.transform.childCount; i++)
        {
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }

        craftingUI.SetActive(false);
        craftingUIButton.onClick.RemoveAllListeners();
        craftingUIButton.onClick.AddListener(() =>
        {
            craftingUI.SetActive(!craftingUI.activeSelf);
            if (craftingUI.activeSelf && craftingMenu != null)
                craftingMenu.Open(CraftingMenu.StationType.Default);
        });

        inventoryUI.SetActive(false);
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() =>
        {
            ToggleInventory();
        });

        RefreshUI();
        


    }

    private void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                if (items[i].GetItem().isStackable)
                {
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = items[i].GetQuantity() + "";
                }
                else
                {
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                }
                    

            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        RefreshHotbarUI();

    }
    private struct SlotRef
    {
        public bool isHotbar;
        public int index;
        public SlotClass slot;
    }

    public void RefreshHotbarUI()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            try
            {
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = hotbarItems[i].GetItem().itemIcon;

                if (hotbarItems[i].GetItem().isStackable)
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = hotbarItems[i].GetQuantity().ToString();
                else
                    hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
            catch
            {
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                hotbarSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                hotbarSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }



    public bool Add(ItemClass item, int quantity)
    {
        //items.Add(item);
        //check if inventory contains item

        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable) {
            slot.AddQuantity(1);
                }
        else
        {
            for (int i = 0; i < hotbarItems.Length; i++)
            {
                if (hotbarItems[i].GetItem() == null)
                {
                    hotbarItems[i].AddItem(item, quantity);
                    RefreshUI();
                    return true;
                }
            }
            for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == null)
                    {
                        items[i].AddItem(item, quantity);
                    RefreshUI();
                    return true;
                }
                }
            }

        RefreshUI();
        return true;
    }

    /* Old Remove (without crafting system logic)
     * public bool Remove(ItemClass item)
    {
        SlotClass slot = Contains(item);
        if (slot != null)
        {
            if (slot.GetQuantity() > 1 && item.isStackable)
            {
                slot.RemoveQuantity(1);
            }
            else
            {
                int slotToRemove = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        slotToRemove = i;
                        break;
                    }
                }
                items[slotToRemove].Clear();
            }
           
        }
        else
        {
            return false;
        }


        RefreshUI();
        return true;
    } */

    public SlotClass Contains(ItemClass item)
    {
        foreach (SlotClass slot in hotbarItems)
        {
            if (slot.GetItem() == item)
            {
                return slot;
            }
        }
        foreach (SlotClass slot in items)
        {
            if (slot.GetItem() == item)
            {
                return slot;
            }
        }

        return null;
    }
    void Update()
    {
        itemCursor.SetActive(isMovingItem);

        if (Mouse.current != null)
            itemCursor.transform.position = Mouse.current.position.ReadValue();

        itemCursor.GetComponent<Image>().sprite =
            (isMovingItem && movingSlot != null && movingSlot.GetItem() != null)
                ? movingSlot.GetItem().itemIcon
                : null;
        selector.transform.position = hotbarSlots[selectedHotbarIndex].transform.position;
    }




    private SlotRef? GetClosestSlotRef()
    {
        if (Mouse.current == null) return null;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Check hotbar first
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (Vector2.Distance(mousePos, hotbarSlots[i].transform.position) <= 50f)
            {
                return new SlotRef { isHotbar = true, index = i, slot = hotbarItems[i] };
            }
        }

        // Then inventory grid
        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(mousePos, slots[i].transform.position) <= 32f)
            {
                return new SlotRef { isHotbar = false, index = i, slot = items[i] };
            }
        }

        return null;
    }



    private bool BeginItemMove()
    {
        var slotRef = GetClosestSlotRef();
        if (slotRef == null) return false;

        sourceSlot = slotRef.Value.slot;        
        if (sourceSlot.GetItem() == null) return false;

        movingSlot = new SlotClass(sourceSlot);   
        sourceSlot.Clear();                       
        isMovingItem = true;

        RefreshUI();
        return true;
    }


    private bool EndItemMove()
    {
        var slotRef = GetClosestSlotRef();

      
        if (slotRef == null)
        {
            sourceSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
            isMovingItem = false;
            RefreshUI();
            return true;
        }

        SlotClass targetSlot = slotRef.Value.slot;

      
        if (targetSlot == sourceSlot)
        {
            sourceSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
            isMovingItem = false;
            RefreshUI();
            return true;
        }

        // If target has item
        if (targetSlot.GetItem() != null)
        {
            // Stack if same + stackable
            if (targetSlot.GetItem() == movingSlot.GetItem() && targetSlot.GetItem().isStackable)
            {
                targetSlot.AddQuantity(movingSlot.GetQuantity());
                movingSlot.Clear();
            }
            else
            {
                //  REAL swap: target gets moving, source gets target's old
                tempSlot = new SlotClass(targetSlot);

                targetSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                sourceSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());

                movingSlot.Clear();
            }
        }
        else
        {
          
            targetSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
        }

        isMovingItem = false;
        RefreshUI();
        return true;
    }


    public void ToggleInventory()
    {
        IsOpen = !IsOpen;

        if (inventoryUI != null)
            inventoryUI.SetActive(IsOpen);

        // If closing inventory, always close crafting too
        if (!IsOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (craftingUI != null)
                craftingUI.SetActive(false);

            if (craftingMenu != null)
                craftingMenu.Close();
        }
    }
    public void PickOrSwapItem()
    {
        if (!IsOpen) return;

        if (!isMovingItem) BeginItemMove();
        else EndItemMove();
    }
    public void SelectHotbar(int index)
    {
        selectedHotbarIndex = index;
    }

    public int GetItemCount(ItemClass item)
    {
        int total = 0;

        for (int i = 0; i < hotbarItems.Length; i++)
            if (hotbarItems[i].GetItem() == item)
                total += hotbarItems[i].GetQuantity();

        for (int i = 0; i < items.Length; i++)
            if (items[i].GetItem() == item)
                total += items[i].GetQuantity();

        return total;
    }

    public bool Remove(ItemClass item, int quantity)
    {
        int remaining = quantity;

        
        for (int i = 0; i < hotbarItems.Length && remaining > 0; i++)
        {
            if (hotbarItems[i].GetItem() != item) continue;

            int inSlot = hotbarItems[i].GetQuantity();
            if (inSlot > remaining)
            {
                hotbarItems[i].RemoveQuantity(remaining);
                remaining = 0;
            }
            else
            {
                remaining -= inSlot;
                hotbarItems[i].Clear();
            }
        }

        // then inventory
        for (int i = 0; i < items.Length && remaining > 0; i++)
        {
            if (items[i].GetItem() != item) continue;

            int inSlot = items[i].GetQuantity();
            if (inSlot > remaining)
            {
                items[i].RemoveQuantity(remaining);
                remaining = 0;
            }
            else
            {
                remaining -= inSlot;
                items[i].Clear();
            }
        }

        RefreshUI();
        return remaining == 0;
    }

    public bool IsFull()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetItem() == null)
                return false;
        }
        return true;
    }
    [SerializeField] private CraftingMenu craftingMenu;   

    
    public void OpenInventoryAndCrafting(CraftingMenu.StationType stationType)
    {
        IsOpen = true;

        if (inventoryUI != null) inventoryUI.SetActive(true);
        if (craftingUI != null) craftingUI.SetActive(true);

        if (craftingMenu != null)
            craftingMenu.Open(stationType);
    }

 

    



}
