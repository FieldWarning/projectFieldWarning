using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PFW.Damage
{
    class HEDamage : Damage
    {
        public struct HEData
        {
            /// <summary>
            /// The power of the shot
            /// </summary>
            public float Power;
            /// <summary>
            /// The radius of the explosion
            /// Beyond the effective radius, the value {Remaining damage}/{Initial damage} is less than CUTOF_FFRACTION
            /// </summary>
            public float EffectiveRadius;
            /// <summary>
            /// Multiplier for health damage
            /// </summary>
            public float HealthDamageFactor;
        }

        private HEData _heData;
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

        public HEDamage(HEData data, Target target, float distanceToCentre) : base(DamageTypes.HE, target)
        {
            _heData = data;
            _distanceToCentre = distanceToCentre;
        }

        public override Target CalculateDamage()
        {
            Target finalState = this.CurrentTarget;
            HEData he = _heData;
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
                float finalPower = (float)(power / (4 * Math.PI * distance * distance));
                return finalPower;
            }
        }
    }
}
