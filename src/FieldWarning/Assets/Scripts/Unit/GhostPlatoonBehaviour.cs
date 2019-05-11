﻿/**
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
using PFW.UI.Prototype;
using PFW.UI.Ingame;

using PFW.Model.Game;
using PFW.Units.Component.Movement;

public class GhostPlatoonBehaviour : MonoBehaviour
{
    public float FinalHeading;

    private bool _raycastIgnore;
    private bool _raycastIgnoreChange = false;
    private GameObject _icon;
    private UnitType _unitType;
    private GameObject _realPlatoon;
    private PlatoonBehaviour _platoonBehaviour;
    private PlayerData _owner;
    private List<GameObject> _units = new List<GameObject>();

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
        _icon.GetComponent<IconBehaviour>().BaseColor = _owner.Team.Color;
        _icon.transform.parent = transform;
    }

    public PlatoonBehaviour GetRealPlatoon()
    {
        return _platoonBehaviour;
    }

    public void BuildRealPlatoon()
    {
        _realPlatoon = GameObject.Instantiate(Resources.Load<GameObject>("Platoon"));

        _platoonBehaviour = _realPlatoon.GetComponent<PlatoonBehaviour>();
        _platoonBehaviour.Initialize(_unitType, _owner, _units.Count);

        _platoonBehaviour.SetGhostPlatoon(this);
    }

    public void Initialize(UnitType t, PlayerData owner, int unitCount)
    {
        _owner = owner;
        _unitType = t;
        transform.position = 100 * Vector3.down;

        // Create units:
        for (int i = 0; i < unitCount; i++)
            AddSingleUnit();
    }

    public void InitializeAfterSplit(
        UnitType t, PlayerData owner)
    {
        _owner = owner;
        _unitType = t;
        transform.position = 100 * Vector3.down;

        InitializeIcon();

        AddSingleUnit();
    }

    private void AddSingleUnit()
    {
        GameObject _unitPrefab = _owner.Session.Factory.FindPrefab(_unitType);
        GameObject unit = _owner.Session.Factory.MakeGhostUnit(_unitPrefab);
        _units.Add(unit);
    }

    public void SetOrientation(Vector3 center, float heading)
    {
        FinalHeading = heading;
        transform.position = center;

        var positions = Formations.GetLineFormation(center, heading, _units.Count);
        for (int i = 0; i < _units.Count; i++) {
            _units[i].GetComponent<MovementComponent>().SetOriginalOrientation(positions[i], Mathf.PI / 2 - heading, false);
        }
    }

    public void SetVisible(bool vis)
    {
        _icon.GetComponent<IconBehaviour>().SetVisible(vis);
        _units.ForEach(x => x.GetComponent<MovementComponent>().SetVisible(vis));

        _units.ForEach(x => x.GetComponent<UnitLabelAttacher>().SetVisibility(vis));
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
        foreach (var u in _units)
            Destroy(u);

        Destroy(gameObject);
    }

    public static GhostPlatoonBehaviour Build(UnitType t, PlayerData owner, int count)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("GhostPlatoon"));
        var behaviour = go.GetComponent<GhostPlatoonBehaviour>();
        behaviour.Initialize(t, owner, count);
        behaviour.InitializeIcon();

        go.ApplyShaderRecursively(Shader.Find("Custom/Ghost"));
        behaviour._icon.GetComponent<IconBehaviour>().SetGhost();

        return behaviour;
    }

    public void Spawn(Vector3 pos)
    {
        BuildRealPlatoon();
        _platoonBehaviour.Spawn(pos);
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
