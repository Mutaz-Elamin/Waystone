using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Material", menuName = "Inventory/Materials")]
public class Material : ItemClass
{

    // Start is called before the first frame update
    public override ItemClass GetItem()
    {
        return this;
    }
}
