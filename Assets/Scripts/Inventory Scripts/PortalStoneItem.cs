using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PortalStoneItem", menuName = "Inventory/PortalStoneItem")]
public class PortalStoneItem : ItemClass
{


    public override ItemClass GetItem()
    {
        return this;
    }


}
