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

        // Only disable objects that are NOT player AND NOT camera
        if (!other.CompareTag("Player") && !other.CompareTag("MainCamera"))
        {
            Debug.Log("Hit: " + other.name);
            other.gameObject.SetActive(false);
        }

        canHit = false;
    }
}
