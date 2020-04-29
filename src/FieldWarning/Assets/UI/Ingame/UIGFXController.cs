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

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Currently this is only used as a toplevle resize script 
    /// for the unit labels (see child classes).
    /// 
    /// Warning: This script is incomplete and unused,
    ///          so it may make sense to remove it.
    ///          Existing logic can be moved to the PlatoonLabel
    ///          class if it is useful to do so.
    /// </summary>
    [ExecuteAlways]
    public abstract class UIGFXController : MonoBehaviour
    {
        [Header("Auto-Size")]
        [SerializeField]
        private bool _autoSizeEnabled = false;
        [SerializeField]
        private GameObject _autoSizeTarget = null;

        [Header("Colors")]
        [SerializeField]
        protected UIColor _baseColorID;

        [SerializeField]
        protected UIColor _accentColorID;
        protected Color _baseColor;
        protected Color _accentColor;

        // A child rect holding most of the label
        private RectTransform _targetRect;
        // A toplevel rect
        private RectTransform _rect;

        protected List<ColorTransition> _colorTransitions = new List<ColorTransition>();

        protected UIState _currentState;
        protected UIState _nextState;

        protected float _lerp;
        protected bool _isAnimating = false;

        [SerializeField]
        protected float _animationSpeed = 10f;

        protected virtual void Start()
        {
            if (_autoSizeEnabled && _autoSizeTarget != null) 
            {
                _rect = GetComponent<RectTransform>();
                _targetRect = _autoSizeTarget.GetComponent<RectTransform>();
            }

            _baseColor = UIColors.GetColor(_baseColorID);
            _accentColor = UIColors.GetColor(_accentColorID);
        }

        protected virtual void Update()
        {
            if (_rect != null && _targetRect != null) 
            {
                Vector2 targetSizeDelta = _targetRect.sizeDelta;

                if (targetSizeDelta != _rect.sizeDelta)
                { 
                    _rect.sizeDelta = targetSizeDelta;
                }
            }

            if (_isAnimating)
            {
                RunAnimations();
            }
        }

        private void RunAnimations()
        {
            _lerp = Mathf.Clamp(_lerp + Time.deltaTime * _animationSpeed, 0f, 1f);

            foreach (ColorTransition transition in _colorTransitions)
            {
                transition.Animate(_lerp);
            }

            if (_lerp >= 1f)
            {
                _isAnimating = false;
            }
        }

        protected void TransitionToState(UIState state)
        {
            _currentState = state;

            _colorTransitions.Clear();
            _lerp = 0f;

            foreach (ColorState colorState in state.ColorStates)
            { 
                _colorTransitions.Add(new ColorTransition(
                        colorState.Component,
                        colorState.Component.color,
                        colorState.Color,
                        colorState.Alpha));
            }

            if (!_isAnimating)
            {
                _isAnimating = true;
            }
        }

        protected void SetInitialState(UIState state)
        {
            foreach (ColorState colorState in state.ColorStates)
            {
                colorState.Component.color = UIColors.WithAlpha(
                        (Color)colorState.Color,
                        (float)colorState.Alpha);
            }

            _currentState = state;
        }
    }
}
