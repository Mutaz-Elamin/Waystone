using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPortalLandmark : MonoBehaviour
{
    [SerializeField] private GameObject portalParticles;

    public void OpenPortal()
    {
        if (portalParticles != null)
        {
            Vector3 particlesPosition = new Vector3(transform.position.x, transform.position.y + 3.9f, transform.position.z);

            Instantiate(portalParticles, particlesPosition, portalParticles.transform.rotation);
        }
    }
}
