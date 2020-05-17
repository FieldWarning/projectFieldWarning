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
using System.Collections.Generic;

using PFW.Model.Game;
using PFW.Units.Component.Data;

namespace PFW.Units.Component.Vision
{
    public class VisionComponent : MonoBehaviour
    {
        /// <summary>
        /// Itold has advised us to put building colliders
        /// slightly above ground. Benefits unclear.
        /// 
        /// This means that a raycast between two units can miss
        /// a building by going under the collider.
        /// 
        /// Until we understand the consequences of lowering the colliders,
        /// or until we do our raycasts from a vision port higher on the
        /// unit, we will need to artificially elevate our raycasts.
        /// </summary>
        const float RAYCAST_WORKAROUND = 0.05f;

        // BEGIN Constants for the soft line of sight system ---

        /// <summary>
        /// Sitting on a forest tile instantly applies 
        /// at least this much penalty against spot attempts.
        /// </summary>
        const int FOREST_INITIAL_PENALTY = 500;

        /// <summary>
        /// An enemy at this distance or less is always spotted
        /// unless there's a hard line of sight blocker.
        /// </summary>
        const int GUARANTEED_VISION_CUTOFF = 300;

        /// <summary>
        /// An enemy behind more than n meters of forest can't ever be spotted.
        /// </summary>
        const int HARD_FOREST_VISION_CUTOFF = 300;

        /// <summary>
        /// Each meter of forest applies this much vision penalty.
        /// </summary>
        const int FOREST_PENALTY_PER_METER = 5;

        /// <summary>
        /// Magnifies the effect of stealth - pen differences
        /// for the pure distance calculation.
        /// </summary>
        const float STEALTH_INFLUENCE_ON_DISTANCE = 0.3f;

        /// <summary>
        /// Magnifies the effect of stealth - pen differences
        /// for the initial penalty against detecting someone
        /// on a forest tile.
        /// </summary>
        const float STEALTH_INFLUENCE_ON_INITIAL_CAMO = 1f;

        /// <summary>
        /// Magnifies the effect of stealth - pen differences
        /// for the penalty from meters of forest between
        /// spotter and target.
        /// </summary>
        const float STEALTH_INFLUENCE_ON_OBSTRUCTIONS = 1f;

        // END Constants for the soft line of sight system ---

        // TODO: Add these values to YAML / UnitConfig schema
        private int _maxSpottingRange = 3000;  // in meters
        private float _stealthFactor = 1f;
        private float _stealthPenFactor = 1f;

        private HashSet<VisionComponent> _spotters = new HashSet<VisionComponent>();

        /// <summary>
        /// Is the unit visible to the player? Note that own units are always visible.
        /// </summary>
        public bool IsVisible { get; private set; } = true;

        /// <summary>
        /// Is the unit spotted by any enemy units?
        /// The difference from IsVisible is that this also works for our own units.
        /// </summary>
        public bool IsSpotted { get { return _spotters.Count != 0; } }

        private UnitDispatcher _unit;
        private TerrainMap _terrainMap;

        private Team _team {
            get { return _unit.Platoon.Owner.Team; }
        }

        public void Initialize(UnitDispatcher dispatcher, DataComponent unitData)
        {
            _unit = dispatcher;
            _terrainMap = MatchSession.Current.TerrainMap;
            _maxSpottingRange = unitData.MaxSpottingRange;
            _stealthFactor = unitData.Stealth;
            _stealthPenFactor = unitData.StealthPenetration;
        }

        /// <summary>
        /// Notify all nearby enemy units that they may have to show themselves.
        /// Only works if they have colliders!
        /// </summary>
        public void ScanForEnemies()
        {
            Collider[] hits = Physics.OverlapSphere(
                    gameObject.transform.position,
                    _maxSpottingRange,
                    LayerMask.NameToLayer("Selectable"),
                    QueryTriggerInteraction.Ignore);

            foreach (Collider c in hits) {
                GameObject go = c.gameObject;

                // this finds colliders, health bars and all other crap except units
                UnitDispatcher unit = go.GetComponentInParent<UnitDispatcher>();
                if (unit == null || !unit.enabled)
                    continue;

                // This assumes that all selectables with colliders have 
                // a visibility manager, which may be a bad assumption:
                if (unit.Platoon.Owner.Team != _team)
                    unit.VisionComponent.MaybeReveal(this);
            }
        }

        /// <summary>
        /// Check if there are any enemies that can detect this unit
        /// and make it invisible if not.
        /// </summary>
        public void MaybeHideFromEnemies()
        {
            if (!IsVisible)
                return;

            _spotters.RemoveWhere(
                s => s == null
                || !s.IsInSoftLineOfSight(this)
                || !IsInHardLineOfSight(s));

            if (_spotters.Count == 0)
                ToggleUnitVisibility(false);
        }

