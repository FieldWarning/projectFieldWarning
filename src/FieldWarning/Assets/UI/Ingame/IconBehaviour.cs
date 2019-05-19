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
using PFW.Units;

namespace PFW.UI.Ingame
{
    public class IconBehaviour : MonoBehaviour
    {
        private static Color DEFAULT_COLOR = Color.cyan;

        private int _layer = -1;

        private SymbolBehaviour _symbol;
        private Transform _billboard;

        private Color _baseColor = DEFAULT_COLOR;
        public Color BaseColor {
            set {
                if (_baseColor == DEFAULT_COLOR) {
                    _baseColor = value;
                }
            }
        }

        private bool _visible = true;

        private void Awake()
        {
            _symbol = transform.GetChild(1).GetComponent<SymbolBehaviour>();
            _billboard = transform.GetChild(0);
        }

        private void Start()
        {
            _billboard.GetComponent<Renderer>().material.color = _baseColor;
            if (_layer != -1)
                SetLayer(_layer);

            SetSelected(false);
            SetVisible(_visible);
        }

        public void SetLayer(int l)
        {
            _layer = l;
            if (_billboard != null)
                _billboard.gameObject.layer = l;
            gameObject.layer = l;
        }

        /// <summary>
        /// Mark as associated to a set of non-ghost units.
        /// </summary>
        /// <param name="list"></param>
        public void AssociateToRealUnits(List<UnitDispatcher> list)
        {
            _billboard.GetComponentInChildren<CompoundHealthbarBehaviour>().AssociateToRealUnits(list);

            // Make selectable:
            transform.gameObject.AddComponent<SelectableBehavior>().Platoon =
                    transform
                            .parent
                            .GetComponent<PlatoonBehaviour>();
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
    }
}