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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Damage;
using PFW.Units.Component.Weapon;
using PFW.Units.Component.Health;


namespace PFW.Units.Component.Armor
{
    class ArmorComponent
    {
        /// <summary>
        /// The movement component where we obtain the front and right vectors
        /// used in deciding which face of the armor is hit
        /// </summary>
        public MovementComponent Unit { get; private set; }
        private PlatoonBehaviour _platoon;
        private HealthComponent _healthComponent;
        
        

        /// <summary>
        /// An array of ArmorAttributes to store armor values 
        /// on front / side / rear/ top respectively
        /// </summary>
        public UnitData.ArmorAttributes[] ArmorData = new UnitData.ArmorAttributes[4];

        public ArmorComponent(
            MovementComponent unit,
            PlatoonBehaviour platoon,
            UnitData.ArmorAttributes[] armorData)
        {
            Unit = unit;
            _platoon = platoon;
            ArmorData = armorData;
        }

        /// <summary>
        /// Calculate the total damage dealt within a successful hit, then update health, armor and ERA values accordingly
        /// </summary>
        public void HandleHit(WeaponData.WeaponDamage receivedDamage, Vector3? displacementToThis, float? distanceToCentre)
        {
            UnitData.ArmorAttributes armorOfImpact = new UnitData.ArmorAttributes();
            int armorIndex;

            if (displacementToThis == null)
            {
                armorOfImpact = DetermineSideOfImpact();
                armorIndex = 4;
            }
            else
            {
                armorOfImpact = DetermineSideOfImpact(displacementToThis.GetValueOrDefault(), out armorIndex);
            }

            DamageData.Target unitAsTarget = ConstructTargetStruct(armorOfImpact);

            DamageData.Target finalState = unitAsTarget;

            // Calculate its damage using its damage type
            switch (receivedDamage.DamageType)
            {
                case DamageTypes.KE:
                    KEDamage keDamage = new KEDamage(
                            receivedDamage.KineticData.GetValueOrDefault(),
                            unitAsTarget,
                            displacementToThis.GetValueOrDefault().magnitude
                    );
                    finalState = keDamage.CalculateDamage();
                    break;
                case DamageTypes.HEAT:
                    HeatDamage heatDamage = new HeatDamage(
                            receivedDamage.HeatData.GetValueOrDefault(),
                            unitAsTarget);
                    finalState = heatDamage.CalculateDamage();
                    break;
                case DamageTypes.HE:
                    HEDamage heDamage = new HEDamage(
                            receivedDamage.HEData.GetValueOrDefault(),
                            unitAsTarget, distanceToCentre.GetValueOrDefault()
                    );
                    finalState = heDamage.CalculateDamage();
                    break;
                case DamageTypes.FIRE:
                    FireDamage fireDamage = new FireDamage(
                            receivedDamage.FireData.GetValueOrDefault(),
                            unitAsTarget);
                    finalState = fireDamage.CalculateDamage();
                    break;
                case DamageTypes.SMALLARMS:
                    SmallarmsDamage lightarmsDamage = new SmallarmsDamage(
                            receivedDamage.LightarmsData.GetValueOrDefault(),
                            unitAsTarget);
                    break;
                default:
                    Debug.LogError("Not a valid damage type!");
                    break;
            }

            _healthComponent.UpdateHealth(finalState.Health);
            ArmorData[armorIndex].Armor = finalState.Armor;
            ArmorData[armorIndex].EraData = finalState.EraData;
        }

        /// <summary>
        /// Use the displacement vector to calculate the angle of the shot
        /// </summary>
        public UnitData.ArmorAttributes DetermineSideOfImpact(Vector3 displacementToThis, out int index)
        {
            index = 0;
            Vector3 displacementToFiringUnit = -displacementToThis;

            // Project this vector to the horizontal plane of the unit
            Vector3 planeNormal = Vector3.Cross(Unit.Forward, Unit.Right);
            Vector3 incomingFire = Vector3.ProjectOnPlane(displacementToFiringUnit, planeNormal);

            // Calculate the angle, since Unity always return a positive value, we have
            // 0 to 45 deg => front
            // 45 to 135 deg => side
            // 135 to 180 deg => rear
            float shotAngle = Vector3.Angle(Unit.Forward, incomingFire);

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
        
        public UnitData.ArmorAttributes DetermineSideOfImpact()
        {
            // When no displacement vector is supplied, the damage is dealt to the top armor
            return ArmorData[4];
        }

        /// <summary>
        /// Use this data of the armor being hit to construct a Target struct for the Damage classes to use as input
        /// </summary>
        public DamageData.Target ConstructTargetStruct(UnitData.ArmorAttributes armorOfImpact)
        {
            DamageData.Target target = new DamageData.Target();
            
            target.Armor = armorOfImpact.Armor;
            target.EraData = armorOfImpact.EraData;

            target.Health = _healthComponent.Health;

            return target;
        }
    }
}
