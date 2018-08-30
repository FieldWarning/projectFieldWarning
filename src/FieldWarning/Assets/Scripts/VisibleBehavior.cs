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

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using PFW.Model.Game;

public class VisibleBehavior : MonoBehaviour
{
    [SerializeField]
    private float max_spot_range = 400f;
    [SerializeField]
    private float stealth_pen_factor = 1f;
    [SerializeField]
    private float stealth_factor = 10f;


    private Point _currentRegion;
    private Dictionary<VisibleBehavior, bool> _spottedBy = new Dictionary<VisibleBehavior, bool>();
    private int _spottedByCount;
    private Dictionary<VisibleBehavior, bool> _spotting = new Dictionary<VisibleBehavior, bool>();
    private bool _hostile;

    // Update is called once per frame
    void Update()
    {
        var newRegion = VisibilityManager.GetRegion(transform);
        if (_currentRegion != newRegion) {
            //Debug.Log("region updated");
            VisibilityManager.UpdateUnitRegion(this, newRegion);
            _currentRegion = newRegion;
        }
    }

    public void Initialize(Team t)
    {
        //Team = t;
        //var o = Team.Blue;
        //if (t == Team.Blue) o = Team.Red;
        //SetHostileTeam(VisibilityManager.getTeamMembers(o));
        //VisibilityManager.addVisibleBehaviour(this);
        //if (Team == Team.Red) {
        //    GetComponent<Renderer>().material.color = Color.red;
        //} else {
        //    GetComponent<Renderer>().material.color = Color.blue;
        //}
        //UpdateTeamBelonging();
        //_currentRegion = VisibilityManager.getRegion(transform);
        //VisibilityManager.updateUnitRegion(this, _currentRegion);
    }

    public void SetDetected(bool detected)
    {
        if (!_hostile) return;
        if (detected) {
            GetComponent<Renderer>().enabled = true;
            //GetComponent<Renderer>().material.color = Color.red;
        } else {
            GetComponent<Renderer>().enabled = false;
            //GetComponent<Renderer>().material.color = Color.black;
        }
    }

    public Point GetRegion()
    {
        return _currentRegion;
    }

    public void SetSpotting(VisibleBehavior enemy, bool p)
    {
        if (p && !_spotting[enemy]) {
            _spotting[enemy] = true;
        } else {
            _spotting[enemy] = false;
        }
    }

    public void SetSpottedBy(VisibleBehavior unit, bool p)
    {
        if (p && !_spottedBy[unit]) {
            if (_spottedByCount == 0) {
                SetDetected(true);
            }
            _spottedByCount += 1;
            _spottedBy[unit] = true; ;

        }
        if (!p && _spottedBy[unit]) {
            _spottedByCount -= 1;
            if (_spottedByCount == 0) {
                SetDetected(false);
            }
            _spottedBy[unit] = false;
        }
    }

    public void SetHostileTeam(List<VisibleBehavior> hostileTeam)
    {
        foreach (var h in hostileTeam.Where(x => !_spotting.Keys.Contains(x))) {
            _spotting.Add(h, false);
            _spottedBy.Add(h, false);
        }
    }

    public void AddHostile(VisibleBehavior b)
    {
        if (!_spottedBy.ContainsKey(b)) 
            _spottedBy.Add(b, false);
        
        if (!_spotting.ContainsKey(b)) 
            _spotting.Add(b, false);        
    }
}
