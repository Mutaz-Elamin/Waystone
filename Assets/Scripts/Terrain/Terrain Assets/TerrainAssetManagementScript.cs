using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainAssetManagement : MonoBehaviour
{
    protected TerrainAssetScript[] terrainAssets;
    protected int count = 0;
    protected int index = 0;
    protected int chunckSize = 0;

    // Method to add to the list of game objects to manage
    public void AddAsset(TerrainAssetScript asset)
    {
        if (index < count)
        {
            terrainAssets[index] = asset;
            index++;
        }
        else
        {
            Debug.Log("Asset array length not big enough.");
        }
    }

    // Method to create the array to hold the assets and set count
    public void SetCount (int count)
    {
        this.count = count;
        terrainAssets = new TerrainAssetScript[count];

        chunckSize = Mathf.CeilToInt(count / 7f);
    }

    // The method all assets scripts need to implement to define their specific behavior - called by a manager method
    public IEnumerator ParentCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < terrainAssets.Length; i++)
            {
                var asset = terrainAssets[i];
                asset.ScriptAction();
                if (count > chunckSize)
                {
                    if (i % chunckSize == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }
    }
}
