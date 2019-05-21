using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Damage
{
    class FireDamage : Damage
    {
        public struct FireData
        {
            /// <summary>
            /// Power of the burning effect
            /// </summary>
            public float Power;
            /// <summary>
            /// Multiplier for health damage
            /// </summary>
            public float HealthDamageFactor;
            /// <summary>
            /// Multiplier for armor degradation
            /// </summary>
            public float Degradation;
            /// <summary>
            /// A minimum damage dealt to vehicles in the fire
            /// Represents the damge to the crew by suffocation and heat
            /// </summary>
            public float SuffocationDamage;
        }

        private FireData _fireData;

        public FireDamage(FireData data, Target target) : base(DamageTypes.FIRE, target)
        {
            _fireData = data;
        }

        public override Target CalculateDamage()
        {
            Target finalState = this.CurrentTarget;

            // Armor degradation
            float finalArmor = Math.Max(
                0.0f,
                finalState.Armor - (_fireData.Power / finalState.Armor) * _fireData.Degradation
            );

            // Calculate damage
            // If the power is less than the armor, deal a minimum amount of damage
            // This represents the damage dealt to the crew due to high temperature and suffocation
            float finalDamage = Math.Max(
                _fireData.SuffocationDamage,
                (_fireData.Power - finalState.Armor) * _fireData.HealthDamageFactor
            );

            finalState.Health -= finalDamage;
            return finalState;
        }
    }
}
