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

namespace PFW.Units.Component.Damage
{
    class SmallArmsDamage : Damage
    {
        private DamageData.SmallArmsData _smallArmsData;

        public SmallArmsDamage(DamageData.SmallArmsData data, DamageData.Target target) : base(DamageType.SMALLARMS, target)
        {
            _smallArmsData = data;
        }

        public override DamageData.Target CalculateDamage()
        {
            DamageData.Target finalState = CurrentTarget;
            if (finalState.Armor == 0)
            {
                // Deal full damage to light targets / infantry targets
                finalState.Health -= _smallArmsData.Power * _smallArmsData.HealthDamageFactor;
                return finalState;
            }
            else
            {
                // No damage dealt
                return finalState;
            }
        }
    }
}
