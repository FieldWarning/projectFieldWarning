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

namespace Assets.Ingame.UI
{
    public class IconBehaviour : SelectableBehavior
    {
        private int _layer = -1;
        private PlatoonBehaviour _platoon;

        private SymbolBehaviour _symbol;
        SymbolBehaviour symbol {
            get {
                if (_symbol == null)
                    _symbol = transform.GetChild(1).GetComponent<SymbolBehaviour>();

                return _symbol;
            }
        }

        private Transform _billboard;
        Transform billboard {
            get {
                if (_billboard == null) {
                    _billboard = transform.GetChild(0);
                }

                return _billboard;
            }
        }

        private bool _init = false;
        private Color _baseColor = Color.blue;
        private bool _visible = true;


        // Use this for initialization
        void Start()
        {
            billboard.GetComponent<Renderer>().material.color = _baseColor;
            if (_layer != -1)
                SetLayer(_layer);

            SetSelected(false);
            SetVisible(_visible);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPlatoon(PlatoonBehaviour p)
        {
            _platoon = p;
            symbol.SetIcon(p.Type);
        }

        public void SetLayer(int l)
        {
            _layer = l;
            if (billboard != null)
                billboard.gameObject.layer = l;
            gameObject.layer = l;
        }

        public void SetSource(List<UnitBehaviour> list)
        {
            billboard.GetComponentInChildren<CompoundHealthbarBehaviour>().SetSource(list);
        }

        public void SetVisible(bool vis)
        {
            gameObject.SetActive(vis);
            if (_billboard != null) {
                billboard.GetComponent<Renderer>().enabled = vis;
                symbol.GetComponent<Renderer>().enabled = vis;

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

            billboard.GetComponent<Renderer>().material.color = color;
            symbol.GetComponent<Renderer>().material.color = color;// (color + Color.white) / 2;
        }

        public void SetGhost()
        {
            billboard.GetComponent<Renderer>().material.SetColor("_Emission", (2 * _baseColor + Color.white) / 3);
            symbol.GetComponent<Renderer>().material.SetColor("_Emission", (2 * _baseColor + Color.white) / 3);
            SetVisible(true);
        }

        public void SetTeam(Team t)
        {
            if (t == Team.Blue) {
                _baseColor = Color.Lerp(Color.blue, Color.white, .1f);
            } else {
                _baseColor = Color.red;
            }
        }
    }
}