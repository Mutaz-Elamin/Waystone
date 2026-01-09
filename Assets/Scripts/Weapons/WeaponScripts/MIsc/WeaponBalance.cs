using System.Collections.Generic;
using UnityEngine;


public enum MaterialType
{
    Stone,
    Bronze,
    Steel,
    Gold,
    Obsidian,
    Blood,
    Slime,
    Weird,
    Ice,
    Shadow,
    Bone,
    Wood
}

public static class WeaponBalance
{
    private static readonly Dictionary<MaterialType, float> multipliers = new Dictionary<MaterialType, float>()
    {
        { MaterialType.Stone,   1.0f },
        { MaterialType.Bronze,  1.2f },
        { MaterialType.Steel,   1.5f },
        { MaterialType.Gold,    1.3f },
        { MaterialType.Obsidian,1.8f },

        //Uniques/Oneoff Miscs
        { MaterialType.Blood,    1.4f },
        { MaterialType.Slime,    1.25f },
        { MaterialType.Shadow,    1.7f },
        { MaterialType.Ice,    2f },
        { MaterialType.Weird,    4f },
        { MaterialType.Bone,    1.75f },
        { MaterialType.Wood,    1.1f}

    };

    public static float GetMultiplier(MaterialType mat)
    {
        if (multipliers.TryGetValue(mat, out var m)) return m;
        return 1f;
    }
}

