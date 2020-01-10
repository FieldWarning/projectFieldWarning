using PFW.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointOverlayBehavior : MonoBehaviour
{

    private PlatoonBehaviour _platoon;
    private LineRenderer _lineR;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_platoon != null)
        {
            if (_platoon.ActiveWaypoint == null)
            {
                // TODO: should prob just set inactive...
                _lineR.gameObject.SetActive(false);
                return;
            }

            _lineR.gameObject.SetActive(true);
            _lineR.SetPosition(0, _platoon.transform.position);
            _lineR.SetPosition(1, _platoon.ActiveWaypoint.Destination);
        }
    }

    public void Initialize(PlatoonBehaviour platoon)
    {
        _platoon = platoon;
        _lineR = transform.Find("Line").GetComponent<LineRenderer>();
        _lineR.startColor = Color.green;
        _lineR.endColor = Color.green;
        _lineR.useWorldSpace = true;
        _lineR.sortingLayerName = "OnTop";
        _lineR.sortingOrder = 20;
        _lineR.positionCount = 2;
        _lineR.startWidth = 0.005f;
        _lineR.endWidth = 0.10f;
    }
}
