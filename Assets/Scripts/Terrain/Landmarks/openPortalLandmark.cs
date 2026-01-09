using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPortalLandmark : MonoBehaviour
{
    [SerializeField] private GameObject portalParticles;
    [SerializeField] private bool winPortal = false;

    private bool portalOpened = false;

    public bool IsPortalOpened()
    {
        return portalOpened;
    }

    public void OpenPortal()
    {
        if (portalParticles != null)
        {
            Vector3 particlesPosition = new Vector3(transform.position.x, transform.position.y + 3.9f, transform.position.z);

            Instantiate(portalParticles, particlesPosition, portalParticles.transform.rotation);
            portalOpened = true;
        }
    }

    public bool IsWinPortal()
    {
        return winPortal;
    }
}
