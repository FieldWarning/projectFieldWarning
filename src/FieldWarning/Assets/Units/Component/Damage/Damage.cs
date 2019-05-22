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
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Units.Component.Damage
{
    /// <summary>
    /// The base class Damage, on which all damage classes are constructed
    /// This class represents a damage dealt in an update, for one-time instant damage (e.g. KEDamage or HeatDamage), the class will be instantiated once
    /// For continuous damage (e.g. FireDamage), the class will be instantiated once and the CalculateDamage will be called at every update (controlled by another script inherenting MonoBehaviour)
    /// </summary>
    internal abstract class Damage
    {
        /// <summary>
        /// Damage type always take a Target struct representing the initial state of the target unit
        /// </summary>
        /// <param name="damageType"></param>
        /// <param name="currentTarget"></param>
        protected Damage(DamageTypes damageType, Target currentTarget)
        {
            this.DamageType = damageType;
            this.CurrentTarget = currentTarget;
        }

        /// <Summary>
        ///  A struct containing the data of the target of the damage
        /// </Summary>
        public Target CurrentTarget { get; private set; }

        /// <Summary>
        ///  The type of the damage, indicated by a enum
        /// </Summary>
        public DamageTypes DamageType { get; private set; }

        /// <Summary>
        /// Use this method to calculate damage.
        /// Override this method in the child classes
        /// </Summary>
        public virtual Target CalculateDamage()
        {
            // Override this function to specify damage algorithm
            return this.CurrentTarget; // No damage dealt thus return the original state of the target
        }

        public struct Era
        {
            public float Value;
            public float KEFractionMultiplier;
            public float HeatFractionMultiplier;
        }

        public struct Target
        {
            public float Armor;
            public Era EraData;
            public float Health;
        }
    }

    public enum DamageTypes
    {
        /// <summary>
        /// Kinetic energy
        /// </summary>
        KE,
        /// <summary>
        /// High-explosive anti-tank
        /// </summary>
        HEAT,
        /// <summary>
        /// High-explosive none shaped-charge
        /// </summary>
        HE,
        /// <summary>
        /// Napalm burn damage
        /// </summary>
        FIRE,
        /// <summary>
        /// Laser
        /// </summary>
        LASER,
        /// <summary>
        /// Light arms damage
        /// </summary>
        LIGHTARMS
    }
}