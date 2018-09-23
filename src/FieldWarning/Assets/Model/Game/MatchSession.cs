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
using System.Collections.Generic;
using UnityEngine;
using PFW.Ingame.UI;

using static PFW.Ingame.UI.InputManager;
using ECS;
using Unity.Entities;
using PFW.Ingame.Prototype;

namespace PFW.Model.Game
{
    public class MatchSession : MonoBehaviour
    {
        [NonSerialized]
        public Player LocalPlayer;

        public Settings Settings { get; } = new Settings();
        public ICollection<Team> Teams { get; } = new List<Team>();

        // TODO: I think all entities that need a global list should keep one
        // of their own, to minimize shared state. Instead of using these 
        // lists, supply a unit registration call and have MatchSession call
        // that in RegisterUnitBirth() (see VisibilityManager for an example):
        public ICollection<UnitBehaviour> AllUnits { get; } = new List<UnitBehaviour>();
        public ICollection<PlatoonBehaviour> AllPlatoons { get; } = new List<PlatoonBehaviour>();

        private InputManager _inputManager;
        public MouseMode CurrentMouseMode {
            get { return _inputManager.CurMouseMode; }
        }

        public SelectionManager SelectionManager;

        private VisibilityManager _visibilityManager;

        public PathfinderData PathfinderData { get; private set; }

        public UnitFactory UnitFactory { get; private set; }
        public void Awake()
        {
            // TODO: I don't think we will want to customize the 
            // team colors etc to be map-specific. So it makes more sense
            // to have MatchSession create the team objects instead of 
            // dragging them into the scene like it works now.
            Team blueTeam = GameObject.Find("Team_Blue").GetComponent<Team>();
            Team redTeam = GameObject.Find("Team_Red").GetComponent<Team>();

            blueTeam.AddPlayer(this);
            redTeam.AddPlayer(this);

            Teams.Add(blueTeam);
            Teams.Add(redTeam);

            LocalPlayer = redTeam.Players[0];


            _inputManager = FindObjectOfType<InputManager>();
            if (_inputManager == null)
                _inputManager = gameObject.AddComponent<InputManager>();

            if (_inputManager.Session == null)
                _inputManager.Session = this;



            SelectionManager = FindObjectOfType<SelectionManager>();
            if (SelectionManager == null)
                SelectionManager = gameObject.AddComponent<SelectionManager>();

            if (SelectionManager.Session == null)
                SelectionManager.Session = this;


            _visibilityManager = FindObjectOfType<VisibilityManager>();
            if (_visibilityManager == null)
                _visibilityManager = gameObject.AddComponent<VisibilityManager>();

            if (_visibilityManager.Session == null)
                _visibilityManager.Session = this;
            if (_visibilityManager.LocalTeam == null)
                _visibilityManager.LocalTeam = LocalPlayer.Team;

            // TODO: pass the terrain from whatever code will be starting matches, instead of searching for it like this:
            PathfinderData = new PathfinderData(GameObject.Find("Terrain").GetComponent<Terrain>());
            UnitFactory = new UnitFactory(this);
        }


        public void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            AllPlatoons.Add(platoon);
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            AllPlatoons.Remove(platoon);
            SelectionManager.RegisterPlatoonDeath(platoon);
        }

        public void RegisterUnitBirth(UnitBehaviour unit)
        {
            AllUnits.Add(unit);
            _visibilityManager.RegisterUnitBirth(unit);
        }

        public void RegisterUnitDeath(UnitBehaviour unit)
        {
            AllUnits.Remove(unit);
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
