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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using NColor = System.Nullable<UnityEngine.Color>;
using NFloat = System.Nullable<System.Single>;

namespace PFW.UI.Ingame
{
    public class ColorState
    {
        public Graphic Component;
        public NColor Color;
        public NFloat Alpha;

        public ColorState(Graphic component, NColor color, NFloat alpha)
        {
            Component = component;
            Color = color;
            Alpha = alpha;
        }
    }
    public class ColorTransition
    {
        public Graphic Component;
        public Color ColorFrom;
        public Color ColorTo;

        public ColorTransition(
                Graphic component,
                Color colorFrom,
                NColor colorTo,
                NFloat alphaTo)
        {
            Component = component;
            ColorFrom = colorFrom;

            Vector3 rgb = (colorTo != null)
                    ? UIColors.RGB((Color) colorTo)
                    : UIColors.RGB(colorFrom);

            float a = (float) (alphaTo != null ? alphaTo : colorFrom.a);

            ColorTo = new Color(rgb.x, rgb.y, rgb.z, a);
        }

        public void Animate(float lerp)
        {
            Color newColor = Color.Lerp(ColorFrom, ColorTo, lerp);
            Component.color = newColor;
        }
    }

    // TODO: This is basically just an abstraction for List<ColorState> that adds some helpful methods
    // Maybe there's a better way to do this?
    public class UIState
    {
        public List<ColorState> ColorStates;

        public UIState(List<ColorState> colorStates)
        {
            ColorStates = colorStates;
        }

        public static UIState Diff(UIState fromState, UIState toState)
        {
            List<ColorState> colors = new List<ColorState>();

            foreach (ColorState fromColor in fromState.ColorStates)
                foreach (ColorState toColor in toState.ColorStates)
                    if (fromColor.Component == toColor.Component
                            && fromColor.Color != toColor.Color)
                        colors.Add(toColor);

            return new UIState(colors);
        }

        public static UIState Merge(params List<ColorState>[] states)
        {
            var merged = new List<ColorState>();

            for (int i = 0; i < states.Length - 1; i++) {
                var aColors = states[i];
                var bColors = states[i + 1];

                List<ColorState> original = aColors.Where(aColor => {
                    foreach (ColorState bColor in bColors)
                        if (bColor.Component == aColor.Component)
                            return false;
                    return true;
                }).ToList();

                List<ColorState> additions = bColors.Where(bColor => {
                    foreach (ColorState aColor in aColors)
                        if (aColor.Component == bColor.Component)
                            return false;
                    return true;
                }).ToList();

                List<ColorState> modifications = bColors.Where(bColor =>
                        (!additions.Contains(bColor))).ToList();

                foreach (ColorState mColor in modifications) {
                    ColorState aColor = aColors.Find(a => a.Component == mColor.Component);
                    mColor.Color = (mColor.Color != null) ? mColor.Color : aColor.Color;
                    mColor.Alpha = (mColor.Alpha != null) ? mColor.Alpha : aColor.Alpha;
                }

                merged.AddRange(original);
                merged.AddRange(additions);
                merged.AddRange(modifications);
            }

            return new UIState(merged);
        }
    }
}