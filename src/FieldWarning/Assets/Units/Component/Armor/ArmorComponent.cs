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

using UnityEngine;

using Mirror;
using PFW.Units.Component.Data;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Health;
using PFW.Units.Component.Weapon;

namespace PFW.Units.Component.Armor
{
    sealed class ArmorComponent : MonoBehaviour
    {
        /// <summary>
        /// The movement component where we obtain the front and right vectors
        /// used in deciding which face of the armor is hit
        /// </summary>
        private MovementComponent _unit;
        private HealthComponent _healthComponent;

        /// <summary>
        /// An array of ArmorAttributes to store armor values
        /// on front / side / rear/ top respectively
        /// </summary>
        private DataComponent _data;

        public void Initialize(
                HealthComponent healthComponent, 
                DataComponent data, 
                MovementComponent movement)
        {
            _data = data;
            _unit = movement;
            _healthComponent = healthComponent;
        }

        /// <summary>
        /// Calculate the total damage dealt within a successful hit, then update health.
        /// </summary>
        public void HandleHit(
                DamageType damageType,
                float firepower,
                Vector3 displacementToThis,
                float distance)
        {
            float receivedDamage = EstimateDamage(
                    damageType, firepower, displacementToThis, distance);

            Logger.LogDamage(
                    LogLevel.INFO,
                    $"Received {receivedDamage} damage with type {damageType}," +
                    $"pre-calculation firepower was {firepower}");

            if (receivedDamage > 0)
            {
                _healthComponent.UpdateHealth(_healthComponent.Health - receivedDamage);
            }
        }

        /// <summary>
        ///     Calculate the damage that would be applied if
        ///     a hit with the given stats were to occur.
        /// </summary>
        public float EstimateDamage(
                DamageType damageType,
                float firepower,
                Vector3 displacement,
                float distance)
        {
            float result = 0;

            int armorOfImpact = DetermineSideOfImpact(displacement);

            switch (damageType)
            {
                case DamageType.HE:
                    result = HeDamage(firepower, armorOfImpact, distance);
                    break;
                case DamageType.HEAT:
                    result = HeatDamage(firepower, armorOfImpact);
                    break;
                case DamageType.KE:
                    result = KeDamage(firepower, armorOfImpact, distance);
                    break;
                case DamageType.SMALL_ARMS:
                    result = SmallArmsDamage(firepower, armorOfImpact);
                    break;
                case DamageType.HEAVY_ARMS:
                    result = HeavyArmsDamage(firepower, armorOfImpact);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Use the displacement vector to calculate the angle of the shot
        /// </summary>
        private int DetermineSideOfImpact(Vector3 displacementToThis)
        {
            int armor = _data.FrontArmor;
            Vector3 displacementToFiringUnit = -displacementToThis;

            // Project this vector to the horizontal plane of the unit
            Vector3 planeNormal = Vector3.Cross(_unit.Forward, _unit.Right);
            Vector3 incomingFire = Vector3.ProjectOnPlane(displacementToFiringUnit, planeNormal);

            // Calculate the angle, since Unity always return a positive value, we have
            // 0 to 45 deg => front
            // 45 to 135 deg => side
            // 135 to 180 deg => rear
            float shotAngle = Vector3.Angle(_unit.Forward, incomingFire);

            switch (shotAngle)
            {
                case float n when (n >= 0 && n <= 45f):
                    armor = _data.FrontArmor;
                    break;
                case float n when (n > 45f && n <= 135f):
                    armor = _data.SideArmor;
                    break;
                case float n when (n > 135f && n <= 180f):
                    armor = _data.RearArmor;
                    break;
            }

            return armor;
        }

        private float KeDamage(
                float firepower,
                int armor,
                float distance)
        {
            float result;

            // range scaling
            firepower -= (int)(distance / Constants.KE_FALLOFF);
            if (firepower < 0)
            {
                firepower = 0;
            }

            if (armor == 0)
            {
                result = 2 * firepower;
            }
            else if (armor == 1)
            {
                result = firepower;
            }
            else
            {
                result = ((firepower - armor) / 2) + 1;
            }

            return result;
        }
        private float HeatDamage(
                float firepower,
                int armor)
        {
            float result;

            if (armor > 20)
            {
                armor = 20;
            }

            if (armor == 0)
            {
                result = 2 * firepower;
            }
            else if (armor == 1)
            {
                result = firepower;
            }
            else
            {
                // Each point of firepower more than the armor value equals 0.5 damage,
                // but if we have 10 more firepower than armor, those points count double.
                float overkill = Mathf.Max(firepower - armor - 10, 0);
                result = (firepower + overkill - armor) / 2 + 1;
            }

            return result;
        }

        private float HeDamage(
                float firepower,
                int armor,
                float distance)
        {
            float result = firepower;

            // range scaling
            result -= (int)(distance / Constants.HE_FALLOFF);

            switch (armor)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    result *= 0.4f;
                    break;
                case 3:
                    result *= 0.3f;
                    break;
                case 4:
                    result *= 0.2f;
                    break;
                case 5:
                    result *= 0.15f;
                    break;
                case 6:
                case 7:
                    result *= 0.1f;
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    result *= 0.05f;
                    break;
                default:
                    result *= 0.01f;
                    break;
            }

            return result;
        }
        private float SmallArmsDamage(
                float firepower,
                int armor)
        {
            float result = 0;

            if (armor == 0)
            {
                result = firepower;
            }
            else if (armor == 1)
            {
                result = firepower * 0.1f;
            }

            return result;
        }
        private float HeavyArmsDamage(
                float firepower,
                int armor)
        {
            float result = 0;

            if (armor == 0)
            {
                result = firepower;
            }
            else if (armor == 1)
            {
                result = firepower * 0.5f;
            }

            return result;
        }
    }
}
