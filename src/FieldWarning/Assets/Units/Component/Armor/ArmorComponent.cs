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

using PFW.Units.Component.Data;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Health;

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
        public int[] ArmorData = new int[4];

        public void Initialize(
                HealthComponent healthComponent, 
                DataComponent data, 
                MovementComponent movement)
        {
            ArmorData = data.Armor;
            _unit = movement;
            _healthComponent = healthComponent;
        }

        /// <summary>
        /// Calculate the total damage dealt within a successful hit, then update health.
        /// </summary>
        public void HandleHit(
            int firepower,
            Vector3? displacementToThis,
            float? distanceToCentre)
        {
            Logger.LogDamage($"ArmorComponent::HandleHit() called");
            int armorOfImpact;

            if (displacementToThis == null)
            {
                armorOfImpact = DetermineSideOfImpact();
            }
            else
            {
                armorOfImpact = DetermineSideOfImpact(displacementToThis.GetValueOrDefault());
            }

            float receivedDamage = 0;

            // Simple KE formula:
            if (armorOfImpact == 0)
            {
                receivedDamage = 2 * firepower;
            }
            else if (armorOfImpact == 1)
            {
                receivedDamage = firepower;
            }
            else
            {
                // Each point of firepower more than the armor value equals 0.5 damage,
                // but if we have 10 more firepower than armor, those points count double.
                float overkill = Mathf.Max(firepower - armorOfImpact - 10, 0);
                receivedDamage = (firepower + overkill - armorOfImpact) / 2 + 1;
            }

            if (receivedDamage > 0)
            {
                _healthComponent.UpdateHealth(_healthComponent.Health - receivedDamage);
            }
        }

        /// <summary>
        /// Use the displacement vector to calculate the angle of the shot
        /// </summary>
        public int DetermineSideOfImpact(Vector3 displacementToThis)
        {
            int index = 0;
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

        public int DetermineSideOfImpact()
        {
            // When no displacement vector is supplied, the damage is dealt to the top armor
            return ArmorData[4];
        }
    }
}
