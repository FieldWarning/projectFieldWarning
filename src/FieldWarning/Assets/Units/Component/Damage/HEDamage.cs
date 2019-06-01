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
    class HEDamage : Damage
    {
        private DamageData.HEData _heData;
        private float _distanceToCentre;

        /// <summary>
        /// The cutoff fraction for HE damage splash
        /// Read documentation {4.2 HE dropoff within AOE} for details
        /// </summary>
        private const float CUTOFF_FRACTION = 0.01f; // Relocate this constant elsewhere when integrating

        /// <summary>
        /// The lookup table that decides how much, in fraction of original value, the HE damage will be taken by a certain armor value
        /// </summary>
        private readonly Dictionary<int, float> ArmorToHEMultiplier = new Dictionary<int, float>
        {
            {0, 1.0f},{1, 0.5f},{2, 0.25f},{3, 0.1f}
        };

        private const int ARMOR_CUTOFF = 4;

        public HEDamage(DamageData.HEData data, DamageData.Target target, float distanceToCentre) : base(DamageTypes.HE, target)
        {
            _heData = data;
            _distanceToCentre = distanceToCentre;
        }

        public override DamageData.Target CalculateDamage()
        {
            DamageData.Target finalState = this.CurrentTarget;
            DamageData.HEData he = _heData;
            he.Power = CalculatedPowerDropoff(he.Power, he.EffectiveRadius, _distanceToCentre);

            // Does not consider ERA
            if (CurrentTarget.Armor >= ARMOR_CUTOFF)
            {
                // The HE round has no effect on such a heavily armored target
                return finalState;
            }
            else
            {
                // Lookup multiplier from the table
                float armorMultiplier;
                bool lookupSuccessful = ArmorToHEMultiplier.TryGetValue((int)CurrentTarget.Armor, out armorMultiplier);
                if (lookupSuccessful)
                {
                    he.Power = he.Power * armorMultiplier;
                    finalState.Health -= he.Power * he.HealthDamageFactor;
                    return finalState;
                }
                else
                {
                    throw new Exception("target armor value not in lookup table");
                }
            }
        }

        private float CalculatedPowerDropoff(float power, float radius, float distance)
        {
            if (distance > radius)
            {
                return 0.0f;
            }
            else
            {
                if (distance > 0)
                {
                    float fractionRemain = (float)(power / (4 * Math.PI * distance * distance));

                    float finalPower = Math.Min(1.0f, fractionRemain) * power;

                    return finalPower;
                }
                else if (distance == 0)
                {
                    // Direct hit, use full HE power
                    return power;
                }
                else
                {
                    // distance < 0, there must be a bug
                    throw new Exception("Distance to centre must be non-negative");
                }
            }
        }
    }
}
