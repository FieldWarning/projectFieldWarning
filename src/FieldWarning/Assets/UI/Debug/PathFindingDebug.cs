using PFW;
using PFW.Model.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PFW.Units.Component.Movement;

public class PathFindingDebug : MonoBehaviour
{
    public GameObject wpPrefab;
    private MatchSession _matchSession = null;

    List<GameObject> wps = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        var gameSession = GameObject.FindWithTag("GameSession");
        _matchSession = gameSession.GetComponent<MatchSession>();

        
    }

    

    // Update is called once per frame
    void Update()
    {
      
        var wpToggle = transform.Find("WPToggle").GetComponent<Toggle>();

        if (wpToggle.isOn && wps.Count == 0)
        {
            List<PathNode> wpGraph = _matchSession.PathData.GetWaypointGraph();
            foreach (var pn in wpGraph)
            {
                wps.Add(Instantiate(wpPrefab, new Vector3(pn.x, pn.y, pn.z), Quaternion.identity));
            }
        }

        if (!wpToggle.isOn && wps.Count > 0)
        {
            foreach (var wp in wps)
            {
                Destroy(wp);
            }

            wps.Clear();
        }







    }

    public void ReloadTerrainData()
    {
        _matchSession.TerrainMap.ReloadTerrainData();
    }
}
