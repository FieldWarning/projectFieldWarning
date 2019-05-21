using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PFW.Damage
{
    class LightarmsDamage : Damage
    {
        public struct LightArmsData
        {
            public float Power;
            public float HealthDamageFactor;
        }

        private LightArmsData _lightarmsData;

        public LightarmsDamage(LightArmsData data, Target target) : base(DamageTypes.LIGHTARMS, target)
        {
            _lightarmsData = data; 
        }

        public override Target CalculateDamage()
        {
            Target finalState = CurrentTarget;
            if (finalState.Armor == 0)
            {
                // Deal full damage to light targets / infantry targets
                finalState.Health -= _lightarmsData.Power * _lightarmsData.HealthDamageFactor;
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
