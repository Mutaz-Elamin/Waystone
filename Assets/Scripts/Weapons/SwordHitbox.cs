using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        // Disable the object it hits
        other.gameObject.SetActive(false);

        // Disable hitting until next attack
        canHit = false;
    }
}
