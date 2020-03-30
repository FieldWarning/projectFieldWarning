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
using UnityEngine.EventSystems;

namespace PFW.UI.Ingame
{
    [ExecuteAlways]
    public class ButtonGFXController : UIGFXController, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Components")]
    #pragma warning disable 0649
        [SerializeField]
        private Button _button;
    #pragma warning restore 0649
        public Image ColorSprite;
        public Image BorderSprite;

        [Header("Colors")]
        private float _colorAlpha = 0.7f;
        private float _borderAlpha = 0.525f;
        private float _colorAlphaHover = 1f;
        private float _borderAlphaHover = 1f;

        private List<ComponentState> _defaultState;
        private List<ComponentState> _hoverState;

        protected override void Start()
        {
            base.Start();
            ColorSprite.color = GetColorWithAlpha(_baseColor, _colorAlpha);
            BorderSprite.color = GetColorWithAlpha(_accentColor, _borderAlpha);

            _defaultState = new List<ComponentState> {
                new ComponentState(ColorSprite, GetColorWithAlpha(_baseColor, _colorAlpha)),
                new ComponentState(BorderSprite, GetColorWithAlpha(_accentColor, _borderAlpha))
            };
            _hoverState = new List<ComponentState> {
                new ComponentState(ColorSprite, _baseColor),
                new ComponentState(BorderSprite, _accentColor)
            };
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TransitionToState(_hoverState);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TransitionToState(_defaultState);
        }
    }
}
