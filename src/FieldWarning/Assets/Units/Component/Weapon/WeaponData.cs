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

using PFW.Units.Component.Damage;

namespace PFW.Units.Component.Weapon
{
    [Serializable]
    public class WeaponData
    {
        public float FireRange = 4000;
        public float ReloadTime = 10;
        public float Accuracy = 40;
        public List<WeaponDamage> Damage;

        /// <summary>
        /// A set of damage data structs representing the power of the weapon
        /// and the type of the damage it deals
        /// </summary>
        [Serializable]
        public struct WeaponDamage
        {
            public DamageType DamageType;
            [Tooltip("The power of the shot, used by all damage types.")]
            public float Power;
            [Tooltip("How much armor the weapon strips off, used by all damage types except small arms.")]
            public float ArmorDegradation;
            [Tooltip("Multiplier for health damage?? TODO clarify. For KE, Fire, HEAT damage.")]
            public float HealthDamageFactor;

            [Tooltip("For fire damage.")]
            public float SuffocationDamage;

            [Tooltip("Air friction constant used in calculation of attenuation, for KE damage.")]
            public float AirFriction;

            [Tooltip("Explosion radius, for HE damage.")]
            public float EffectiveRadius;
        }
    }
}

