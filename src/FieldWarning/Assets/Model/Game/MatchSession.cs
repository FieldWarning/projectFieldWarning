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
using PFW.Ingame.UI;

namespace PFW.Model.Game
{
    public class MatchSession : MonoBehaviour
    {
        public Settings Settings { get; } = new Settings();
        public ICollection<Team> Teams { get; private set; }
        public ICollection<UnitBehaviour> AllUnits { get; private set; }
        public ICollection<PlatoonBehaviour> AllPlatoons { get; private set; }

        // rip encapsulation:
        public UIManagerBehaviour UIManager;
        public SelectionManager SelectionManager;

        public void Awake()
        {
            UIManager = FindObjectOfType<UIManagerBehaviour>();
            if (UIManager == null)
                UIManager = gameObject.AddComponent<UIManagerBehaviour>();

            if (UIManager.Session == null)
                UIManager.Session = this;

            // TODO lazy hack, fix:
            UIManager.Owner = GameObject.Find("RedPlayer1").GetComponent<Player>();

            SelectionManager = FindObjectOfType<SelectionManager>();
            if (SelectionManager == null)
                SelectionManager = gameObject.AddComponent<SelectionManager>();

            if (SelectionManager.Session == null)
                SelectionManager.Session = this;

            Teams = new List<Team>();

            AllUnits = new List<UnitBehaviour>();
            AllPlatoons = new List<PlatoonBehaviour>();
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
    }
}
