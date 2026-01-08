using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public bool isStackable = true;

    private WeaponsManager weaponsManager;

    // Start is called before the first frame update
    public abstract ItemClass GetItem();
    public virtual void UseItem(GameObject user) { }

    public virtual void Equip(GameObject user) {
        weaponsManager = user.GetComponent<WeaponsManager>();
        weaponsManager.SpawnTestWeapon(null);
    } 
}
