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

namespace PFW.UI.Ingame
{
    [ExecuteAlways]
    public class UnitLabelGFXController : UIGFXController
    {
        [Header("Functional Components")]
    #pragma warning disable 0649
        // [SerializeField]
        // private ButtonGFXController _buttonGFX;

        [SerializeField]
        private Button _button;

        [Header("Graphical Components")]
        [SerializeField]
        private Image _colorSprite;

        [SerializeField]
        private Image _borderSprite;

        [SerializeField]
        private Image _dropShadow;

        [SerializeField]
        private Image _selectionGlow;

        [SerializeField]
        private TextMeshProUGUI _unitName;

        [Header("Icons")]
        [SerializeField]
        private TextMeshProUGUI _unitIcon;

        [SerializeField]
        private TextMeshProUGUI _weaponStatusIcon;

        [SerializeField]
        private TextMeshProUGUI _loadedInfIcon;

        [SerializeField]
        private TextMeshProUGUI _routingIcon;
    #pragma warning restore 0649

        private SelectedState _selectedState = SelectedState.Deselected;
        private WeaponState _weaponState = WeaponState.Idle;
        private HoverState _hoverState = HoverState.Default;

        private enum SelectedState
        {
            Deselected,
            Selected
        }

        private enum HoverState
        {
            Default,
            Hover
        }

        private enum WeaponState
        {
            Idle,
            Aiming
        }

        private List<ColorState> GetWeaponStatusState()
        {
            Color color;
            float alpha;

            if (_weaponState == WeaponState.Idle)
                alpha = 0.3f;
            else
                alpha = 1f;

            if (_selectedState == SelectedState.Deselected) {
                if (_weaponState == WeaponState.Idle)
                    color = _accentColor;
                else
                    color = Color.white;
            } else {
                color = _baseColor;
            }

            return new List<ColorState> {
                new ColorState(_weaponStatusIcon, color, alpha)
            };
        }

        private List<ColorState> GetHoverState()
        {
            if (_selectedState == SelectedState.Deselected) {
                return new List<ColorState> {
                    new ColorState(
                            _colorSprite,
                            null,
                            (_hoverState == HoverState.Default ? 0.7f : 1f)),
                    new ColorState(
                            _borderSprite,
                            null,
                            (_hoverState == HoverState.Default ? 0.525f : 1f))
                };
            } else {
                return new List<ColorState>();
            }
        }

        private List<ColorState> GetSelectedState()
        {
            if (_selectedState == SelectedState.Deselected) {
                return new List<ColorState> {
                    new ColorState(_colorSprite, _baseColor, null),
                    new ColorState(_borderSprite, _accentColor, null),
                    new ColorState(_unitIcon, Color.white, 1f),
                    new ColorState(_unitName, Color.white, 1f),
                    new ColorState(_dropShadow, Color.white, 1f),
                    new ColorState(_selectionGlow, _accentColor, 0f)
                };
            } else {
                return new List<ColorState> {
                    new ColorState(_colorSprite, Color.white, 1f),
                    new ColorState(_borderSprite, _baseColor, 1f),
                    new ColorState(_unitIcon, _baseColor, 1f),
                    new ColorState(_unitName, _baseColor, 1f),
                    new ColorState(_dropShadow, Color.white, 0f),
                    new ColorState(_selectionGlow, _accentColor, 1f)
                };
            }
        }

        private UIState GenerateState()
        {
            return UIState.Merge(
                    GetHoverState(),
                    GetSelectedState(),
                    GetWeaponStatusState());
        }

        protected override void Start()
        {
            base.Start();

            SetInitialState(GenerateState());
        }

        private void UpdateState()
        {
            TransitionToState(GenerateState());
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetButtonDown("Jump"))
                ToggleWeaponStatus();
        }

        public void OnButtonClick()
        {
            if (_selectedState != SelectedState.Selected)
                _selectedState = SelectedState.Selected;
            else
                _selectedState = SelectedState.Deselected;

            UpdateState();
        }

        public void OnButtonMouseover()
        {
            if (_selectedState != SelectedState.Selected) {
                _hoverState = HoverState.Hover;
                UpdateState();
            }
        }

        public void OnButtonMouseout()
        {
            if (_selectedState != SelectedState.Selected) {
                _hoverState = HoverState.Default;
                UpdateState();
            }
        }

        public void ToggleWeaponStatus()
        {
            if (_weaponState == WeaponState.Idle)
                _weaponState = WeaponState.Aiming;
            else
                _weaponState = WeaponState.Idle;

            UpdateState();
        }
    }
}
