using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Weapons")]
public class Weapon : ItemClass
{

    // Start is called before the first frame update
    public override ItemClass GetItem() {
        return this;
            }
}
