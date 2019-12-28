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

using PFW.Model.Game;
using PFW.Units.Component.Data;
using PFW.Units.Component.Weapon;

namespace PFW.Units.Component.Health
{
    public class HealthComponent : MonoBehaviour
    {
        public float Health { get; private set; }
        private PlatoonBehaviour _platoon;
        private UnitDispatcher _dispatcher;
        private TargetTuple _targetTuple;

        private void Awake()
        {
            Health = gameObject.GetComponent<DataComponent>().MaxHealth;
        }

        public void Initialize(UnitDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _platoon = gameObject.GetComponent<SelectableBehavior>().Platoon;
            _targetTuple = dispatcher.TargetTuple;
        }

        public void UpdateHealth(float newHealth)
        {
            if (newHealth <= 0)
                Destroy();
            else
                Health = newHealth;
        }

        public void Destroy()
        {
            _targetTuple.Reset();

            MatchSession.Current.RegisterUnitDeath(_dispatcher);

            _platoon.Units.Remove(_dispatcher);
            GameObject.Destroy(gameObject);

            _platoon.GhostPlatoon.RemoveOneGhostUnit();

            if (_platoon.Units.Count == 0) {
                GameObject.Destroy(_platoon.gameObject);
                MatchSession.Current.RegisterPlatoonDeath(_platoon);
            }
        }
    }
}
