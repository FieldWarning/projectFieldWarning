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

using PFW.Units;
using UnityEngine;

namespace PFW.UI.Ingame.UnitLabel
{
    public sealed class OverlayFactory
    {
        private OverlayFactory()
        {
        }

        public static OverlayFactory Instance { get; } = new OverlayFactory();

        public WaypointOverlayBehavior CreateWaypointOverlay(PlatoonBehaviour pb)
        {
            var overlayPrefab = Resources.Load<GameObject>("WaypointOverlay");

            var waypointOverlayBehavior = Object.Instantiate(
                    overlayPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<WaypointOverlayBehavior>();

            waypointOverlayBehavior.Initialize(pb);

            return waypointOverlayBehavior;
        }

        public TargetingOverlay CreateTargetingOverlay(UnitDispatcher unit)
        {
            var overlayPrefab = Resources.Load<GameObject>("TargetingOverlay");

            var targetingOverlay = Object.Instantiate(
                    overlayPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<TargetingOverlay>();

            targetingOverlay.Initialize(unit);

            return targetingOverlay;
        }
    }
}
