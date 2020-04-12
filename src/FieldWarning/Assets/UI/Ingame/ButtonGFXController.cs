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
using UnityEngine.EventSystems;

namespace PFW.UI.Ingame
{

    /// <summary>
    ///     Graphics controller for buttons. 
    ///     Describes how a button should graphically change
    ///     in response to e.g. user interaction.
    /// </summary>
    /// 
    ///     Warning: This script is incomplete and unused,
    ///          so it may make sense to remove it.
    ///          Existing logic can be moved to the PlatoonLabel
    ///          class if it is useful to do so.
    [ExecuteAlways]
    public class ButtonGFXController : UIGFXController, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Components")]
        [SerializeField]
        private Button _button = null;
        public Image ColorSprite;
        public Image BorderSprite;

        [Header("Colors")]
        private float _colorAlpha = 0.7f;
        private float _borderAlpha = 0.525f;
        private float _colorAlphaHover = 1f;
        private float _borderAlphaHover = 1f;

        private List<ColorState> _defaultState;
        private List<ColorState> _hoverState;

        protected override void Start()
        {
            base.Start();
            ColorSprite.color = UIColors.WithAlpha(_baseColor, _colorAlpha);
            BorderSprite.color = UIColors.WithAlpha(_accentColor, _borderAlpha);

            _defaultState = new List<ColorState> {
                new ColorState(ColorSprite,_baseColor, _colorAlpha),
                new ColorState(BorderSprite, _accentColor, _borderAlpha)
            };
            _hoverState = new List<ColorState> {
                new ColorState(ColorSprite, _baseColor, _colorAlphaHover),
                new ColorState(BorderSprite, _accentColor, _borderAlphaHover)
            };
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TransitionToState(UIState.Merge(_hoverState));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TransitionToState(UIState.Merge(_defaultState));
        }
    }
}
