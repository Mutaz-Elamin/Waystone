using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{

    public SlotClass[] inputItems;
    public SlotClass outputItem;
}
