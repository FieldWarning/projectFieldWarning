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
using PFW.Model.Armory;
using UnityEngine;

namespace PFW.Service
{
    public class FactionService : MonoBehaviour, IFactionService
    {
        private ICollection<Faction> _factions;
        private ICollection<Coalition> _coalitions;

        public void Awake()
        {
            _factions = new List<Faction>();
            _coalitions = new List<Coalition>();

            var nato = new Faction() { Name = "NATO", Color = "Blue" };
            var wapa = new Faction() { Name = "Warsaw Pact", Color = "Red" };

            _factions.Add(nato);
            _factions.Add(wapa);

            _coalitions.Add(new Coalition() { Name = "USA", Faction = nato });
            _coalitions.Add(new Coalition() { Name = "USSR", Faction = wapa });
        }

        public ICollection<Faction> All()
        {
            return _factions;
        }

        public ICollection<Coalition> AllCoalitions()
        {
            return _coalitions;
        }

        public ICollection<Coalition> AllByFaction(Faction faction)
        {
            return _coalitions.Where(c => c.Faction == faction).ToList();
        }
    }
}

