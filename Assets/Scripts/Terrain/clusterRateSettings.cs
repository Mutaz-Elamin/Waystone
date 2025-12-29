using System;
using UnityEngine;

[Serializable]
public struct ClusterRateSettings
{
    public enum TimeFilter
    {
        DayOnly,
        NightOnly,
        Both
    }

    [SerializeField] private TimeFilter timeFilter;

    [SerializeField, Range(0f, 1f)] private float chanceDay;
    [SerializeField, Range(0f, 1f)] private float chanceNight;

    [SerializeField, Min(0f)] private float minPlayerDistance;
    [SerializeField, Min(0f)] private float maxPlayerDistance;

    public TimeFilter Filter => timeFilter;
    public float ChanceDay => chanceDay;
    public float ChanceNight => chanceNight;
    public float MinPlayerDistance => minPlayerDistance;
    public float MaxPlayerDistance => maxPlayerDistance;

    public float GetChance(bool isNight) => isNight ? chanceNight : chanceDay;

    public bool CanAttempt(bool isNight, float playerDistance)
    {
        if (timeFilter == TimeFilter.DayOnly && isNight) return false;
        if (timeFilter == TimeFilter.NightOnly && !isNight) return false;

        if (playerDistance < minPlayerDistance) return false;
        if (maxPlayerDistance > 0f && playerDistance > maxPlayerDistance) return false;

        return true;
    }
}
