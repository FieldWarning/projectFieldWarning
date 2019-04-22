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

using PFW.Units;
using PFW.Units.Component.Vision;

namespace PFW.Model.Game
{
    /// <summary>
    /// Holds various lists of the units existing in a match.
    /// </summary>
    public class UnitRegistry
    {
        private Team _localTeam;

        public List<UnitDispatcher> Units { get; } =
                new List<UnitDispatcher>();

        // TODO
        //public List<UnitDispatcher> RedUnits { get; } =
        //        new List<UnitDispatcher>();
        //public List<UnitDispatcher> BlueUnits { get; } =
        //        new List<UnitDispatcher>();

        public List<UnitDispatcher> AllyUnits { get; } =
                new List<UnitDispatcher>();
        public List<UnitDispatcher> EnemyUnits { get; } =
                new List<UnitDispatcher>();

        public List<VisionComponent> AllyVisionComponents { get; } =
                new List<VisionComponent>();
        public List<VisionComponent> EnemyVisionComponents { get; } =
                new List<VisionComponent>();

        public UnitRegistry(Team localTeam)
        {
            _localTeam = localTeam;
        }

        /// <summary>
        /// Must notify the registry every time a
        /// (real, active) unit is created.
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterUnitBirth(UnitDispatcher unit)
        {
            Units.Add(unit);

            VisionComponent visibleBehavior = unit.VisionComponent;
            if (unit.Platoon.Owner.Team == _localTeam) {
                AllyUnits.Add(unit);
                AllyVisionComponents.Add(visibleBehavior);
            } else {
                EnemyUnits.Add(unit);
                EnemyVisionComponents.Add(visibleBehavior);
            }
        }

        /// <summary>
        /// Must notify the registry every time a
        /// (real, active) unit is removed.
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterUnitDeath(UnitDispatcher unit)
        {
            Units.Remove(unit);

            VisionComponent visionComponent = unit.VisionComponent;
            if (unit.Platoon.Owner.Team == _localTeam) {
                AllyUnits.Remove(unit);
                AllyVisionComponents.Remove(visionComponent);
            } else {
                EnemyUnits.Remove(unit);
                EnemyVisionComponents.Remove(visionComponent);
            }
        }

        /// <summary>
        /// The registry needs to know which team the locally
        /// playing player is on, as some unit lists are split
        /// on an ally/enemy basis.
        /// </summary>
        /// <param name="newTeam"></param>
        public void UpdateTeamBelonging(Team newTeam)
        {
            if (_localTeam == newTeam)
                return;

            _localTeam = newTeam;

            // Refresh ally/enemy-based lists:
            AllyUnits.Clear();
            EnemyUnits.Clear();

            AllyVisionComponents.Clear();
            EnemyVisionComponents.Clear();

            foreach (UnitDispatcher unit in Units) {
                if (_localTeam == unit.Platoon.Owner.Team) {
                    AllyUnits.Add(unit);
                    AllyVisionComponents.Add(unit.VisionComponent);
                } else {
                    EnemyUnits.Add(unit);
                    EnemyVisionComponents.Add(unit.VisionComponent);
                }
            }
        }
    }
}
