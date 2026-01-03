using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftedWorkbenches", menuName = "Inventory/CraftedWorkbenches")]
public class CraftedWorkbenches : ItemClass
{

    // Start is called before the first frame update
    public override ItemClass GetItem()
    {
        return this;
    }
}
