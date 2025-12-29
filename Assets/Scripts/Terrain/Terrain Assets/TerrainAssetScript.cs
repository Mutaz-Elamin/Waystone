using UnityEngine;

public abstract class TerrainAssetScript : MonoBehaviour
{
    [Header("Managed Ticking (Per-Chunk Runner)")]
    [SerializeField, Min(0f)] private float scriptActionInterval = 0f;

    private float nextTickTime;

    protected virtual void Awake() { }

    public abstract void ScriptAction();

    internal void ManagedResetTickTimer(float now)
    {
        nextTickTime = now;
    }

    internal void ManagedTick(float now)
    {
        if (scriptActionInterval <= 0f)
        {
            ScriptAction();
            return;
        }

        if (now >= nextTickTime)
        {
            ScriptAction();
            nextTickTime = now + scriptActionInterval;
        }
    }
}
