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
using PFW.Ingame.Prototype;
using PFW.Ingame.UI;
using PFW.Units;
using PFW.Units.Component.Vision;
using UnityEngine;

namespace PFW.Model.Game
{
    /**
     * Represents the ongoing match.
     *
     * Holds a lot of data that would be singleton or global, but is intentionally
     * non-static (so that we can easily clean up).
     */
    public class MatchSession : MonoBehaviour
    {
        private InputManager _inputManager;
        private VisibilityManager _visibilityManager;

        public List<VisionComponent> AllyVisibleBehaviours {
            get {
                return _visibilityManager.AllyUnits;
            }
        }
        public List<VisionComponent> EnemyVisibleBehaviours {
            get {
                return _visibilityManager.EnemyUnits;
            }
        }

        public Settings Settings { get; set; }

        public PlayerBehaviour LocalPlayer { get; private set; }

        /* TODO: I think all entities that need a global list should keep one
         * of their own, to minimize shared state. Instead of using these
         * lists, supply a unit registration call and have MatchSession call
         * that in RegisterUnitBirth() (see VisibilityManager for an example): */
        public List<Team> Teams { get; } = new List<Team>();
        public ICollection<UnitDispatcher> Units { get; } = new List<UnitDispatcher>();
        public ICollection<PlatoonBehaviour> Platoons { get; } = new List<PlatoonBehaviour>();

        public PathfinderData PathData { get; private set; }

        public UnitFactory Factory { get; private set; }

        public void Awake()
        {
            var blueTeam = GameObject.Find("Team_Blue").GetComponent<Team>();
            var redTeam = GameObject.Find("Team_Red").GetComponent<Team>();

            blueTeam.AddPlayer(this);
            redTeam.AddPlayer(this);

            Teams.Add(blueTeam);
            Teams.Add(redTeam);

            LocalPlayer = gameObject.AddComponent<PlayerBehaviour>();
            LocalPlayer.Data = redTeam.Players[0];

            GameObject.Find("Managers").GetComponent<DeploymentMenu>().LocalPlayer = LocalPlayer;

            _inputManager = FindObjectOfType<InputManager>() ??
                     gameObject.AddComponent<InputManager>();

            _visibilityManager = FindObjectOfType<VisibilityManager>() ??
                     gameObject.AddComponent<VisibilityManager>();

            _inputManager.Session = _inputManager.Session ?? this;
            _visibilityManager.LocalTeam = _visibilityManager.LocalTeam ?? LocalPlayer.Data.Team;

            // TODO: Pass terrain from future location of starting matches (no Find)
            PathData = new PathfinderData(GameObject.Find("Terrain").GetComponent<Terrain>());
            Factory = new UnitFactory(this);
            Settings = new Settings();
        }

        public void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            Platoons.Add(platoon);
            _inputManager.RegisterPlatoonBirth(platoon);
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            Platoons.Remove(platoon);
            _inputManager.RegisterPlatoonDeath(platoon);
        }

        public void RegisterUnitBirth(UnitDispatcher unit)
        {
            Units.Add(unit);
            _visibilityManager.RegisterUnitBirth(unit);
        }

        public void RegisterUnitDeath(UnitDispatcher unit)
        {
            Units.Remove(unit);
            _visibilityManager.RegisterUnitDeath(unit);
        }

        // TODO If we can refactor MatchSession to create the spawn points, we will be able to get rid of this:
        public void AddSpawnPoint(SpawnPointBehaviour spawn)
        {
            _inputManager.AddSpawnPoint(spawn);
        }

        public void UpdateTeamBelonging(Team newTeam)
        {
            _visibilityManager.UpdateTeamBelonging(newTeam);
        }
    }
}
