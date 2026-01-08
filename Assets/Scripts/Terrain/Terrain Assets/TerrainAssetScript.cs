using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainAssetScript : MonoBehaviour
{
    protected abstract void Awake();
    // The method all assets scripts need to implement to define their specific behavior - called by a manager method
    public abstract void ScriptAction();
}
