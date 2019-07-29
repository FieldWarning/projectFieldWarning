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

        [SerializeField]
        private Image _colorSprite;

        [SerializeField]
        private Image _borderSprite;
    #pragma warning restore 0649

        [Header("Configuration")]
        [SerializeField]
        private float _animationSpeed = 10f;

        private float _lerp;
        private bool _isAnimating = false;

        private float _colorAlpha = 0.7f;
        private float _borderAlpha = 0.525f;
        private float _colorAlphaHover = 1f;
        private float _borderAlphaHover = 1f;

        protected override void Start()
        {
            base.Start();
            _colorSprite.color = GetColorWithAlpha(_baseColor, _colorAlpha);
            _borderSprite.color = GetColorWithAlpha(_accentColor, _borderAlpha);
        }

        protected override void Update()
        {
            base.Update();

            if (_isAnimating)
                RunAnimations();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartTransition(TransitionDirection.ToHoverState);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartTransition(TransitionDirection.ToDefaultState);
        }

        protected void StartTransition(TransitionDirection direction)
        {
            var colorFrom = _colorSprite.color;
            var borderColorFrom = _borderSprite.color;
            var colorTo = (direction == TransitionDirection.ToHoverState)
                    ? GetColorWithAlpha(_baseColor, _colorAlphaHover)
                    : GetColorWithAlpha(_baseColor, _colorAlpha);
            var borderColorTo = (direction == TransitionDirection.ToHoverState)
                    ? GetColorWithAlpha(_accentColor, _borderAlphaHover)
                    : GetColorWithAlpha(_accentColor, _borderAlpha);

            _transitionList.Clear();

            Transition colorTransition = new Transition(_colorSprite, colorFrom, colorTo);
            Transition borderTransition = new Transition(_borderSprite, borderColorFrom, borderColorTo);

            _transitionList.Add(colorTransition);
            _transitionList.Add(borderTransition);

            _lerp = 0f;

            if (!_isAnimating)
                _isAnimating = true;
        }

        private void RunAnimations()
        {
            _lerp = Mathf.Clamp(_lerp + Time.deltaTime * _animationSpeed, 0f, 1f);

            foreach (var transition in _transitionList)
                transition.Animate(_lerp);

            if (_lerp >= 1f)
                _isAnimating = false;
        }

        // TODO: Find out if Unity already has a utility like this, and if not, move it to a static class
        private Color GetColorWithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        protected enum TransitionDirection
        {
            ToHoverState,
            ToDefaultState
        }
    }
}
