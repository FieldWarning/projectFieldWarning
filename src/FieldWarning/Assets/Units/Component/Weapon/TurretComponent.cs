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

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Manages the weapon rotation for a unit.
    /// 
    /// A turret is any rotatable part of a weapon. This includes things like 
    /// cannon barrels (vertical laying) and machine gun bodies. 
    /// </summary>
    /// 
    /// Turrets are generally held by a weapon and nested in a reverse hierarchy
    /// starting from the turret the weapon sits directly on and ending at a root
    /// turret that may be shared by multiple weapons/smaller turrets that sit on it.
    public class TurretComponent : MonoBehaviour
    {
        [SerializeField]
        private bool _isHowitzer = false;

        [SerializeField]
        private Transform _mount;
        [SerializeField]
        private Transform _turret;

        [SerializeField]
        private TurretComponent _parentTurret;
        
        [SerializeField]
        public float ArcHorizontal = 180, ArcUp = 40, ArcDown = 20, RotationRate = 40f;

        private TargetTuple _target = null;
        private int _curTargetPriority = 0;

        /// <summary>
        /// Set a target that the turret shall rotate to. 
        /// 
        /// Does nothing if there is already a higher-priority target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="priority"></param>
        public void SetTarget(TargetTuple target, int priority)
        {
            if (_target == null || priority >= _curTargetPriority) {
                _target = target;
                _curTargetPriority = priority;
            }

            _parentTurret?.SetTarget(target, priority);
        }
        
        public bool IsFacingTarget { get; private set; } = false;


        private void Update()
        {
            if (_target == null) {
                return;
            }

            bool aimed = false;
            float targetHorizontalAngle = 0f;
            float targetVerticalAngle = 0f;

            Vector3 pos = _target.Position;

            if (pos != Vector3.zero) {
                aimed = true;
                // comented out because arty has no shot emmiter:
                // shotEmitter.LookAt(pos);

                Vector3 directionToTarget = pos - _turret.position;
                Quaternion rotationToTarget = Quaternion.LookRotation(_mount.transform.InverseTransformDirection(directionToTarget));

                targetHorizontalAngle = rotationToTarget.eulerAngles.y.unwrapDegree();
                if (Mathf.Abs(targetHorizontalAngle) > ArcHorizontal) {
                    targetHorizontalAngle = 0f;
                    aimed = false;
                }

                targetVerticalAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                if (targetVerticalAngle < -ArcUp || targetVerticalAngle > ArcDown) {
                    targetVerticalAngle = 0f;
                    aimed = false;
                }
            }

            float horizontalAngle = _turret.localEulerAngles.y;
            float verticalAngle = _turret.localEulerAngles.x;
            float turn = Time.deltaTime * RotationRate;
            float deltaAngle;

            deltaAngle = (targetHorizontalAngle - horizontalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn) {
                horizontalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                horizontalAngle = targetHorizontalAngle;
            }

            #region ArtyAdditionalCode
            if (_isHowitzer)
                targetVerticalAngle = -ArcUp;
            #endregion

            deltaAngle = (targetVerticalAngle - verticalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn) {
                verticalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                verticalAngle = targetVerticalAngle;
            }

            _turret.localEulerAngles = new Vector3(verticalAngle, horizontalAngle, 0);

            IsFacingTarget = aimed;
            
            #region ArtyAdditionalCode
            if (_isHowitzer)
                IsFacingTarget = true;
            #endregion
        }

        public void WakeUp()
        {
            //_weaponComponents.ForEach(w => w.WakeUp(this));
            enabled = true;
        }
    }
}