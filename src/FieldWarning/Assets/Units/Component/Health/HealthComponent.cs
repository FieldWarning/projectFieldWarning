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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PFW.Units.Component.Health
{
    public class HealthComponent
    {
        public float Health { get; private set; }
        private PlatoonBehaviour _platoon;
        private GameObject _gameObject;
        private UnitDispatcher _dispatcher;

        public HealthComponent(
            float maxHealth,
            PlatoonBehaviour platoon,
            GameObject gameObject,
            UnitDispatcher dispatcher)
        {
            Health = maxHealth;
            _platoon = platoon;
            _gameObject = gameObject;
            _dispatcher = dispatcher;
        }

        public void HandleHit(float receivedDamage)
        {
            if (Health <= 0)
                return;

            Health -= receivedDamage;
            if (Health <= 0)
                Destroy();
        }

        public void Destroy()
        {
            _platoon.Owner.Session.RegisterUnitDeath(_dispatcher);

            _platoon.Units.Remove(_dispatcher);
            GameObject.Destroy(_gameObject);

            _platoon.GhostPlatoon.HandleRealUnitDestroyed();

            if (_platoon.Units.Count == 0) {
                GameObject.Destroy(_platoon.gameObject);
                _platoon.Owner.Session.RegisterPlatoonDeath(_platoon);
            }
        }
    }
}
