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
using UnityEngine;
using UnityEngine.VFX;

using PFW.Model.Armory.JsonContents;
using static PFW.Util;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Controls a specific part of a unit model and rotates it to face a target.
    /// 
    /// A turret is any rotatable part of a weapon. This includes things like
    /// cannon barrels (vertical laying) and machine gun bodies.
    /// 
    /// A turret can either have a weapon, in which case it rotates based on what 
    /// the weapon is aiming at, or it has child turrets, in which case it 
    /// rotates based on what the child turrets need.
    /// </summary>
    public class Turret
    {
        // targetingStrategy
        private readonly Cannon _weapon; // weapon turret

        /// <summary>
        ///     Represents the max ranges against
        ///     each possible target type.
        /// </summary>
        private readonly float[] _maxRanges;

        // The default here has significance because only the
        //      last turrets know the actual weapon's velocity. 
        // TODO Give toplevel turrets the ability to know 
        //      the velocity of the most important gun in their hierarchy.
        private readonly float _shellVelocity = 1000 * Constants.MAP_SCALE;

        /// <summary>
        /// The explicit target is one set by player input,
        /// while the real target can either be that or something
        /// picked automatically (for example, something that is in range
        /// while the explicit target is not).
        /// </summary>
        private TargetTuple _explicitTarget;  // weapon turret, sync
        private TargetTuple _target;  // weapon turret, sync
        private int _priority;  // weapon turret, higher is better
        public List<Turret> Children; // parent turret, sync

        private Transform _turret = null; // object being rotated, both

        private readonly float _arcHorizontal = 180;
        private readonly float _arcUp = 40;
        private readonly float _arcDown = 20;
        private readonly float _rotationRate = 40f;

        public bool IsFacingTarget { get; private set; } = false;

        public Turret(GameObject unit, TurretConfig turretConfig)
        {
            _maxRanges = new float[(int)TargetType._SIZE];
            _arcHorizontal = turretConfig.ArcHorizontal;
            _arcUp = turretConfig.ArcUp;
            _arcDown = turretConfig.ArcDown;
            _rotationRate = turretConfig.RotationRate;
            _priority = turretConfig.Priority;
            _turret = RecursiveFindChild(unit.transform, turretConfig.TurretRef);
            Children = new List<Turret>();

            if (turretConfig.Children != null && turretConfig.Children.Count > 0)
            {
                foreach (TurretConfig childTurretConfig in turretConfig.Children)
                {
                    Children.Add(new Turret(unit, childTurretConfig));
                }
            }
            else
            {
                AudioSource shotAudioSource = _turret.gameObject.AddComponent<AudioSource>();

                if (turretConfig.Cannon != null)
                {
                    Transform barrelTip =
                            turretConfig.Cannon.BarrelTipRef == "" ?
                                    _turret :
                                    RecursiveFindChild(
                                            _turret.parent,
                                            turretConfig.Cannon.BarrelTipRef);
                    if (barrelTip == null)
                    {
                        barrelTip = _turret;
                    }

                    _weapon = new Cannon(
                            turretConfig.Cannon,
                            shotAudioSource,
                            barrelTip);

                    _maxRanges = _weapon.CalculateMaxRanges();
                    _shellVelocity = turretConfig.Cannon.Velocity * Constants.MAP_SCALE;
                }
                else
                {
                    Logger.LogConfig("Couldn't create a weapon in a turret without children. " +
                            "No weapon specified in the config?", LogLevel.ERROR);
                }
            }
        }

        /// <summary>
        /// Shoot at the current target if in range.
        /// </summary>
        /// <returns>True if a shot was produced.</returns>
        public bool MaybeShoot(float distanceToTarget, bool isServer)
        {
            bool shotFired = false;
            bool targetInRange = _maxRanges[(int)_target.Type] > distanceToTarget;
            if (IsFacingTarget && targetInRange)
            {
                Vector3 vectorToTarget = _target.Position - _turret.transform.position;
                shotFired = _weapon.TryShoot(
                    _target, vectorToTarget, distanceToTarget, isServer);
            }

            foreach (Turret turret in Children)
            {
                shotFired |= turret.MaybeShoot(distanceToTarget, isServer);
            }

            return shotFired;
        }

        public void HandleUpdate()
        {
            _weapon?.HandleUpdate();

            if (_target == null || !_target.Exists)
            {
                TurnTurretBackToDefaultPosition();
                return;
            }

            bool aimed = false;
            float targetHorizontalAngle = 0f;
            float targetVerticalAngle = 0f;

            if (_target.Position != Vector3.zero)
            {
                aimed = true;

                Vector3 directionToTarget = _target.Position - _turret.position;
                directionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z);
                Quaternion rotationToTarget = Quaternion.LookRotation(
                        _turret.parent.transform.InverseTransformDirection(directionToTarget));

                // Add any necessary cannon elevation to the rotation
                rotationToTarget *= ShellBehaviour.CalculateBarrelAngle(
                        _shellVelocity, _turret.transform.position, _target.Position, out _);

                targetHorizontalAngle = rotationToTarget.eulerAngles.y.unwrapDegree();

                // If this turret has no flexibility (ArcHorizontal = 0) and is fully
                // rotated by a parent turret, it can get stuck 0.0000000001 degrees
                // away from the target due to float rounding errors (parent rounds
                // one way and decides he's done, child rounds the other way).
                // So round away the last degree to avoid this case:
                targetHorizontalAngle = Util.RoundTowardZero(targetHorizontalAngle);
                if (Mathf.Abs(targetHorizontalAngle) > _arcHorizontal)
                {
                    targetHorizontalAngle = 0f;
                    aimed = false;
                }

                targetVerticalAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                targetVerticalAngle = (float)Math.Floor(targetVerticalAngle);
                if (targetVerticalAngle < -_arcUp || targetVerticalAngle > _arcDown)
                {
                    targetVerticalAngle = 0f;
                    aimed = false;
                }
            }

            float turn = Time.deltaTime * _rotationRate;
            float horizontalAngle = _turret.localEulerAngles.y;
            float verticalAngle = _turret.localEulerAngles.x;
            float deltaAngle;

            deltaAngle = (targetHorizontalAngle - horizontalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                horizontalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            }
            else
            {
                horizontalAngle = targetHorizontalAngle;
            }

            deltaAngle = (targetVerticalAngle - verticalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                verticalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            }
            else
            {
                verticalAngle = targetVerticalAngle;
            }

            _turret.localEulerAngles = new Vector3(verticalAngle, horizontalAngle, 0);

            IsFacingTarget = aimed;

            foreach (Turret turret in Children)
            {
                turret.HandleUpdate();
            }
        }

        private void TurnTurretBackToDefaultPosition()
        {
            float turn = Time.deltaTime * _rotationRate;
            Vector3 localEulerAngles = _turret.localEulerAngles;

            float targetHorizontalAngle = 0f;
            float targetVerticalAngle = 0f;
            float horizontalAngle = localEulerAngles.y;
            float verticalAngle = localEulerAngles.x;

            if (Math.Abs(horizontalAngle) < 0.1f && Math.Abs(verticalAngle) < 0.1f)
                return;

            float deltaAngle;

            deltaAngle = (targetHorizontalAngle - horizontalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                horizontalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
            }
            else
            {
                horizontalAngle = targetHorizontalAngle;
            }

            deltaAngle = (targetVerticalAngle - verticalAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                verticalAngle += (deltaAngle > 0 ? 1 : -1) * turn;
            }
            else
            {
                verticalAngle = targetVerticalAngle;
            }

            _turret.localEulerAngles = new Vector3(verticalAngle, horizontalAngle, 0);
        }

        public void ClearExplicitTarget()
        {
            _explicitTarget = null;
            _target = null;
            foreach (Turret turret in Children)
            {
                turret.ClearExplicitTarget();
            }
        }

        /// <summary>
        /// Sets a max-priority target for this turret.
        /// </summary>
        public void SetExplicitTarget(TargetTuple target)
        {
            _explicitTarget = target;
            _target = target;  // TODO check if we can shoot it before setting this
            foreach (Turret turret in Children)
            {
                turret.SetExplicitTarget(target);
            }
        }

        /// <summary>
        /// Get the range at which this turret can shoot at a specific target.
        /// </summary>
        /// 
        /// TODO In the future we will need to also return -1
        /// for turrets that can't shoot the target at all.
        public float MaxRange(TargetTuple target)
        {
            float maxRange = _maxRanges[(int)target.Type];
            foreach (Turret turret in Children)
            {
                float turretMax = turret.MaxRange(target);
                maxRange = maxRange > turretMax ? maxRange : turretMax;
            }
            return maxRange;
        }
    }
}
