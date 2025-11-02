using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Script for testing functions/features that currently lack in-game triggers
public class testScript : MonoBehaviour
{
    [SerializeField] private PassiveNPC passiveNPC;

    // Method for testing damage application for passive NPCs
    [ContextMenu("Test Passive TakeDamage")]
    private void TestTakeDamage()
    {
        passiveNPC.TakeDamage(5);
    }
}
