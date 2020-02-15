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

using System.Collections.Generic;

using UnityEngine;

using PFW.Units.Component.Data;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Damage;
using PFW.Units.Component.Weapon;
using PFW.Units.Component.Health;
using static PFW.Units.Component.Damage.DamageData;

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
        public ArmorAttributes[] ArmorData = new ArmorAttributes[4];

        public void Initialize(
                HealthComponent healthComponent, 
                DataComponent data, 
                MovementComponent movement)
        {
            ArmorData = data.ArmorData;
            _unit = movement;
            _healthComponent = healthComponent;
        }

        /// <summary>
        /// Calculate the total damage dealt within a successful hit, then update health, armor and ERA values accordingly
        /// </summary>
        public void HandleHit(
            List<WeaponData.WeaponDamage> receivedDamage,
            Vector3? displacementToThis,
            float? distanceToCentre)
        {
            Logger.LogDamage($"ArmorComponent::HandleHit() called");
            ArmorAttributes armorOfImpact = new ArmorAttributes();
            int armorIndex;

            if (displacementToThis == null)
            {
                armorOfImpact = DetermineSideOfImpact();
                armorIndex = 4;
            }
            else
            {
                armorOfImpact = DetermineSideOfImpact(
                    displacementToThis.GetValueOrDefault(),
                    out armorIndex);
            }

            DamageData.Target unitAsTarget = ConstructTargetStruct(armorOfImpact);

            // Calculate its damage using its damage type
            foreach (WeaponData.WeaponDamage damageInstance in receivedDamage) {

                switch (damageInstance.DamageType) {
                    case DamageType.KE:
                        KEDamage keDamage = new KEDamage(
                                new KineticData(damageInstance),
                                unitAsTarget,
                                displacementToThis.GetValueOrDefault().magnitude
                        );
                        unitAsTarget = keDamage.CalculateDamage();
                        break;
                    case DamageType.HEAT:
                        HeatDamage heatDamage = new HeatDamage(
                                new HeatData(damageInstance),
                                unitAsTarget);
                        unitAsTarget = heatDamage.CalculateDamage();
                        break;
                    case DamageType.HE:
                        HEDamage heDamage = new HEDamage(
                                new HEData(damageInstance),
                                unitAsTarget, distanceToCentre.GetValueOrDefault()
                        );
                        unitAsTarget = heDamage.CalculateDamage();
                        break;
                    case DamageType.FIRE:
                        FireDamage fireDamage = new FireDamage(
                                new FireData(damageInstance),
                                unitAsTarget);
                        unitAsTarget = fireDamage.CalculateDamage();
                        break;
                    case DamageType.SMALLARMS:
                        SmallArmsDamage lightarmsDamage = new SmallArmsDamage(
                                new SmallArmsData(damageInstance),
                                unitAsTarget);
                        break;
                    default:
                        Debug.LogError("Not a valid damage type!");
                        break;
                }

                _healthComponent.UpdateHealth(unitAsTarget.Health);
                ArmorData[armorIndex].Armor = unitAsTarget.Armor;
                ArmorData[armorIndex].EraData = unitAsTarget.EraData;
            }
        }

        /// <summary>
        /// Use the displacement vector to calculate the angle of the shot
        /// </summary>
        public ArmorAttributes DetermineSideOfImpact(Vector3 displacementToThis, out int index)
        {
            index = 0;
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
                    index = 0;
                    break;
                case float n when (n > 45f && n <= 135f):
                    index = 1;
                    break;
                case float n when (n > 135f && n <= 180f):
                    index = 2;
                    break;
            }

            return ArmorData[index];
        }

        public ArmorAttributes DetermineSideOfImpact()
        {
            // When no displacement vector is supplied, the damage is dealt to the top armor
            return ArmorData[4];
        }

        /// <summary>
        /// Use this data of the armor being hit to construct a Target struct for the Damage classes to use as input
        /// </summary>
        public DamageData.Target ConstructTargetStruct(ArmorAttributes armorOfImpact)
        {
            DamageData.Target target = new DamageData.Target();

            target.Armor = armorOfImpact.Armor;
            target.EraData = armorOfImpact.EraData;

            target.Health = _healthComponent.Health;

            return target;
        }
    }
}
