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
using Mirror;

using PFW.Model.Match;
using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;
using PFW.Units.Component.Vision;

namespace PFW.Units.Component.Weapon
{

    public class TurretSystem : NetworkBehaviour
    {
        public UnitDispatcher Unit { get; private set; } // TODO set
        private bool _movingTowardsTarget = false;

        /// <summary>
        /// The explicit target is one set by player input,
        /// while the real target can either be that or something
        /// picked automatically (for example, something that is in range
        /// while the explicit target is not).
        /// </summary>
        private TargetTuple _explicitTarget;
        private TargetTuple __targetBackingField;
        public List<Cannon> AllWeapons { get; private set; }

        private VisionComponent _vision => Unit.VisionComponent;

        private TargetTuple _target 
        { 
            get 
            {
                return __targetBackingField;
            } 
            set
            {
                __targetBackingField = value;

                if (value != null)
                {
                    _fireRange = MaxRange(value);
                    //_fireRangeIsIndirect = _fireRange == MaxRangeIndirectFire(value);
                }
            }
        }

        public bool HasTargetingOrder 
        {
            get 
            {
                return _explicitTarget != null;
            }
        }

        /// <summary>
        /// The max fire range to the current target, in unity units.
        /// </summary>
        private float _fireRange;
        //private bool _fireRangeIsIndirect;

        public List<Turret> Children; // sync

        /// <summary>
        /// Constructor for MonoBehaviour
        /// </summary>
        public void Initialize(GameObject unit, Unit armoryUnit)
        {
            Children = new List<Turret>();
            AllWeapons = new List<Cannon>();
            if (armoryUnit.Config.Turrets != null)
            {
                foreach (TurretConfig turretConfig in armoryUnit.Config.Turrets)
                {
                    Children.Add(new Turret(unit, turretConfig, AllWeapons));
                }
            }
            Unit = GetComponent<UnitDispatcher>();
            enabled = true;
        }

        private void Update()
        {
            foreach (Turret turret in Children)
            {
                turret.Rotate(_target);
            }

            float distanceToTarget = 99999;
            if (_target != null && _target.Exists)
            {
                // TODO move most of the Update logic to the respective turrets
                // (likely to a new member class of them called 'TargetingStrategy')
                distanceToTarget = Vector3.Distance(
                        Unit.transform.position, _target.Position);
                if (_fireRange > distanceToTarget && _movingTowardsTarget)
                {
                    StopChasingTarget();
                }
                else if (distanceToTarget >= _fireRange && !_movingTowardsTarget)
                {
                    StartChasingTarget();
                }

                if (_target.IsUnit && !_target.Enemy.VisionComponent.IsSpotted)
                {
                    Logger.LogTargeting(
                            LogLevel.DEBUG,
                            gameObject,
                            "Dropping a target because it is no longer spotted.");
                    if (_target == _explicitTarget)
                    {
                        _explicitTarget = null;
                    }
                    _target = null;
                }

                if (!HasTargetingOrder && _target != null)
                {
                    if (!_vision.IsInHardLineOfSightFast(_target.Position)
                        || !_vision.IsInSoftLineOfSight(_target.Position, 0)
                        || distanceToTarget > _fireRange)
                    {
                        _target = null;
                        Logger.LogTargeting(
                                LogLevel.DEBUG,
                                gameObject,
                                "Dropping a target because it is out of range or LoS.");
                    }
                }
            }

            if (_target != null && _target.Exists)
            {
                bool targetInRange = !_movingTowardsTarget;
                bool salvoConcluded = false;

                foreach (Turret turret in Children)
                {
                    salvoConcluded |= turret.MaybeShoot(_target, distanceToTarget, isServer);
                }

                // If shooting at the ground, stop after the first salvo:
                if (salvoConcluded && _target.IsGround)
                {
                    _target = null;
                    _explicitTarget = null;
                }
            }
            else
            {
                FindAndTargetClosestEnemy();
            }
        }

        private void StartChasingTarget()
        {
            _movingTowardsTarget = true;
            Unit.SetDestination(_target.Position);

            Logger.LogTargeting(
                    LogLevel.DEBUG,
                    gameObject,
                    "Starting to chase a target that has moved out of range.");
        }

        private void StopChasingTarget()
        {
            _movingTowardsTarget = false;
            Unit.SetDestination(Unit.transform.position);

            Logger.LogTargeting(
                    LogLevel.DEBUG, 
                    gameObject,
                    "Stopped moving because a targeted enemy unit is in range.");
        }

