using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Damage
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