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

using PFW.Units;

namespace PFW.Model.Armory
{
    /**
     * Each deck creates its own unit objects.
     * 
     * Warning: Currently, units don't have a deck ID. We only look up a unit
     * by id when it is created, and at that point in time we have access to the 
     * player who created it (and thus the deck). If we need to create a unit after
     * the creator has been removed (e.g. disconnect), we will need a more refined solution
     * that preserves the link to the deck (or a global unit id).
     */ 
    //[Serializable]
    public class Unit
    {
        // Identifies which category the unit is in.
        public byte CategoryId;
        // Unique for a deck+category pair, 
        // should match the index in the unit list.
        public int Id;

        public string Name { get; }
        public int Price { get; }

        private readonly UnitConfig _config;

        //[Tooltip("The gameobject this will be cloned from.")]
        public GameObject Prefab { get; }

        public Unit(UnitConfig config)
        {
            Name = config.Name;
            Price = config.Price;
            Prefab = Resources.Load<GameObject>(config.PrefabPath);
            _config = config;
        }

        /// <summary>
        ///     After a unit is spawned in a basic state that mirror
        ///     can transmit, augment it with 'art'
        ///     (aka non-networking components) based on the config.
        ///     Assumption: The config is the same for all clients.
        /// </summary>
        public void Augment(GameObject gameObject, bool isGhost) =>
                UnitFitter.Augment(gameObject, _config, isGhost);
    }
}