        private void FindAndTargetClosestEnemy()
        {
            Logger.LogTargeting(LogLevel.DEBUG, gameObject, "Scanning for a target.");

            // TODO utilize precomputed distance lists from session
            // Maybe add Sphere shaped collider with the radius of the range and then 
            // use trigger enter and exit to keep a list of in range Units

            foreach (UnitDispatcher enemy in MatchSession.Current.EnemiesByTeam[Unit.Platoon.Team])
            {
                if (!enemy.VisionComponent.IsSpotted)
                {
                    continue;
                }

                // See if they are shootable:
                float distance = Vector3.Distance(
                        Unit.transform.position, 
                        enemy.Transform.position);
                if (distance < MaxRangeDirectFire(enemy.TargetTuple) // Indirect weapons dont autotarget
                    && _vision.IsInHardLineOfSightFast(enemy.TargetTuple.Position)
                    && _vision.IsInSoftLineOfSight(enemy.TargetTuple.Position, 0))
                {
                    Logger.LogTargeting(
                            LogLevel.DEBUG,
                            gameObject,
                            "Target found and selected after scanning.");
                    SetTarget(enemy.TargetTuple, false);
                    break;
                }
            }
        }

        public void CancelOrders()
        {
            _movingTowardsTarget = false;
            _explicitTarget = null;
            _target = null;
        }

        /// <summary>
        /// Set a ground position as the shooting target.
        /// </summary>
        public void TargetPosition(Vector3 position, bool autoApproach = true)
        {
            SetTarget(new TargetTuple(position), autoApproach);
        }

        /// <summary>
        /// Set a max-priority target for all child turrets.
        /// </summary>
        private void SetTarget(TargetTuple target, bool autoApproach)
        {
            Logger.LogTargeting(
                    LogLevel.DEBUG,
                    gameObject,
                    "Received target from the outside.");
            float distance = Vector3.Distance(Unit.transform.position, target.Position);

            _explicitTarget = target;
            _target = target;

            if (distance > _fireRange && autoApproach)
            {
                _movingTowardsTarget = true;
                // TODO if the UnitDispatcher can detect that we're in range, we
                // would be able to drop the handle to it
                Unit.SetDestination(target.Position);
            }
        }

        /// <summary>
        /// Calculate the max range this turret can shoot at with at least one weapon,
        /// IN UNITY UNITS.
        /// </summary>
        /// This is a method to avoid storing duplicate information, and
        /// because we may want to ignore disabled turrets, or turrets 
        /// that can't shoot at a specific target etc.
        /// 
        /// TODO: Code duplication can be reduced if we only implement this in 
        /// the turret class and have a fake toplevel turret we call this method on,
        /// but a fake turret like that also adds complexity, hard to decide.
        private float MaxRange(
                TargetTuple target,
                bool includeDirectFire,
                bool includeIndirectFire)
        {
            float maxRange = 0;
            foreach (Turret turret in Children)
            {
                float turretMax;
                if (includeDirectFire && includeIndirectFire)
                {
                    turretMax = turret.MaxRange(target);
                }
                else if (includeDirectFire)
                {
                    turretMax = turret.MaxRangeDirectFire(target);
                }
                else if (includeIndirectFire)
                {
                    turretMax = turret.MaxRangeIndirectFire(target);
                }
                else 
                {
                    turretMax = 0;
                }


                if (maxRange < turretMax)
                {
                    maxRange = turretMax;
                }
            }
            return maxRange;
        }

        /// <summary>
        /// The max range of this turret system for fire position purposes,
        /// IN UNITY UNITS.
        /// Note that this can change as weapons are disabled, and so
        /// return values from this method should not be cached.
        /// </summary>
        public float MaxFirePosRange()
            => MaxRange(new TargetTuple(Vector3.zero), true, true);
        private float MaxFirePosRangeDirectFire()
            => MaxRange(new TargetTuple(Vector3.zero), true, false);
        public float MaxFirePosRangeIndirectFire()
            => MaxRange(new TargetTuple(Vector3.zero), false, true);

        private float MaxRange(TargetTuple target)
            => MaxRange(target, true, true);
        private float MaxRangeDirectFire(TargetTuple target)
            => MaxRange(target, true, false);
        private float MaxRangeIndirectFire(TargetTuple target)
            => MaxRange(target, false, true);
    }
}
