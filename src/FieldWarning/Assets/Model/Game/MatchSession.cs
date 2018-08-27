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

namespace PFW.Model.Game
{
    public class MatchSession : MonoBehaviour
    {
        [NonSerialized]
        public Player LocalPlayer;

        public Settings Settings { get; } = new Settings();
        public ICollection<Team> Teams { get; } = new List<Team>();
        public ICollection<UnitBehaviour> AllUnits { get; } = new List<UnitBehaviour>();
        public ICollection<PlatoonBehaviour> AllPlatoons { get; } = new List<PlatoonBehaviour>();

        // rip encapsulation:
        public InputManager UIManager;
        public SelectionManager SelectionManager;

        private VisibilityManager _visibilityManager;

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


            UIManager = FindObjectOfType<InputManager>();
            if (UIManager == null)
                UIManager = gameObject.AddComponent<InputManager>();

            if (UIManager.Session == null)
                UIManager.Session = this;



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
        }

        public void RegisterUnitDeath(UnitBehaviour unit)
        {
            AllUnits.Remove(unit);
        }
    }
}
