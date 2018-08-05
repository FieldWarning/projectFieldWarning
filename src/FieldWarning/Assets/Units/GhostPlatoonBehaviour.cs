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
using Pfw.Ingame.Prototype;
using Assets.Ingame.UI;

public class GhostPlatoonBehaviour : MonoBehaviour
{
    public float FinalHeading;
    
    private bool _raycastIgnore;
    private bool _raycastIgnoreChange = false;
    private GameObject _icon;
    private GameObject _baseUnit;
    private UnitType _unitType;
    private GameObject _realPlatoon;
    private PlatoonBehaviour _platoonBehaviour;
    private Player _owner;
    private List<GameObject> _units = new List<GameObject>();
    
    void Start()
    {

    }
    
    void Update()
    {
        if (_raycastIgnoreChange) {
            _raycastIgnoreChange = false;
            _setIgnoreRaycast(_raycastIgnore);
        }
    }

    private void InitializeIcon()
    {
        _icon = GameObject.Instantiate(Resources.Load<GameObject>("Icon"));
        //Debug.Log(platoonBehaviour.gameObject);
        //icon.GetComponent<IconBehaviour>().setPlatoon(platoonBehaviour);
        _icon.GetComponent<IconBehaviour>().SetTeam(_owner.getTeam());
        _icon.transform.parent = transform;        
    }

    public PlatoonBehaviour GetRealPlatoon()
    {
        if (_platoonBehaviour == null)
            BuildRealPlatoon();

        return _platoonBehaviour;
    }

    public void BuildRealPlatoon()
    {
        _realPlatoon = GameObject.Instantiate(Resources.Load<GameObject>("Platoon"));

        _platoonBehaviour = _realPlatoon.GetComponent<PlatoonBehaviour>();
        _platoonBehaviour.Initialize(_unitType, _owner, _units.Count);

        _platoonBehaviour.SetGhostPlatoon(this);
        _realPlatoon.transform.position = transform.position + 100 * Vector3.down;
    }

    public void Initialize(UnitType t, Player owner, int unitCount)
    {
        _owner = owner;
        _unitType = t;
        _baseUnit = UnitFactory.GetUnit(_unitType);
        transform.position = 100 * Vector3.down;

        // Create units:
        for (int i = 0; i < unitCount; i++)
            AddSingleUnit();
    }

    private void AddSingleUnit()
    {
        GameObject go = GameObject.Instantiate(_baseUnit);
        go.GetComponent<UnitBehaviour>().enabled = false;
        var shader = Resources.Load<Shader>("Ghost");
        go.ApplyShaderRecursively(shader);
        go.transform.position = 100 * Vector3.down;
        _units.Add(go);
        //go.transform.parent = this.transform;
    }

    public void SetOrientation(Vector3 position, float heading)
    {
        FinalHeading = heading;
        transform.position = position;
        Vector3 v = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        var left = new Vector3(-v.z, 0, v.x);

        var pos = position + (_units.Count - 1) * (PlatoonBehaviour.BaseDistance / 2) * left;
        for (int i = 0; i < _units.Count; i++) {

            var localPosition = pos - PlatoonBehaviour.BaseDistance * i * left;
            var localRotation = Quaternion.Euler(new Vector3(0, -Mathf.Rad2Deg * (heading) + 90, 0));
            _units[i].GetComponent<UnitBehaviour>().SetOriginalOrientation(localPosition, localRotation, false);
            _units[i].GetComponent<UnitBehaviour>().UpdateMapOrientation();
        }
    }

    public void SetVisible(bool vis)
    {
        _icon.GetComponent<IconBehaviour>().SetVisible(vis);
        _units.ForEach(x => x.GetComponent<UnitBehaviour>().SetVisible(vis));
    }

    public void SetIgnoreRaycast(bool ignore)
    {
        _raycastIgnore = ignore;
        _raycastIgnoreChange = true;
    }

    public void _setIgnoreRaycast(bool ignore)
    {
        var layer = 0;
        if (ignore)
            layer = 2;

        Debug.Log(layer);
        gameObject.layer = layer;
        _icon.layer = layer;
        foreach (var u in _units)
            u.layer = layer;
    }

    public void Destroy()
    {
        _platoonBehaviour.Destroy();

        foreach (var u in _units)
            Destroy(u);

        Destroy(gameObject);
    }

    public static GhostPlatoonBehaviour Build(UnitType t, Player owner, int count)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("GhostPlatoon"));
        var behaviour = go.GetComponent<GhostPlatoonBehaviour>();
        behaviour.Initialize(t, owner, count);
        behaviour.BuildRealPlatoon();
        behaviour.InitializeIcon();

        go.ApplyShaderRecursively(Shader.Find("Custom/Ghost"));
        behaviour._icon.GetComponent<IconBehaviour>().SetGhost();

        return behaviour;
    }

    public void HandleRealUnitDestroyed()
    {
        GameObject u = _units[0];
        _units.Remove(u);
        Destroy(u);
        if (_units.Count == 0)
            Destroy();
    }
}
