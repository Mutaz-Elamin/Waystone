using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneralResourceAsset : HealthBasedAsset
{
    [SerializeField] private DamageCause DamageWeakness;
    [SerializeField] private DamageCause DamageResistance;
    [SerializeField] private GameObject[] resourcesDroppedOnDeath;

    protected virtual void OnEnable()
    {
        health = StartHealth;
    }


    public override void TakeDamage(int damage, DamageCause cause)
    {
        if (DamageWeakness != DamageCause.None && cause == DamageWeakness)
        {
            damage *= 2;
        }
        else if (DamageResistance != DamageCause.None && cause == DamageResistance)
        {
            damage /= 2;
        }

        base.TakeDamage(damage, cause);
    }

    protected override void Die()
    {
        ClusterMember clusterMember = GetComponent<ClusterMember>();
        if (clusterMember != null)
        {
            DropResources();
            clusterMember.Despawn();
        }
        else
        {
            base.Die();
        }
    }
}
