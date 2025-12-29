using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TerrainAssetManagement : MonoBehaviour
{
    [Header("Per-Chunk Tick Budget")]
    [SerializeField, Min(0)] private int maxAssetsProcessedPerFrame = 256;

    [SerializeField, Min(0)] private int nullSweepIntervalFrames = 60;

    private readonly List<TerrainAssetScript> assets = new(256);
    private int cursor;
    private int frameCounter;
    private Coroutine routine;

    public void ConfigureBudget(int maxPerFrame)
    {
        maxAssetsProcessedPerFrame = Mathf.Max(0, maxPerFrame);
    }

    public void RegisterAsset(TerrainAssetScript asset)
    {
        if (asset == null) return;
        if (assets.Contains(asset)) return;

        assets.Add(asset);
        asset.ManagedResetTickTimer(Time.time);
    }

    public void UnregisterAsset(TerrainAssetScript asset)
    {
        if (asset == null) return;
        assets.Remove(asset);

        if (cursor >= assets.Count)
            cursor = 0;
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;
        if (routine == null)
            routine = StartCoroutine(ChunkRoutine());
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator ChunkRoutine()
    {
        while (true)
        {
            TickOnce();
            yield return null;
        }
    }

    private void TickOnce()
    {
        float now = Time.time;

        if (nullSweepIntervalFrames > 0)
        {
            frameCounter++;
            if (frameCounter >= nullSweepIntervalFrames)
            {
                frameCounter = 0;
                SweepNulls();
            }
        }

        int count = assets.Count;
        if (count == 0) return;

        int budget = maxAssetsProcessedPerFrame <= 0 ? count : Mathf.Min(maxAssetsProcessedPerFrame, count);

        for (int i = 0; i < budget; i++)
        {
            if (assets.Count == 0) break;
            if (cursor >= assets.Count) cursor = 0;

            TerrainAssetScript a = assets[cursor];

            if (a == null || !a.isActiveAndEnabled)
            {
                assets.RemoveAt(cursor);
                continue;
            }

            a.ManagedTick(now);
            cursor++;
        }
    }

    private void SweepNulls()
    {
        for (int i = assets.Count - 1; i >= 0; i--)
            if (assets[i] == null) assets.RemoveAt(i);

        if (cursor >= assets.Count)
            cursor = 0;
    }
}
