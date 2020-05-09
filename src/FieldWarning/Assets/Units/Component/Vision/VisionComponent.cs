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

        // TODO: Add these values to YAML / UnitConfig schema
        [SerializeField]
        private float max_spot_range = 800f;
        [SerializeField]
        private float stealth_pen_factor = 1f;
        [SerializeField]
        private float stealth_factor = 10f;

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

        private Team _team {
            get { return _unit.Platoon.Owner.Team; }
        }

        public void Initialize(UnitDispatcher dispatcher)
        {
            _unit = dispatcher;
        }

        /// <summary>
        /// Notify all nearby enemy units that they may have to show themselves.
        /// Only works if they have colliders!
        /// </summary>
        public void ScanForEnemies()
        {
            Collider[] hits = Physics.OverlapSphere(
                    gameObject.transform.position,
                    max_spot_range,
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
                || !s.CanDetect(this) 
                || !ClearLineOfSight(s, this));

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
            if (spotter.CanDetect(this) && ClearLineOfSight(spotter, this)) 
            {
                if (_spotters.Count == 0) 
                {
                    ToggleUnitVisibility(true);
                }

                _spotters.Add(spotter);
            }
        }

        private bool CanDetect(VisionComponent target)
        {
            float distance = Vector3.Distance(
                    gameObject.transform.position,
                    target.gameObject.transform.position);
            return
                distance < max_spot_range
                && distance < max_spot_range * stealth_pen_factor / target.stealth_factor;
                
        }

        private bool ClearLineOfSight(VisionComponent spotter, VisionComponent target)
        {
            bool result = IsInLineOfSight(target.gameObject.transform.position, out _);
            return result;
        }

        /// <summary>
        /// For a given point, check if there is a clear line
        /// of sight. This does not use optics values, it only
        /// checks that there are no buildings/dense forests in the way.
        /// 
        /// The out parameter is set to the farthest visible point.
        /// </summary>
        public bool IsInLineOfSight(Vector3 point, out Vector3 visionBlocker)
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
