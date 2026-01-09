using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupResource : MonoBehaviour
{
    public void Pickup()
    {
        HealthBasedAsset healthBasedAsset = GetComponent<HealthBasedAsset>();
        if (healthBasedAsset != null)
        {
            healthBasedAsset.TakeDamage(100, DamageCause.Other);
        }
    }
}
