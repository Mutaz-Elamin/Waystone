using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


// Script for testing functions/features that currently lack in-game triggers
public class testScript : MonoBehaviour
{
    [SerializeField] private PassiveNPC passiveNPC;
    [SerializeField] private NeutralNPC neutralNPC;

    // Method for testing damage application for passive NPCs
    [ContextMenu("Test Passive TakeDamage")]
    private void TestTakeDamage()
    {
        passiveNPC.TakeDamage(1, DamageCause.PlayerAttack);
    }

    [ContextMenu("Test Neutral TakeDamage")]
    private void TestNeutralDamage()
    {
        neutralNPC.TakeDamage(1, DamageCause.PlayerAttack);
    }
}
