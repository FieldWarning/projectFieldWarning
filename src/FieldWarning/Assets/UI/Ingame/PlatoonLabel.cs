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
using PFW.Model.Match;
using PFW.Model.Armory;

namespace PFW.UI.Ingame
{
    /// <summary>
    ///     The UI overlay label/nameplate for a platoon.
    /// </summary>
    public sealed class PlatoonLabel : MonoBehaviour
    {
        private TeamColorScheme _color;

        [SerializeField]
        private Button _button = null;

        [Header("Graphical Components")]
        [SerializeField]
        private Image _colorSprite = null;

        [SerializeField]
        private Image _borderSprite = null;

        [SerializeField]
        private TextMeshProUGUI _unitTypeIcon = null;

        [SerializeField]
        private TextMeshProUGUI _unitName = null;

        public bool Visible { 
            get { 
                return gameObject.activeSelf;  
            }
            set {
                gameObject.SetActive(value);
            }
        }

        private void SetColor(Color color)
        {
            _colorSprite.color = color;
            _borderSprite.color = (2 * color + Color.white / 3);
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
        ///     Ghost platoons have paler icons which hint that they're not real,
        ///     and are always visible to their owner.
        /// </summary>
        public void InitializeAsGhost(Unit unit, TeamColorScheme colorScheme)
        {
            _unitName.text = unit.Name;
            _color = colorScheme;
            _unitTypeIcon.text = unit.Config.LabelIcon;
            SetColor(_color.GhostColor);
        }

        /// <summary>
        ///     Platoon labels for real units are initialized
        ///     before the platoon is fully spawned and activated,
        ///     so they have to be made visible in a separate, later call.
        /// </summary>
        public void InitializeAsReal(
                Unit unit, 
                TeamColorScheme colorScheme, 
                PlatoonBehaviour platoon)
        {
            _unitName.text = unit.Name;
            _color = colorScheme;
            _unitTypeIcon.text = unit.Config.LabelIcon;
            SetColor(colorScheme.BaseColor);

            // When the label is clicked, notify the selection manager:
            _button.onClick.AddListener(
                    () => MatchSession.Current.PlatoonLabelClicked(platoon));
        }
    }
}
