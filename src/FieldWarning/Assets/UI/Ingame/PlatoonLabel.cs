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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using PFW.Units;
using PFW.Model.Game;

namespace PFW.UI.Ingame
{
    /// <summary>
    ///     The UI overlay label/nameplate for a platoon.
    /// </summary>
    public sealed class PlatoonLabel : MonoBehaviour
    {
        private TeamColorScheme _color;

        [Header("Graphical Components")]
        [SerializeField]
        private Image _colorSprite = null;

        [SerializeField]
        private Image _borderSprite = null;

        [SerializeField]
        private Image _dropShadow = null;

        [SerializeField]
        private Image _selectionGlow = null;

        [SerializeField]
        private TextMeshProUGUI _unitName = null;

        public void SetColorScheme(TeamColorScheme colorScheme)
        {
            _color = colorScheme;
            SetColor(colorScheme.BaseColor);
        }

        private void SetColor(Color color)
        {
            _colorSprite.color = color;
            _borderSprite.color = (2 * color + Color.white / 3);
        }

        public void SetLayer(int l)
        {
            gameObject.layer = l;
        }

        /// <summary>
        /// Mark as associated to a set of non-ghost units.
        /// </summary>
        /// This used to be necessary when the 
        /// unit labels showed platoon health,
        /// but since they no longer do, 
        /// it is unclear if we still need it.
        public void AssociateToRealUnits(List<UnitDispatcher> list)
        {
            //_billboard.GetComponentInChildren<CompoundHealthbarBehaviour>().AssociateToRealUnits(list);

            // Make selectable:
            transform.gameObject.AddComponent<SelectableBehavior>().Platoon =
                    transform
                            .parent
                            .GetComponent<PlatoonBehaviour>();
        }

        public void SetVisible(bool vis)
        {
            gameObject.SetActive(vis);

            if (vis)
            {
                SetLayer(LayerMask.NameToLayer("Selectable"));
            }
            else
            {
                SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
            }
        }

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                SetColor(_color.SelectedColor);
            }
            else 
            {
                SetColor(_color.BaseColor);
            }
        }

        /// <summary>
        ///     Ghost platoons have paler icons which hint that they're not real.
        /// </summary>
        public void SetGhost()
        {
            SetColor(_color.GhostColor);
            SetVisible(true);
        }
    }
}
