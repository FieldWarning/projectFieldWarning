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

namespace Assets.Ingame.UI {
    public class BuyTransaction {
        private readonly GhostPlatoonBehaviour _ghostPlatoonBehaviour;
        private readonly List<GhostPlatoonBehaviour> _ghostUnits;


        public UnitType UnitType { get; }
        public Player Owner { get; }

        //public event Action<GhostPlatoonBehaviour> Finished;

        //public void OnFinished(GhostPlatoonBehaviour behaviour)
        //{
        //    Finished?.Invoke(behaviour);
        //}

        public BuyTransaction(UnitType type, Player owner, List<GhostPlatoonBehaviour> ghostUnits)
        {
            UnitType = type;
            Owner = owner;

            _ghostPlatoonBehaviour = GhostPlatoonBehaviour.build(type, owner, 1);

            _ghostUnits = ghostUnits;
            _ghostUnits.Add(_ghostPlatoonBehaviour);
        }

        public void AddUnit()
        {
            _ghostPlatoonBehaviour.AddSingleUnit();
            _ghostPlatoonBehaviour.buildRealPlatoon();
        }

        public GhostPlatoonBehaviour Finish()
        {
            return _ghostPlatoonBehaviour;
        }
    }
}
