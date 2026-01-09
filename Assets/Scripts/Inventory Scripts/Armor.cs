using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Inventory/Armor")]
public class Armor : ItemClass
{
    public int armorRating = 0;

    public int healthBonus = 0;
    public int staminaBonus = 0;


    public override ItemClass GetItem()
    {
        return this;
    }


}
