/**
* Copyright (c) 2017-present, PFW Contributors.
*
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
* compliance with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software distributed under the License is
* distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
* the License for the specific language governing permissions and limitations under the License.
*/

using PFW.Model.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PFW.Units.Component.Movement;

public class PathFindingDebug : MonoBehaviour
{
    public GameObject wpPrefab;
    public GameObject wpEdgePrefab;
    private MatchSession _matchSession = null;

    List<GameObject> wps = new List<GameObject>();
    List<GameObject> edges = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        _matchSession = MatchSession.Current;
    }

    private void ToggleWPs(bool toggle)
    {
        if (toggle && wps.Count == 0)
        {
            List<PathNode> wpGraph = _matchSession.PathData.GetWaypointGraph();
            foreach (PathNode pn in wpGraph)
            {
                wps.Add(Instantiate(wpPrefab, new Vector3(pn.x, pn.y, pn.z), Quaternion.identity));
            }
        }

        if (!toggle && wps.Count > 0)
        {
            foreach (var wp in wps)
            {
                Destroy(wp);
            }

            wps.Clear();
        }
    }

    private void ToggleEdges(bool toggle)
    {
        if (toggle && edges.Count == 0)
        {
            List<PathNode> wpGraph = _matchSession.PathData.GetWaypointGraph();
            foreach (PathNode pn in wpGraph)
            {
                foreach (PathArc arc in pn.Arcs)
                {
                    GameObject line = Instantiate(
                            wpEdgePrefab, 
                            new Vector3(arc.Node1.x, arc.Node1.y, arc.Node1.z), 
                            Quaternion.identity);
                    LineRenderer _lineR = line.transform.Find("Line").GetComponent<LineRenderer>();
                    _lineR.startColor = Color.white;
                    _lineR.endColor = Color.white;
                    _lineR.useWorldSpace = true;
                    _lineR.sortingLayerName = "OnTop";
                    _lineR.sortingOrder = 20;

                    _lineR.startWidth = 0.20f;
                    _lineR.endWidth = 0.20f;

                    _lineR.positionCount = 2;

                    _lineR.SetPosition(0, new Vector3(arc.Node1.x, arc.Node1.y, arc.Node1.z));
                    //_lineR.SetPosition(1, new Vector3(arc.Node1.x, arc.Node1.y, arc.Node1.z));
                    _lineR.SetPosition(1, new Vector3(arc.Node2.x, arc.Node2.y, arc.Node2.z));
                    line.SetActive(true);
                    edges.Add(line);
                }
                
            }
        }

        if (!toggle && edges.Count > 0)
        {
            foreach (GameObject line in edges)
            {
                Destroy(line);
            }

            edges.Clear();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        Toggle wpToggle = transform.Find("WPToggle").GetComponent<Toggle>();
        Toggle edgeToggle = transform.Find("EdgeToggle").GetComponent<Toggle>();

        ToggleWPs(wpToggle.isOn);

        ToggleEdges(edgeToggle.isOn);
    }

    public void ReloadTerrainData()
    {
        _matchSession.TerrainMap.ReloadTerrainData();
    }
}