        /// <summary>
        /// It is the responsibility of the defender to
        /// reveal themselves if necessary. This is done here.
        /// </summary>
        /// <param name="spotter"></param>
        private void MaybeReveal(VisionComponent spotter)
        {
            if (spotter.IsInSoftLineOfSight(this) && IsInHardLineOfSight(spotter))
            {
                if (_spotters.Count == 0)
                {
                    ToggleUnitVisibility(true);
                }

                _spotters.Add(spotter);
            }
        }

        /// <summary>
        /// For a given point, check if there is a clear line
        /// of sight. This does not use optics values, it only
        /// checks that there are no buildings/dense forests in the way.
        /// 
        /// The out parameter is set to the farthest visible point.
        /// </summary>
        public bool IsInHardLineOfSight(Vector3 point, out Vector3 visionBlocker)
        {
            int layerMask = 1 << LayerMask.NameToLayer("HardLosBlock");
            Vector3 SpotterPosition = gameObject.transform.position;
            SpotterPosition.y += RAYCAST_WORKAROUND;
            Vector3 TargetPosition = point;
            TargetPosition.y += RAYCAST_WORKAROUND;
            Vector3 TargetDirection = TargetPosition - SpotterPosition;
            float DistanceToTarget = Vector3.Distance(SpotterPosition, TargetPosition);

            RaycastHit hit;
            if (!Physics.Raycast(SpotterPosition, TargetDirection, out hit, DistanceToTarget, layerMask))
            {
                visionBlocker = point;

                return true;
            }
            else
            {
                visionBlocker = hit.point;

                return false;
            }
        }

        public bool IsInHardLineOfSight(VisionComponent other) =>
                IsInHardLineOfSight(other.transform.position, out _);

        /// <summary>
        /// Checks that the unit can be seen through forests
        /// and other 'soft' line of sight blockers.
        /// </summary>
        /// Draws a line from the other component to this one,
        /// applying a penalty for every forest tile encountered.
        /// If the target sits on a forest tile there is also a large
        /// initial penalty. Penalty is multiplied by stealth - optics
        /// difference.
        /// Exceptions: 300m of distance = guaranteed spot;
        ///             300m+ of forest = guaranteed non-spot;
        public bool IsInSoftLineOfSight(Vector3 targetPos, float targetStealth)
        {
            float distance = Vector3.Distance(
                    targetPos, transform.position);
            distance /= Constants.MAP_SCALE;

            if (GUARANTEED_VISION_CUTOFF >= distance)
            {
                return true;
            }

            if (distance > _maxSpottingRange)
            {
                return false;
            }

            float penaltyBonus;
            if (targetStealth >= _stealthPenFactor)
            {
                penaltyBonus = 1 + targetStealth - _stealthPenFactor;
            }
            else
            {
                penaltyBonus =
                        (targetStealth + 1) / (_stealthPenFactor + 1);
            }

            float penalty = distance +
                    distance * penaltyBonus * STEALTH_INFLUENCE_ON_DISTANCE;

            if (_terrainMap.GetTerrainType(targetPos) == TerrainMap.FOREST)
            {
                penalty += FOREST_INITIAL_PENALTY +
                        FOREST_INITIAL_PENALTY * penaltyBonus * STEALTH_INFLUENCE_ON_INITIAL_CAMO;
            }

            float forestLength = _terrainMap.GetForestLengthOnLine(
                    targetPos, transform.position);
            if (forestLength > HARD_FOREST_VISION_CUTOFF)
            {
                return false;
            }

            float forestPenalty = forestLength * FOREST_PENALTY_PER_METER;
            penalty += forestPenalty + 
                    forestPenalty * penaltyBonus * STEALTH_INFLUENCE_ON_OBSTRUCTIONS;

            return _maxSpottingRange > penalty;
        }

        private bool IsInSoftLineOfSight(VisionComponent other)
                => IsInSoftLineOfSight(other.transform.position, other._stealthFactor);

        public void ToggleUnitVisibility(bool revealUnit)
        {
            IsVisible = revealUnit;
            ToggleAllRenderers(gameObject, revealUnit);

            MaybeTogglePlatoonVisibility();
        }

        /// <summary>
        /// If all units are invisible, make the platoon invisible. 
        /// </summary>
        private void MaybeTogglePlatoonVisibility()
        {
            PlatoonBehaviour platoon = _unit.Platoon;

            bool visible = !platoon.Units.TrueForAll(
                          u => !u.VisionComponent.IsVisible);

            platoon.ToggleLabelVisibility(visible);
        }

       
        private void ToggleAllRenderers(GameObject o, bool visible)
        {
            Renderer[] allRenderers = o.GetComponentsInChildren<Renderer>();
            foreach (Renderer childRenderer in allRenderers)
                childRenderer.enabled = visible;
        }

    }
}
