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

namespace PFW.Units.Component.Damage
{
    class HeatDamage:Damage
    {
        private DamageData.HeatData _heatData;

        public HeatDamage(DamageData.HeatData data, DamageData.Target target)
            : base(DamageTypes.HEAT, target)
        {
            _heatData = data;
        }

        public override DamageData.Target CalculateDamage()
        {
            DamageData.Target finalState = this.CurrentTarget;
            DamageData.HeatData heat = _heatData;

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