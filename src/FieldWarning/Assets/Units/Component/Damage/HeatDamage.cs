using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Damage
{
    class HeatDamage:Damage
    {
        public struct HeatData
        {
            /// <summary>
            /// The power of the shot
            /// </summary>
            public float Power;
            /// <summary>
            /// Multiplier for armor degradation
            /// </summary>
            public float Degradation;
            /// <summary>
            /// Multiplier for health damage
            /// </summary>
            public float HealthDamageFactor;
        }

        private HeatData _heatData;

        public HeatDamage(HeatData data, Target target)
            : base(DamageTypes.HEAT, target)
        {
            _heatData = data;
        }

        public override Target CalculateDamage()
        {
            Target finalState = this.CurrentTarget;
            HeatData heat = _heatData;

            // No air friction attenuation as HEAT round detonates on surface of the armor

            if (finalState.EraData.Value > 0.0f) {
                // Calculate effects of ERA
                float finalEra = Math.Max(
                    0.0f,
                    finalState.EraData.Value - heat.Power * finalState.EraData.HeatFractionMultiplier
                );
                finalState.EraData.Value = finalEra;
                
                heat.Power = CalculatePostEraPower(
                    heat.Power,
                    finalState.EraData.HeatFractionMultiplier
                );
            }

            // Armor degradation
            float finalArmor = Math.Max(
                0.0f,
                finalState.Armor - (heat.Power / finalState.Armor) * heat.Degradation
            );
            finalState.Armor = finalArmor;

            // Calculate final damage
            float finalDamage = Math.Max(
                0.0f,
                (heat.Power - finalState.Armor) * heat.HealthDamageFactor
            );
            float finalHealth = Math.Max(
                0.0f,
                finalState.Health - finalDamage
            );
            finalState.Health = finalHealth;
            
            return finalState;
        }

        private static float CalculatePostEraPower(float power, float eraFractionMultiplier)
        {
            return power * (1 - eraFractionMultiplier);
        }
    }
}