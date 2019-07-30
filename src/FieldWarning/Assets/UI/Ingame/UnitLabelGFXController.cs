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

using System.Collections;
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

        private float _colorAlpha = 0.7f;
        private float _borderAlpha = 0.525f;
        private float _weaponStatusIdleAlpha = 0.3f;

        private UIState _defaultState;
        private UIState _hoverState;
        private UIState _selectedState;

        protected override void Start()
        {
            base.Start();
            _unitIcon.color = Color.white;
            _unitName.color = Color.white;
            _weaponStatusIcon.color = UIColors.WithAlpha(_accentColor, _weaponStatusIdleAlpha);

            _defaultState = new UIState(new List<ColorState> {
                    new ColorState(_colorSprite, _baseColor, _colorAlpha),
                    new ColorState(_borderSprite, _accentColor, _borderAlpha),
                    new ColorState(_unitName, Color.white, 1f),
                    new ColorState(_unitIcon, Color.white, 1f),
                    new ColorState(_weaponStatusIcon, _accentColor, _weaponStatusIdleAlpha),
                    new ColorState(_dropShadow, Color.white, 1f),
                    new ColorState(_selectionGlow, _accentColor, 0f)});

            _hoverState = UIState.Merge(
                    _defaultState.StateColors,
                    new List<ColorState> {
                        new ColorState(_colorSprite, null, 1f),
                        new ColorState(_borderSprite, null, 1f)
                    });

            _selectedState = UIState.Merge(
                    _defaultState.StateColors,
                    new List<ColorState> {
                        new ColorState(_colorSprite, Color.white, 1f),
                        new ColorState(_borderSprite, _baseColor, 1f),
                        new ColorState(_unitIcon, _baseColor, 1f),
                        new ColorState(_unitName, _baseColor, 1f),
                        new ColorState(_dropShadow, null, 0f),
                        new ColorState(_selectionGlow, null, 1f)
                    });

            SetInitialState(_defaultState);
        }

        public void OnButtonClick()
        {
            if (_currentState != _selectedState) {
                TransitionToState(_selectedState);
            } else {
                TransitionToState(_hoverState);
            }
        }

        public void OnButtonMouseover()
        {
            if (_currentState != _selectedState) {
                TransitionToState(_hoverState);
            }
        }

        public void OnButtonMouseout()
        {
            if (_currentState != _selectedState) {
                TransitionToState(_defaultState);
            }
        }
    }
}
