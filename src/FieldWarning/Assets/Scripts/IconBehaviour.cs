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

public class IconBehaviour : SelectableBehavior
{
    int layer = -1;
    public PlatoonBehaviour platoon;

    SymbolBehaviour _symbol;
    SymbolBehaviour symbol {
        get {
            if (_symbol == null)
                _symbol = transform.GetChild(1).GetComponent<SymbolBehaviour>();

            return _symbol;
        }
    }

    Transform _billboard;
    Transform billboard {
        get {
            if (_billboard == null) {
                _billboard = transform.GetChild(0);
            }

            return _billboard;
        }
    }

    public bool isInitialized = false;
    private bool init = false;
    Color baseColor = Color.blue;
    bool visible = true;


    // Use this for initialization
    void Start()
    {
        billboard.GetComponent<Renderer>().material.color = baseColor;
        if (layer != -1)
            setLayer(layer);

        setSelected(false);
        setVisible(visible);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setPlatoon(PlatoonBehaviour p)
    {
        platoon = p;
        symbol.SetIcon(p.Type);
    }

    public void setLayer(int l)
    {
        layer = l;
        if (billboard != null)
            billboard.gameObject.layer = l;
        gameObject.layer = l;
    }

    public void setSource(List<UnitBehaviour> list)
    {
        isInitialized = true;
        billboard.GetComponentInChildren<CompoundHealthbarBehaviour>().setSource(list);
    }

    public void setVisible(bool vis)
    {

        gameObject.SetActive(vis);
        if (_billboard != null) {
            billboard.GetComponent<Renderer>().enabled = vis;
            symbol.GetComponent<Renderer>().enabled = vis;

        } else {
            visible = vis;
        }

        if (vis) {
            setLayer(LayerMask.NameToLayer("Selectable"));
        } else {
            setLayer(LayerMask.NameToLayer("Ignore Raycast"));
        }
    }

    public void setSelected(bool selected)
    {
        Color color;

        if (selected) {
            color = (baseColor + Color.white) / 2;
        } else {
            color = baseColor;
        }

        billboard.GetComponent<Renderer>().material.color = color;
        symbol.GetComponent<Renderer>().material.color = color;// (color + Color.white) / 2;
    }

    public void setGhost()
    {
        billboard.GetComponent<Renderer>().material.SetColor("_Emission", (2 * baseColor + Color.white) / 3);
        symbol.GetComponent<Renderer>().material.SetColor("_Emission", (2 * baseColor + Color.white) / 3);
        setVisible(true);
    }

    public void setTeam(Team t)
    {
        if (t == Team.Blue) {
            baseColor = Color.Lerp(Color.blue, Color.white, .1f);
        } else {
            baseColor = Color.red;
        }
    }
}
