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
        [Header("Main Components")]
    #pragma warning disable 0649
        [SerializeField]
        private Button _button;

        [SerializeField]
        private Image _colorSprite;

        [SerializeField]
        private Image _borderSprite;

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

        protected override void Start()
        {
            base.Start();
            _colorSprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _colorAlpha);
            _borderSprite.color = new Color(_accentColor.r, _accentColor.g, _accentColor.b, _borderAlpha);
            _unitIcon.color = Color.white;
            _unitName.color = Color.white;
            _weaponStatusIcon.color = new Color(_accentColor.r, _accentColor.g, _accentColor.b, _weaponStatusIdleAlpha);
        }
    }
}
