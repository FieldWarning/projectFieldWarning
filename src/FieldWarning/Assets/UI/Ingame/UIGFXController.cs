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

namespace PFW.UI.Ingame
{
    [ExecuteAlways]
    public abstract class UIGFXController : MonoBehaviour
    {
        [Header("Auto-Size")]
        [SerializeField]
        private bool _autoSizeEnabled = false;
    #pragma warning disable 0649
        [SerializeField]
        private GameObject _autoSizeTarget;
    #pragma warning restore 0649

        [Header("Colors")]
        [SerializeField]
        protected UIColor _baseColorID;

        [SerializeField]
        protected UIColor _accentColorID;
        protected Color _baseColor;
        protected Color _accentColor;

        private RectTransform _targetRect;
        private RectTransform _rect;

        protected class ComponentState
        {
            public Graphic Component;
            public Color Color;

            public ComponentState(Graphic component, Color color)
            {
                Component = component;
                Color = color;
            }
        }

        protected List<ComponentState> _uiState;

        protected class Transition
        {
            public Graphic Graphic;
            public Color ColorFrom;
            public Color ColorTo;

            public Transition(Graphic component, Color colorFrom, Color colorTo)
            {
                Graphic = component;
                ColorFrom = colorFrom;
                ColorTo = colorTo;
            }

            public void Animate(float lerp)
            {
                Color newColor = Color.Lerp(ColorFrom, ColorTo, lerp);
                Graphic.color = newColor;
            }
        }

        protected List<Transition> _transitionList = new List<Transition>();

        protected float _lerp;
        protected bool _isAnimating = false;

        [SerializeField]
        protected float _animationSpeed = 10f;

        protected virtual void Start()
        {
            if (_autoSizeEnabled && _autoSizeTarget != null) {
                _rect = GetComponent<RectTransform>();
                _targetRect = _autoSizeTarget.GetComponent<RectTransform>();
            }

            _baseColor = UIColors.GetColor(_baseColorID);
            _accentColor = UIColors.GetColor(_accentColorID);
        }

        protected virtual void Update()
        {
            if (_rect != null && _targetRect != null) {
                Vector2 targetSizeDelta = _targetRect.sizeDelta;

                if (targetSizeDelta != _rect.sizeDelta)
                    _rect.sizeDelta = targetSizeDelta;
            }

            if (_isAnimating)
                RunAnimations();
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
        protected Color GetColorWithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        protected void TransitionToState(List<ComponentState> state)
        {
            _transitionList.Clear();
            _lerp = 0f;

            foreach (ComponentState componentState in state)
                _transitionList.Add(new Transition(
                        componentState.Component,
                        componentState.Component.color,
                        componentState.Color));

            if (!_isAnimating)
                _isAnimating = true;
        }
    }
}
