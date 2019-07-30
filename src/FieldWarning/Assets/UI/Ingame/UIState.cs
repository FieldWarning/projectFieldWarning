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

namespace PFW.UI.Ingame
{
    // Color State
    public class ColorState
    {
        // TODO: Add alpha as a third argument?
        public Graphic Component;
        public Color Color;

        public ColorState(Graphic component, Color color)
        {
            Component = component;
            Color = color;
        }
    }
    public class ColorTransition
    {
        // TODO: Add alpha as a third and fifth argument?
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

    // Activation State
    public class ActivationState
    {
        public GameObject Component;
        public bool Activated;

        public ActivationState(GameObject component, bool activated)
        {
            Component = component;
            Activated = activated;
        }
    }
    public class ActivationTransition
    {
        public GameObject Component;
        public bool FromState;
        public bool ToState;
        private Graphic _graphic;
        private Color _fromColor;
        private Color _toColor;

        public ActivationTransition(
                GameObject component,
                bool fromState,
                bool toState)
        {
            Component = component;
            FromState = fromState;
            ToState = toState;
            _graphic = component.GetComponent<Graphic>();
            _fromColor = FromState ? _graphic.color : UIColors.WithAlpha(_graphic.color, 0f);
            _toColor = ToState ? _graphic.color : UIColors.WithAlpha(_graphic.color, 0f);
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

    // Combined State
    public class UIState
    {
        public List<ColorState> StateColors;
        public List<ActivationState> StateActivations;

        public UIState(List<ColorState> stateColors, List<ActivationState> stateActivations)
        {
            StateColors = stateColors;
            StateActivations = stateActivations;
        }

        public static UIState Diff(UIState fromState, UIState toState)
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