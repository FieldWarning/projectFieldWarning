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
using PFW.Units.Component.Damage;

namespace PFW.Units.Component.Weapon
{
    [Serializable]
    public class WeaponData
    {
        public float FireRange { get; set; } = 4000;
        public float ReloadTime { get; set; } = 10;
		public float Accuracy { get; set; } = 40;
        public WeaponDamage Damage { get; private set; }

        /// <summary>
        /// A set of damage data structs representing the power of the weapon
        /// and the type of the damage it deals
        /// </summary>
        public struct WeaponDamage
        {
            // TODO: add weapon with multiple types of damage
            public DamageData.KineticData? KineticData;
            public DamageData.HeatData? HeatData;
            public DamageData.HEData? HEData;
            public DamageData.FireData? FireData;
            public DamageData.SmallarmsData? LightarmsData;
            public DamageTypes DamageType;
        }

        public WeaponData(
            float fireRange, float reloadTime, float accuracy, WeaponDamage damage)
        {
            FireRange = fireRange;
            ReloadTime = reloadTime;
            Accuracy = accuracy;
            Damage = damage;
        }
    }
}

