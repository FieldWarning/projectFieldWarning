using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Damage
{
    class KEDamage:Damage
    {
        public struct KineticData
        {
            /// <summary>
            /// The power of the shot
            /// </summary>
            public float Power;
            /// <summary>
            /// Distance between the firing unit and target unit
            /// </summary>
            public float Distance;
            /// <summary>
            /// Multiplier for armor degradation
            /// </summary>
            public float Degradation;
            /// <summary>
            /// Multiplier for health damage
            /// </summary>
            public float HealthDamageFactor;
            /// <summary>
            /// Air friction constant used in calculation of attenuation
            /// </summary>
            public float Friction;
        }

        private KineticData _keData;

        public KEDamage(KineticData data, Target target)
            : base(DamageTypes.KE, target)
        {
            _keData = data;
        }

        public override Target CalculateDamage()
        {
            Target finalState = this.CurrentTarget;
            KineticData ke = _keData;

            // Calculate attenuation of air friction
            ke.Power = CalculateKEAttenuationSimple(
                ke.Power,
                ke.Distance,
                ke.Friction
            );

            if (finalState.EraData.Value > 0.0f) {
                // Calculate effects of ERA
                float finalEra = Math.Max(
                    0.0f,
                    finalState.EraData.Value - ke.Power * finalState.EraData.KEFractionMultiplier
                );
                finalState.EraData.Value = finalEra;

                ke.Power = CalculatePostEraPower(
                    ke.Power,
                    finalState.EraData.KEFractionMultiplier
                );
            }

            // Armor degradation
            float finalArmor = Math.Max(
                0.0f,
                finalState.Armor - (ke.Power / finalState.Armor) * ke.Degradation
            );
            finalState.Armor = finalArmor;

            // Calculate final damage
            float finalDamage = Math.Max(
                0.0f,
                (ke.Power - finalState.Armor) * ke.HealthDamageFactor
            );
            float finalHealth = Math.Max(
                0.0f,
                finalState.Health - finalDamage
            );
            finalState.Health = finalHealth;
            
            return finalState;
        }


        private static float CalculateKEAttenuationSimple(float power, float distance, float friction)
        {
            return  (float)Math.Exp(-friction * distance) * power;
        }
        
        private static float CalculatePostEraPower(float power, float eraFractionMultiplier)
        {
            return power * (1 - eraFractionMultiplier);
        }
    }
}