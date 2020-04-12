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

using System;
using UnityEngine;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Constants for commonly-used colors and utility methods
    /// for UI color-related logic.
    /// </summary>
    public static class UIColors
    {
        private static Color _blue = new Color(0.2f, 0.37f, 0.6f, 1f);
        private static Color _blueLight = new Color(0.557f, 0.7f, 0.9f, 1f);
        private static Color _red = new Color(0.67f, 0.16f, 0.153f, 1f);
        private static Color _redLight = new Color(0.9f, 0.592f, 0.588f, 1f);
        private static Color _yellow = new Color(1f, 0.784f, 0.247f, 1f);

        public static Color GetColor(UIColor color)
        {
            switch (color) 
            {
            case UIColor.Blue:      return _blue;
            case UIColor.BlueLight: return _blueLight;
            case UIColor.Red:       return _red;
            case UIColor.RedLight:  return _redLight;
            case UIColor.Yellow:    return _yellow;
            default:                throw new Exception($"Unknown color `{color}` requested!");
            }
        }

        public static Vector3 RGB(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        /// <summary>
        /// Get the provided color with the provided alpha channel.
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }

    public enum UIColor
    {
        Blue,
        BlueLight,
        Red,
        RedLight,
        Yellow
    }
}
