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
    class FireDamage : Damage
    {
        private DamageData.FireData _fireData;

        public FireDamage(DamageData.FireData data, Target target) : base(DamageTypes.FIRE, target)
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
