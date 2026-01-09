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

        PlayerStats stats = user.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.Eat(20f);

        inv.Remove(this, 1);
    }
    public override void Equip(GameObject user)
    {
        return;
    }

}
