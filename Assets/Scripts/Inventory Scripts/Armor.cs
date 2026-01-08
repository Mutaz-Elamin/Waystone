using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Inventory/Armor")]
public class Armor : ItemClass
{

    
    public override ItemClass GetItem()
    {
        return this;
    }
  

}
