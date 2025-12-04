using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public bool isStackable = true;

    // Start is called before the first frame update
    public abstract ItemClass GetItem();
}
