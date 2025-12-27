using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject slotHolder;
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


    private GameObject[] slots ;

    void Start()
    {
        



        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new SlotClass();
        }

        for (int i = 0; i < startingItems.Length; i++)
        {
            items[i] = startingItems[i];
        }


        for (int i = 0; i < slotHolder.transform.childCount; i++)
        {
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }



        inventoryUI.SetActive(false);
        Add(itemToAdd,1);
        Add(itemToAdd,1);
        Remove(itemToRemove);
        refreshUI();


    }

    private void refreshUI()
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

            refreshUI();
        return true;
    }

    public bool Remove(ItemClass item)
    {
        SlotClass slot = Contains(item);
        if (slot != null)
        {
            if (slot.GetQuantity() > 1 && item.isStackable)
            {
                slot.removeQuantity(1);
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


            refreshUI();
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
        itemCursor.transform.position = Input.mousePosition;
        itemCursor.GetComponent<Image>().sprite = isMovingItem ? movingSlot.GetItem().itemIcon : null;
        if (Input.GetMouseButtonDown(0))
        {
            if (isMovingItem)
            {
                EndItemMove();
            }
            else
            {
                BeginItemMove();
            }
        }
            
         
        
    }

    private SlotClass getClosestSlot()
    {
       
        for (int i = 0; i < items.Length; i++)
        {
           if (Vector2.Distance(Input.mousePosition, slots[i].transform.position) <= 32f)
            {
                return items[i];
            }
        }
        return null;
    }

    private bool BeginItemMove()
    {
        originalSlot = getClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null)
        {
            return false;
        }
        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        refreshUI();
        return true;
    }

    private bool EndItemMove()
    {
        originalSlot = getClosestSlot();
        if (originalSlot == null)
        {
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();

        }
        else
        {
            if (originalSlot.GetItem() != null)
            {
                if (originalSlot.GetItem() == movingSlot.GetItem())
                {
                    if (originalSlot.GetItem().isStackable)
                    {
                        originalSlot.AddQuantity(movingSlot.GetQuantity());
                        movingSlot.Clear();
                    }
                    else
                    {
                        tempSlot = new SlotClass(originalSlot);
                        originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                        movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());
                        refreshUI();
                        return true;
                    }
                   
                }
                else
                {
                    tempSlot = new SlotClass(originalSlot);
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());
                    refreshUI();
                    return true;
                }
            }
            else
            {
                originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }
        isMovingItem = false;
        refreshUI();
        return true;
    }
    public void ToggleInventory()
    {
        IsOpen = !IsOpen;
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }
}
