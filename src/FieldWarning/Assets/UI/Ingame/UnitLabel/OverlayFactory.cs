using PFW.Units;
using System.Collections.Generic;
using UnityEngine;

public class OverlayFactory : MonoBehaviour
{
    public static OverlayFactory instance; // Needed

    void Start()
    {
        instance = this;
    }

    public WaypointOverlayBehavior CreateWaypointOverlay(PlatoonBehaviour pb)
    {
        
        var overlayPrefab = Resources.Load<GameObject>("WaypointOverlay");
        var waypointOverlayBehavior = Object.Instantiate(overlayPrefab, Vector3.zero, Quaternion.identity).GetComponent<WaypointOverlayBehavior>();
        waypointOverlayBehavior.Initialize(pb);

       

        return waypointOverlayBehavior;

    }
}
