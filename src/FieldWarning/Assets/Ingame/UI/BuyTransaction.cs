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
using Pfw.Ingame.Prototype;
using UnityEngine;

using PFW.Model.Game;

namespace PFW.Ingame.UI
{
    public class BuyTransaction
    {
        private GhostPlatoonBehaviour _ghostPlatoonBehaviour;

        private const int MAX_PLATOON_SIZE = 4;
        private const int MIN_PLATOON_SIZE = 1;

        private int _smallestPlatoonSize;

        public UnitType UnitType { get; }
        public Player Owner { get; }
        public List<GhostPlatoonBehaviour> GhostPlatoons { get; }

        public BuyTransaction(UnitType type, Player owner)
        {
            UnitType = type;
            Owner = owner;

            _smallestPlatoonSize = MIN_PLATOON_SIZE;
            _ghostPlatoonBehaviour =
                GhostPlatoonBehaviour.Build(type, owner, _smallestPlatoonSize);

            GhostPlatoons = new List<GhostPlatoonBehaviour>();
            GhostPlatoons.Add(_ghostPlatoonBehaviour);
        }

        public void AddUnit()
        {
            if (_smallestPlatoonSize < MAX_PLATOON_SIZE) {

                GhostPlatoons.Remove(_ghostPlatoonBehaviour);
                _ghostPlatoonBehaviour.Destroy();

                _smallestPlatoonSize++;
                _ghostPlatoonBehaviour =
                    GhostPlatoonBehaviour.Build(UnitType, Owner, _smallestPlatoonSize);
                GhostPlatoons.Add(_ghostPlatoonBehaviour);
            } else {

                // If all platoons in the transaction are max size, we add a new one and update the size counter:
                _smallestPlatoonSize = MIN_PLATOON_SIZE;
                _ghostPlatoonBehaviour = GhostPlatoonBehaviour.Build(UnitType, Owner, _smallestPlatoonSize);
                GhostPlatoons.Add(_ghostPlatoonBehaviour);
            }
        }

        public BuyTransaction Clone()
        {
            BuyTransaction clone = new BuyTransaction(UnitType, Owner);

            int unitCount = (GhostPlatoons.Count - 1) * MAX_PLATOON_SIZE + _smallestPlatoonSize;

            while (unitCount-- > 1)
                clone.AddUnit();

            return clone;
        }

        public void Finish()
        {
            foreach (GhostPlatoonBehaviour g in GhostPlatoons) {
                g.BuildRealPlatoon();
            }
        }

        // Places the ghost units (unit silhouettes) in view of the player:
        public void PreviewPurchase(Vector3 position, Vector3 facingPoint)
        {
            Vector3 diff = facingPoint - position;
            float heading = diff.getRadianAngle();

            Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
            int formationWidth = GhostPlatoons.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
            float platoonDistance = 4 * PlatoonBehaviour.UNIT_DISTANCE;
            var right = Vector3.Cross(forward, Vector3.up);
            var pos = position + platoonDistance * (formationWidth - 1) * right / 2f;
            for (var i = 0; i < formationWidth; i++)
                GhostPlatoons[i].SetOrientation(pos - i * platoonDistance * right, heading);
        }
    }
}
