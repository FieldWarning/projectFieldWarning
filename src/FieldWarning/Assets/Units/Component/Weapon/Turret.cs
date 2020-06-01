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
        private IWeapon _weapon; // weapon turret
        private float _fireRange; // weapon turret, in unity units

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

        private bool _isHowitzer = false; // purpose unclear, both

        private Transform _mount = null;  // purpose unclear, both
        private Transform _turret = null; // object being rotated, both

        public float ArcHorizontal = 180, ArcUp = 40, ArcDown = 20, RotationRate = 40f;

        private static GameObject _muzzleFlashResource;
        private static AudioClip _gunSoundResource;

        public bool IsFacingTarget { get; private set; } = false;

        public Turret(GameObject unit, TurretConfig turretConfig)
        {
            ArcHorizontal = turretConfig.ArcHorizontal;
            ArcUp = turretConfig.ArcUp;
            ArcDown = turretConfig.ArcDown;
            RotationRate = turretConfig.RotationRate;
            _priority = turretConfig.Priority;
            _turret = RecursiveFindChild(unit.transform, turretConfig.TurretRef);
            _mount = RecursiveFindChild(unit.transform, turretConfig.MountRef);
            Children = new List<Turret>();

            if (turretConfig.Children.Count > 0)
            {
                foreach (TurretConfig childTurretConfig in turretConfig.Children)
                {
                    Children.Add(new Turret(unit, childTurretConfig));
                }
            }
            else
            {
                AudioSource shotAudioSource = _turret.gameObject.AddComponent<AudioSource>();

                // The Unit json parser creates objects even when there are none,
                // so instead of testing for null we have to test for a 0 value..
                if (turretConfig.Howitzer.FireRange != 0)
                {
                    _isHowitzer = true;

                    Transform barrelTip = 
                            turretConfig.Cannon.BarrelTipRef == "" ? 
                                    _turret :
                                    RecursiveFindChild(
                                            _turret.parent, 
                                            turretConfig.Howitzer.BarrelTipRef);
                    if (barrelTip == null)
                    {
                        barrelTip = _turret;
                    }

                    _muzzleFlashResource = Resources.Load<GameObject>(
                            turretConfig.Howitzer.MuzzleFlash);
                    _gunSoundResource = Resources.Load<AudioClip>(
                            turretConfig.Howitzer.Sound);
                    GameObject muzzleFlashGO = GameObject.Instantiate(
                            _muzzleFlashResource, barrelTip);

                    _weapon = new Howitzer(
                            turretConfig.Howitzer,
                            shotAudioSource,
                            _gunSoundResource,
                            muzzleFlashGO.GetComponent<VisualEffect>(),
                            barrelTip);
                    _fireRange =
                            turretConfig.Howitzer.FireRange * Constants.MAP_SCALE;
                }
                else if (turretConfig.Cannon.FireRange != 0)
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

                    _muzzleFlashResource = Resources.Load<GameObject>(turretConfig.Cannon.MuzzleFlash);
                    _gunSoundResource = Resources.Load<AudioClip>(turretConfig.Cannon.Sound);
                    GameObject muzzleFlashGO = GameObject.Instantiate(
                            _muzzleFlashResource, barrelTip);

                    _weapon = new Cannon(
                            turretConfig.Cannon,
                            shotAudioSource,
                            _gunSoundResource,
                            muzzleFlashGO.GetComponent<VisualEffect>(),
                            barrelTip);
                    _fireRange =
                            turretConfig.Cannon.FireRange * Constants.MAP_SCALE;
                }
                else
                {
                    Debug.LogError("Couldn't create a weapon in a turret without children. " +
                            "No weapon specified in the config?");
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
            bool targetInRange = _fireRange > distanceToTarget;
            if (IsFacingTarget && targetInRange)
            {
                Vector3 vectorToTarget = _target.Position - _turret.transform.position;
                shotFired = _weapon.TryShoot(
                    _target, vectorToTarget, isServer);
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
                IsFacingTarget = _isHowitzer;
                return;
            }

            bool aimed = false;
            float targetHorizontalAngle = 0f;
            float targetVerticalAngle = 0f;

            Vector3 pos = _target.Position;

            if (pos != Vector3.zero)
            {
                aimed = true;
                // commented out because arty has no shot emmiter:
                // shotEmitter.LookAt(pos);

                Vector3 directionToTarget = pos - _turret.position;
                Quaternion rotationToTarget = Quaternion.LookRotation(
                        _mount.transform.InverseTransformDirection(directionToTarget));

                targetHorizontalAngle = rotationToTarget.eulerAngles.y.unwrapDegree();

                // If this turret has no flexibility (ArcHorizontal = 0) and is fully
                // rotated by a parent turret, it can get stuck 0.0000000001 degrees
                // away from the target due to float rounding errors (parent rounds
                // one way and decides he's done, child rounds the other way).
                // So round away the last degree to avoid this case:
                targetHorizontalAngle = targetHorizontalAngle > 0 ?
                        (float)Math.Floor(targetHorizontalAngle)
                        : (float)Math.Ceiling(targetHorizontalAngle);
                if (Mathf.Abs(targetHorizontalAngle) > ArcHorizontal)
                {
                    targetHorizontalAngle = 0f;
                    aimed = false;
                }

                targetVerticalAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                targetVerticalAngle = (float)Math.Floor(targetVerticalAngle);
                if (targetVerticalAngle < -ArcUp || targetVerticalAngle > ArcDown)
                {
                    targetVerticalAngle = 0f;
                    aimed = false;
                }
            }

            float turn = Time.deltaTime * RotationRate;
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

            #region ArtyAdditionalCode
            if (_isHowitzer)
                targetVerticalAngle = -ArcUp;
            #endregion

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

            #region ArtyAdditionalCode
            if (_isHowitzer)
                IsFacingTarget = true;
            #endregion

            foreach (Turret turret in Children)
            {
                turret.HandleUpdate();
            }
        }

        private void TurnTurretBackToDefaultPosition()
        {
            float turn = Time.deltaTime * RotationRate;
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
            float maxRange = _fireRange;
            foreach (Turret turret in Children)
            {
                float turretMax = turret.MaxRange(target);
                maxRange = maxRange > turretMax ? maxRange : turretMax;
            }
            return maxRange;
        }
    }
}
