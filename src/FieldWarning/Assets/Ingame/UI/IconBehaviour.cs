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

namespace PFW.Ingame.UI
{
    public class IconBehaviour : SelectableBehavior
    {
        private int _layer = -1;
        private PlatoonBehaviour _platoon;

        private SymbolBehaviour _symbol;
        private Transform _billboard;

        private bool _init = false;
        private Color _baseColor = Color.blue;
        private bool _visible = true;

        void Awake()
        {
            _symbol = transform.GetChild(1).GetComponent<SymbolBehaviour>();
            _billboard = transform.GetChild(0);
        }

        void Start()
        {
            _billboard.GetComponent<Renderer>().material.color = _baseColor;
            if (_layer != -1)
                SetLayer(_layer);

            SetSelected(false);
            SetVisible(_visible);
        }

        void Update()
        {

        }

        public void SetPlatoon(PlatoonBehaviour p)
        {
            _platoon = p;
            _symbol.SetIcon(p.Type);
        }

        public void SetLayer(int l)
        {
            _layer = l;
            if (_billboard != null)
                _billboard.gameObject.layer = l;
            gameObject.layer = l;
        }

        public void SetSource(List<UnitBehaviour> list)
        {
            _billboard.GetComponentInChildren<CompoundHealthbarBehaviour>().SetSource(list);
        }

        public void SetVisible(bool vis)
        {
            gameObject.SetActive(vis);
            if (_billboard != null) {
                _billboard.GetComponent<Renderer>().enabled = vis;
                _symbol.GetComponent<Renderer>().enabled = vis;

            } else {
                _visible = vis;
            }

            if (vis) {
                SetLayer(LayerMask.NameToLayer("Selectable"));
            } else {
                SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
            }
        }

        public void SetSelected(bool selected)
        {
            Color color;

            if (selected) {
                color = (_baseColor + Color.white) / 2;
            } else {
                color = _baseColor;
            }

            _billboard.GetComponent<Renderer>().material.color = color;
            _symbol.GetComponent<Renderer>().material.color = color;// (color + Color.white) / 2;
        }

        public void SetGhost()
        {
            _billboard.GetComponent<Renderer>().material.SetColor("_Emission", (2 * _baseColor + Color.white) / 3);
            _symbol.GetComponent<Renderer>().material.SetColor("_Emission", (2 * _baseColor + Color.white) / 3);
            SetVisible(true);
        }

        public void SetTeam(PFW.Model.Game.Team t)
        {
            _baseColor = t.Color;
        }
    }
}