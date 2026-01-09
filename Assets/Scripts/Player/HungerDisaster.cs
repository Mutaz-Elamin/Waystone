using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class HungerDisaster : NaturalDisaster
{
    [SerializeField] private HeatstrokeEffect_BuiltIn heatstroke;
    [SerializeField] private float heatPerDay = 0.25f;

    protected override void day1Effect()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.setHungerSpeed(stats.HungerLoseSpeed * 1.2f);
        heatstroke.SetHeatstroke(0.25f);
    }
    protected override void day2Effect()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.setHungerSpeed(stats.HungerLoseSpeed * 1.3f);
        heatstroke.SetHeatstroke(0.5f);
    }
    protected override void day3Effect()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.setHungerSpeed(stats.HungerLoseSpeed * 1.3f);
        heatstroke.SetHeatstroke(0.75f);
    }
    protected override void day4Effect()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.setHungerSpeed(stats.HungerLoseSpeed * 2f);
        stats.setHealthLoseSpeed(2f);
        heatstroke.SetHeatstroke(1);
    }

}
