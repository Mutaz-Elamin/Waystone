using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    
    [SerializeField] private GameObject slotHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;
    [SerializeField] private GameObject inventoryUI;


    private List<ItemClass> items = new List<ItemClass>();


    private GameObject[] slots;

    void Start()
    {
        inventoryUI = GameObject.Find("Inventory");
    
        slotHolder = GameObject.Find("Slots");



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
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].itemIcon;
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
            }
        }

    }

    public void Add(ItemClass item)
    {
        items.Add(item);
        Debug.Log("Added " + item.itemName + " to inventory.");
        refreshUI();
    }

    public void Remove(ItemClass item)
    {
        items.Remove(item);
        Debug.Log("Removed " + item.itemName + " from inventory.");
    }
    void Update()
    {

    }
    public void ToggleInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }
}
