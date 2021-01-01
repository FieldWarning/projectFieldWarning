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

namespace PFW.Model.Match
{
    public class Team : MonoBehaviour
    {
        public enum TeamName { UNDEFINED, USSR, NATO };
        public TeamName Name;
        public TeamColorScheme ColorScheme;

        public List<PlayerData> Players { get; } = new List<PlayerData>();

        public bool IsEnemy(Team t)
        {
            return Name != t.Name;
        }
    }

    /// <summary>
    ///     Ideally this would include all colors used
    ///     by the game so that we can easily tweak
    ///     the color scheme by changing one place.
    /// </summary>
    [System.Serializable]
    public class TeamColorScheme
    {
        [Tooltip("Color for the team's platoon labels and most other things.")]
        public Color BaseColor;
        [Tooltip("Color for the team's selected platoons.")]
        public Color SelectedColor;
        [Tooltip("Color for the team's captured zones.")]
        public Color CapturedColor;
        [Tooltip("Color for the team's ghost platoons.")]
        public Color GhostColor;
    }
}
