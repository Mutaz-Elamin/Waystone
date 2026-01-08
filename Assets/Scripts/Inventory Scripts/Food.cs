using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Inventory/Food")]
public class Food : ItemClass
{

    
    public override ItemClass GetItem()
    {
        return this;
    }
    public override void UseItem(GameObject user)
    {
        InventoryManager inv = user.GetComponent<InventoryManager>();
        if (inv == null) return;

        // TODO: eat logic here (hunger restore, animation, etc.)

        inv.Remove(this, 1);
    }
    public override void Equip(GameObject user)
    {
        return;
    }

}
