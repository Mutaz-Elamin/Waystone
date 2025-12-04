using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    
    [SerializeField] private GameObject slotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;
    [SerializeField] private GameObject inventoryUI;
    public bool IsOpen = false;

    public List<SlotClass> items = new List<SlotClass>();
    

    private GameObject[] slots;

    void Start()
    {
        



        slots = new GameObject[slotHolder.transform.childCount];
        for (int i = 0; i < slotHolder.transform.childCount; i++)
        {
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }



        inventoryUI.SetActive(false);

        
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

    public bool Add(ItemClass item)
    {
        //items.Add(item);
        //check if inventory contains item

        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().isStackable) {
            slot.AddQuantity(1);
                }
        else
        {
            if (items.Count < slots.Length)
            {
                items.Add(new SlotClass(item, 1));
            }
            else
            {
                              
                return false;
            }
            
        }

            refreshUI();
        return true;
    }

    public void Remove(ItemClass item)
    {
        //items.Remove(item);
        SlotClass slotToRemove;
        foreach(SlotClass slot in items)
        {
            if (slot.GetItem() == item)
            {
                slot.removeQuantity(1);
                if (slot.GetQuantity() <= 0)
                {
                    slotToRemove = slot;
                    items.Remove(slotToRemove);
                }
                break;
            }
        }

        refreshUI();
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

    }
    public void ToggleInventory()
    {
        IsOpen = !IsOpen;
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }
}
