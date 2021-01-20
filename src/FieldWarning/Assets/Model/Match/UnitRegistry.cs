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

using UnityEngine;
using System.Collections.Generic;

using PFW.Units;
using PFW.Units.Component.Vision;

namespace PFW.Model.Match
{
    /// <summary>
    /// Holds various lists of the units existing in a match.
    /// </summary>
    public class UnitRegistry
    {
        private Team _localTeam;

        /// <summary>
        /// List of all active units in a match.
        /// </summary>
        public List<UnitDispatcher> Units { get; } =
                new List<UnitDispatcher>();

        /// <summary>
        /// List of all units that are team members
        /// from the perspective of a given team.
        /// </summary>
        public Dictionary<Team, List<UnitDispatcher>> UnitsByTeam { get; } =
                  new Dictionary<Team, List<UnitDispatcher>>();

        /// <summary>
        /// List of all units that are enemies
        /// from the perspective of a given team
        /// </summary>
        public Dictionary<Team, List<UnitDispatcher>> EnemiesByTeam { get; } =
                  new Dictionary<Team, List<UnitDispatcher>>();

        /// <summary>
        /// List of all units that are team members
        /// from the perspective of the local player.
        /// </summary>
        public List<UnitDispatcher> AllyUnits { get; private set; }

        /// <summary>
        /// List of all units that are enemies
        /// from the perspective of the local player.
        /// </summary>
        public List<UnitDispatcher> EnemyUnits { get; private set; }

        public List<VisionComponent> AllyVisionComponents { get; } =
                new List<VisionComponent>();
        public List<VisionComponent> EnemyVisionComponents { get; } =
                new List<VisionComponent>();

        public UnitRegistry(Team localTeam, List<Team> teams)
        {
            _localTeam = localTeam;
            teams.ForEach(
                team => UnitsByTeam.Add(team, new List<UnitDispatcher>()));
            teams.ForEach(
                team => EnemiesByTeam.Add(team, new List<UnitDispatcher>()));

            AllyUnits = UnitsByTeam[_localTeam];
            EnemyUnits = EnemiesByTeam[_localTeam];
        }

        /// <summary>
        /// Must notify the registry every time a
        /// (real, active) unit is created.
        /// <para></para>
        /// Do not call outside of MatchSession.
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterUnitBirth(UnitDispatcher unit)
        {
            Units.Add(unit);

            Team unitTeam = unit.Platoon.Team;
            UnitsByTeam[unitTeam].Add(unit);

            // Add unit as enemy to all other teams:
            foreach (var pair in EnemiesByTeam)
                if (pair.Key != unitTeam)
                    pair.Value.Add(unit);

            VisionComponent visibleBehavior = unit.VisionComponent;
            if (unitTeam == _localTeam) {
                AllyVisionComponents.Add(visibleBehavior);
            } else {
                EnemyVisionComponents.Add(visibleBehavior);
            }
        }

        /// <summary>
        /// Must notify the registry every time a
        /// (real, active) unit is removed.
        /// <para></para>
        /// Do not call outside of MatchSession.
        /// </summary>
        /// <param name="unit"></param>
        public void RegisterUnitDeath(UnitDispatcher unit)
        {
            Units.Remove(unit);

            Team unitTeam = unit.Platoon.Team;
            UnitsByTeam[unitTeam].Remove(unit);
            foreach (var pair in EnemiesByTeam)
                if (pair.Key != unitTeam)
                    pair.Value.Remove(unit);

            VisionComponent visionComponent = unit.VisionComponent;
            if (unitTeam == _localTeam) {
                AllyVisionComponents.Remove(visionComponent);
            } else {
                EnemyVisionComponents.Remove(visionComponent);
            }
        }

        /// <summary>
        /// The registry needs to know which team the locally
        /// playing player is on, as some unit lists are split
        /// on an ally/enemy basis.
        /// <para></para>
        /// Do not call outside of MatchSession.
        /// </summary>
        /// <param name="newTeam"></param>
        public void UpdateTeamBelonging(Team newTeam)
        {
            if (_localTeam == newTeam)
                return;

            _localTeam = newTeam;

            // Refresh ally/enemy-based lists:
            AllyUnits = UnitsByTeam[_localTeam];
            EnemyUnits = EnemiesByTeam[_localTeam];

            AllyVisionComponents.Clear();
            EnemyVisionComponents.Clear();

            foreach (UnitDispatcher unit in Units) {
                if (_localTeam == unit.Platoon.Team) {
                    AllyVisionComponents.Add(unit.VisionComponent);
                } else {
                    EnemyVisionComponents.Add(unit.VisionComponent);
                }
            }
        }

        public List<UnitDispatcher> FindUnitsAroundPoint(
                Vector3 point, float radius)
        {
            List<UnitDispatcher> result = FindAlliesAroundPoint(point, radius);
            result.AddRange(FindEnemiesAroundPoint(point, radius));
            return result;
        }

        public List<UnitDispatcher> FindEnemiesAroundPoint(
                Vector3 point, float radius)
        {
            return FindUnitsAroundPoint(point, radius, EnemyUnits);
        }
        public List<UnitDispatcher> FindAlliesAroundPoint(
                Vector3 point, float radius)
        {
            return FindUnitsAroundPoint(point, radius, AllyUnits);
        }

        private List<UnitDispatcher> FindUnitsAroundPoint(
                Vector3 point, float radius, List<UnitDispatcher> searchSet)
        {
            List<UnitDispatcher> result = new List<UnitDispatcher>();
            foreach (UnitDispatcher unit in searchSet)
            {
                float distance = Vector3.Distance(point, unit.transform.position);
                if (radius > distance)
                {
                    result.Add(unit);
                }
            }
            return result;
        }
    }
}
