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

public class GhostPlatoonBehaviour : MonoBehaviour
{
    // Use this for initialization
    bool initIcon = false;
    bool raycastIgnore;
    bool raycastIgnoreChange = false;
    public float finalHeading;
    public GameObject icon;
    GameObject baseUnit;
    UnitType unitType;
    GameObject realPlatoon;
    private PlatoonBehaviour _platoonBehaviour;
    Player owner;
    List<GameObject> units = new List<GameObject>();


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (raycastIgnoreChange) {
            raycastIgnoreChange = false;
            _setIgnoreRaycast(raycastIgnore);
        }
    }

    public void InitializeIcon()
    {
        if (!initIcon) {

            initIcon = true;
            icon = GameObject.Instantiate(Resources.Load<GameObject>("Icon"));
            //Debug.Log(platoonBehaviour.gameObject);
            //icon.GetComponent<IconBehaviour>().setPlatoon(platoonBehaviour);
            icon.GetComponent<IconBehaviour>().setTeam(owner.getTeam());
            icon.transform.parent = transform;
        }
    }

    public PlatoonBehaviour GetRealPlatoon()
    {
        if (_platoonBehaviour == null)
            BuildRealPlatoon();

        return _platoonBehaviour;
    }

    public void BuildRealPlatoon()
    {
        realPlatoon = GameObject.Instantiate(Resources.Load<GameObject>("Platoon"));

        _platoonBehaviour = realPlatoon.GetComponent<PlatoonBehaviour>();
        _platoonBehaviour.Initialize(unitType, owner, units.Count);
        //platoonBehaviour.setEnabled(false);
        _platoonBehaviour.SetGhostPlatoon(this);
        realPlatoon.transform.position = transform.position + 100 * Vector3.down;
    }

    public void Initialize(UnitType t, Player owner, int n)
    {
        this.owner = owner;
        unitType = t;
        baseUnit = UnitFactory.GetUnit(unitType);
        transform.position = 100 * Vector3.down;

        // Create units:
        for (int i = 0; i < n; i++)
            AddSingleUnit();
    }

    private void AddSingleUnit()
    {
        GameObject go = GameObject.Instantiate(baseUnit);
        go.GetComponent<UnitBehaviour>().enabled = false;
        var shader = Resources.Load<Shader>("Ghost");
        go.ApplyShaderRecursively(shader);
        go.transform.position = 100 * Vector3.down;
        units.Add(go);
        //go.transform.parent = this.transform;
    }

    public void SetOrientation(Vector3 position, float heading)
    {

        finalHeading = heading;
        transform.position = position;
        Vector3 v = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        var left = new Vector3(-v.z, 0, v.x);

        var pos = position + (units.Count - 1) * (PlatoonBehaviour.BaseDistance / 2) * left;
        for (int i = 0; i < units.Count; i++) {

            var localPosition = pos - PlatoonBehaviour.BaseDistance * i * left;
            var localRotation = Quaternion.Euler(new Vector3(0, -Mathf.Rad2Deg * (heading) + 90, 0));
            units[i].GetComponent<UnitBehaviour>().SetOriginalOrientation(localPosition, localRotation, false);
            units[i].GetComponent<UnitBehaviour>().UpdateMapOrientation();
        }
    }

    public void SetVisible(bool vis)
    {
        InitializeIcon();
        icon.GetComponent<IconBehaviour>().setVisible(vis);
        units.ForEach(x => x.GetComponent<UnitBehaviour>().SetVisible(vis));
    }

    public void SetIgnoreRaycast(bool ignore)
    {
        raycastIgnore = ignore;
        raycastIgnoreChange = true;
    }

    public void _setIgnoreRaycast(bool ignore)
    {
        var layer = 0;
        if (ignore)
            layer = 2;

        Debug.Log(layer);
        gameObject.layer = layer;
        icon.layer = layer;
        foreach (var u in units)
            u.layer = layer;
    }

    public void Destroy()
    {
        _platoonBehaviour.Destroy();

        foreach (var u in units)
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
        behaviour.icon.GetComponent<IconBehaviour>().setGhost();

        return behaviour;
    }

    public void HandleRealUnitDestroyed()
    {
        GameObject u = units[0];
        units.Remove(u);
        Destroy(u);
        if (units.Count == 0)
            Destroy();
    }
}
