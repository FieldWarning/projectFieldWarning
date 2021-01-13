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

using PFW.Model.Match;
using PFW.Model.Armory;
using PFW.Units;
using static PFW.Constants;

namespace PFW.UI.Ingame
{
    public class BuyTransaction
    {
        private GhostPlatoonBehaviour _newestPlatoon;

        private int _smallestPlatoonSize;

        public Unit Unit { get; }
        public PlayerData Owner { get; }
        public List<GhostPlatoonBehaviour> PreviewPlatoons { get; }

        public int UnitCount {
            get {
                return _smallestPlatoonSize + (PreviewPlatoons.Count - 1) * MAX_PLATOON_SIZE;
            }
        }

        public BuyTransaction(Unit unit, PlayerData owner)
        {
            Unit = unit;
            Owner = owner;

            PreviewPlatoons = new List<GhostPlatoonBehaviour>();
            StartNewPlatoon();
        }

        public void AddUnit()
        {
            if (_smallestPlatoonSize < MAX_PLATOON_SIZE) {

                _smallestPlatoonSize++;
                _newestPlatoon.AddSingleUnit();

            } else {

                StartNewPlatoon();
            }
        }

        /// <summary>
        /// Create the smallest platoon allowed.
        /// </summary>
        private void StartNewPlatoon()
        {
            _smallestPlatoonSize = MIN_PLATOON_SIZE;
            _newestPlatoon = GhostPlatoonBehaviour.CreatePreviewMode(Unit, Owner, MIN_PLATOON_SIZE);

            PreviewPlatoons.Add(_newestPlatoon);
        }

        public BuyTransaction Clone()
        {
            BuyTransaction clone = new BuyTransaction(Unit, Owner);

            int unitCount = (PreviewPlatoons.Count - 1) * MAX_PLATOON_SIZE + _smallestPlatoonSize;

            while (unitCount-- > 1)
                clone.AddUnit();

            return clone;
        }

        /// <summary>
        /// Places the ghost units (unit silhouettes) in view of the player.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="facingPoint"></param>
        public void PreviewPurchase(Vector3 center, Vector3 facingPoint)
        {
            Vector3 diff = facingPoint - center;
            float heading = diff.getRadianAngle();

            List<Vector3> positions = Formations.GetLineFormation(
                    center, heading + Mathf.PI / 2, PreviewPlatoons.Count);
            for (int i = 0; i < PreviewPlatoons.Count; i++)
            {
                PreviewPlatoons[i].SetPositionAndOrientation(positions[i], heading);
                PreviewPlatoons[i].SetVisible(true);
            }
        }

        public void HidePreview()
        {
            for (int i = 0; i < PreviewPlatoons.Count; i++)
                PreviewPlatoons[i].SetVisible(false);
        }
    }
}
