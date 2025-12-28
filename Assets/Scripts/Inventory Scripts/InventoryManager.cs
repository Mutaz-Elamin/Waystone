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



        inventoryUI.SetActive(false);
        Add(itemToAdd,1);
        Add(itemToAdd,1);
        Remove(itemToRemove);
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
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == null)
                    {
                        items[i].AddItem(item, quantity);
                        break;
                    }
                }
            }

        RefreshUI();
        return true;
    }

    public bool Remove(ItemClass item)
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
    }

    public SlotClass Contains(ItemClass item)
    {
        foreach(SlotClass slot in items)
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
    }




    private SlotRef? GetClosestSlotRef()
    {
        if (Mouse.current == null) return null;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Check hotbar first
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (Vector2.Distance(mousePos, hotbarSlots[i].transform.position) <= 32f)
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

        sourceSlot = slotRef.Value.slot;          //  remember source
        if (sourceSlot.GetItem() == null) return false;

        movingSlot = new SlotClass(sourceSlot);   // cursor copy
        sourceSlot.Clear();                       // remove from source
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
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }
    public void PickOrSwapItem()
    {
        if (!IsOpen) return;

        if (!isMovingItem) BeginItemMove();
        else EndItemMove();
    }

}
