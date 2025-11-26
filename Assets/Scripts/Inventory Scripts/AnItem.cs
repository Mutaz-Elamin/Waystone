using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/newItem")]
public class AnItem : ItemClass
{

    // Start is called before the first frame update
    public override ItemClass GetItem() {
        return this;
            }
}
