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

        // Color State
        protected class ColorState
        {
            public Graphic Component;
            public Color Color;

            public ColorState(Graphic component, Color color)
            {
                Component = component;
                Color = color;
            }
        }
        protected class ColorTransition
        {
            public Graphic Component;
            public Color ColorFrom;
            public Color ColorTo;

            public ColorTransition(Graphic component, Color colorFrom, Color colorTo)
            {
                Component = component;
                ColorFrom = colorFrom;
                ColorTo = colorTo;
            }

            public void Animate(float lerp)
            {
                Color newColor = Color.Lerp(ColorFrom, ColorTo, lerp);
                Component.color = newColor;
            }
        }
        protected List<ColorTransition> _colorTransitions = new List<ColorTransition>();

        // Activation State
        protected class ActivationState
        {
            public GameObject Component;
            public bool Activated;

            public ActivationState(GameObject component, bool activated)
            {
                Component = component;
                Activated = activated;
            }
        }
        protected class ActivationTransition
        {
            public GameObject Component;
            public bool FromState;
            public bool ToState;
            private Graphic _graphic;
            private Color _fromColor;
            private Color _toColor;
            private UIGFXController _host;

            public ActivationTransition(
                    ref GameObject component,
                    bool fromState,
                    bool toState,
                    UIGFXController host)
            {
                Component = component;
                FromState = fromState;
                ToState = toState;
                _host = host;
                _graphic = component.GetComponent<Graphic>();
                _fromColor = FromState ? _graphic.color : host.GetColorWithAlpha(_graphic.color, 0f);
                _toColor = ToState ? _graphic.color : host.GetColorWithAlpha(_graphic.color, 0f);
            }

            public void Animate(float lerp)
            {
                if (!Component.activeSelf) {
                    _graphic.color = _fromColor;
                    Component.SetActive(true);
                }

                Color newColor = Color.Lerp(_fromColor, _toColor, lerp);
                _graphic.color = newColor;

                if (lerp >= 1f && ToState == false) {
                    Component.SetActive(false);
                    _graphic.color = _fromColor;
                }
            }
        }
        protected List<ActivationTransition> _activationTransitions = new List<ActivationTransition>();

        // Combined State
        protected class UIState
        {
            public List<ColorState> StateColors;
            public List<ActivationState> StateActivations;

            public UIState(List<ColorState> stateColors, List<ActivationState> stateActivations)
            {
                StateColors = stateColors;
                StateActivations = stateActivations;
            }
        }

        protected UIState _currentState;
        protected UIState _nextState;

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

            foreach (var transition in _colorTransitions)
                transition.Animate(_lerp);
            foreach (var transition in _activationTransitions)
                transition.Animate(_lerp);

            if (_lerp >= 1f) {
                _isAnimating = false;
                _currentState = _nextState;
                _nextState = null;
            }
        }

        // TODO: Find out if Unity already has a utility like this, and if not, move it to a static class
        protected Color GetColorWithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        // protected void TransitionToState(List<ColorState> state)
        protected void TransitionToState(UIState state)
        {
            _nextState = state;

            if (!_isAnimating)
                state = DiffStates(_currentState, state);

            _colorTransitions.Clear();
            _activationTransitions.Clear();
            _lerp = 0f;

            foreach (ColorState colorState in state.StateColors)
                _colorTransitions.Add(new ColorTransition(
                        colorState.Component,
                        colorState.Component.color,
                        colorState.Color));

            foreach (ActivationState activationState in state.StateActivations)
                _activationTransitions.Add(new ActivationTransition(
                        ref activationState.Component,
                        activationState.Component.activeSelf,
                        activationState.Activated,
                        this));

            if (!_isAnimating)
                _isAnimating = true;
        }

        private UIState DiffStates(UIState fromState, UIState toState)
        {
            List<ColorState> colors = new List<ColorState>();
            List<ActivationState> activations = new List<ActivationState>();

            foreach (ColorState fromColor in fromState.StateColors)
                foreach (ColorState toColor in toState.StateColors)
                    if (fromColor.Component == toColor.Component
                            && fromColor.Color != toColor.Color)
                        colors.Add(toColor);

            foreach (ActivationState fromActivation in fromState.StateActivations)
                foreach (ActivationState toActivation in toState.StateActivations)
                    if (fromActivation.Component == toActivation.Component
                            && fromActivation.Activated != toActivation.Activated)
                        activations.Add(toActivation);

            return new UIState(colors, activations);
        }
    }
}
