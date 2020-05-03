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

using PFW.Model.Game;
using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;

namespace PFW.Units.Component.Weapon
{

    public class TurretSystem : NetworkBehaviour
    {
        public UnitDispatcher Unit { get; private set; } // TODO set
        private bool _movingTowardsTarget = false;
        private TargetTuple _explicitTarget;
        private TargetTuple __targetBackingField;
        private TargetTuple _target 
        { 
            get 
            {
                return __targetBackingField;
            } 
            set
            {
                __targetBackingField = value;
                _fireRange = MaxRange(value);
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
        /// The max fire range to the current target.
        /// </summary>
        private int _fireRange;  
        public List<Turret> Children; // sync

        /// <summary>
        /// Constructor for MonoBehaviour
        /// </summary>
        public void Initialize(GameObject unit, Unit armoryUnit)
        {
            Children = new List<Turret>();
            foreach (TurretConfig turretConfig in armoryUnit.Config.Turrets)
            {
                Children.Add(new Turret(unit, turretConfig));
            }
            Unit = GetComponent<UnitDispatcher>();
            enabled = true;
        }

        private void Update()
        {
            foreach (Turret turret in Children)
            {
                turret.HandleUpdate();
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
                    StopMoving();
                }
                else if (distanceToTarget >= _fireRange && !_movingTowardsTarget)
                { 
                    // todo start chasing target again..
                }

                MaybeDropOutOfRangeTarget();

                if (_target.IsUnit && !_target.Enemy.VisionComponent.IsSpotted)
                {
                    Logger.LogTargeting(
                        "Dropping a target because it is no longer spotted.", gameObject);
                    _target = null;
                }
            }

            if (_target != null && _target.Exists)
            {
                bool targetInRange = !_movingTowardsTarget;
                bool shotFired = false;

                foreach (Turret turret in Children)
                {
                    shotFired |= turret.MaybeShoot(distanceToTarget, isServer);
                }

                // If shooting at the ground, stop after the first shot:
                if (shotFired && _target.IsGround)
                {
                    _target = null;
                    _explicitTarget = null;
                    foreach (Turret turret in Children)
                    {
                        turret.SetExplicitTarget(_explicitTarget);
                    }
                }
            }
            else
            {
                FindAndTargetClosestEnemy();
            }
        }

        private void StopMoving()
        {
            _movingTowardsTarget = false;
            Unit.SetDestination(Unit.transform.position);

            Logger.LogTargeting(
                "Stopped moving because a targeted enemy unit is in range.", gameObject);
        }

        private void FindAndTargetClosestEnemy()
        {
            Logger.LogTargeting("Scanning for a target.", gameObject);

            // TODO utilize precomputed distance lists from session
            // Maybe add Sphere shaped collider with the radius of the range and then 
            // use trigger enter and exit to keep a list of in range Units

            foreach (UnitDispatcher enemy in MatchSession.Current.EnemiesByTeam[Unit.Platoon.Owner.Team])
            {
                if (!enemy.VisionComponent.IsSpotted)
                {
                    continue;
                }

                // See if they are in range of weapon:
                float distance = Vector3.Distance(Unit.transform.position, enemy.Transform.position);
                if (distance < MaxRange(enemy.TargetTuple))
                {
                    Logger.LogTargeting("Target found and selected after scanning.", gameObject);
                    SetTarget(enemy.TargetTuple, false);
                    break;
                }
            }
        }

        /// <summary>
        /// If the target is an enemy unit and it is out of range,
        /// forget about it.
        /// </summary>
        private void MaybeDropOutOfRangeTarget()
        {
            // We only drop unit targets, not positions:
            if (_target.Enemy == null)
                return;

            float distance = Vector3.Distance(Unit.transform.position, _target.Position);
            if (distance > _fireRange)
            {
                _target = null;
                Logger.LogTargeting("Dropping a target because it is out of range.", gameObject);
            }
        }

        /// <summary>
        /// Set a ground position as the shooting target.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="autoApproach"></param>
        public void TargetPosition(Vector3 position, bool autoApproach = true)
        {
            SetTarget(new TargetTuple(position), autoApproach);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="autoApproach"></param>
        private void SetTarget(TargetTuple target, bool autoApproach)
        {
            Logger.LogTargeting("Received target from the outside.", gameObject);
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

            foreach (Turret turret in Children)
            {
                turret.SetExplicitTarget(_explicitTarget);
            }
        }

        /// <summary>
        /// Calculate the max range this turret can shoot at with at least one weapon.
        /// </summary>
        /// This is a method to avoid storing duplicate information, and
        /// because we may want to ignore disabled turrets, or turrets 
        /// that can't shoot at a specific target etc.
        /// 
        /// TODO: Code duplication can be reduced if we only implement this in 
        /// the turret class and have a fake toplevel turret we call this method on,
        /// but a fake turret like that also adds complexity, hard to decide.
        /// <returns></returns>
        private int MaxRange(TargetTuple target)
        {
            int maxRange = 0;
            foreach (Turret turret in Children)
            {
                int turretMax = turret.MaxRange(target);
                maxRange = maxRange > turretMax ? maxRange : turretMax;
            }
            return maxRange;
        }
    }
}
