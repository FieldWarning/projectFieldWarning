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
using PFW.UI.Prototype;
using UnityEngine;

using PFW.Model.Game;

namespace PFW.UI.Ingame
{
    public class BuyTransaction
    {
        private GhostPlatoonBehaviour _ghostPlatoonBehaviour;

        private const int MAX_PLATOON_SIZE = 4;
        private const int MIN_PLATOON_SIZE = 1;

        private int _smallestPlatoonSize;

        public UnitType UnitType { get; }
        public PlayerData Owner { get; }
        public List<GhostPlatoonBehaviour> GhostPlatoons { get; }

        public BuyTransaction(UnitType type, PlayerData owner)
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
        public void PreviewPurchase(Vector3 center, Vector3 facingPoint)
        {
            Vector3 diff = facingPoint - center;
            float heading = diff.getRadianAngle();

            var positions = Formations.GetLineFormation(center, heading + Mathf.PI / 2, GhostPlatoons.Count);
            for (var i = 0; i < GhostPlatoons.Count; i++)
                GhostPlatoons[i].SetOrientation(positions[i], heading);

        }
    }
}
