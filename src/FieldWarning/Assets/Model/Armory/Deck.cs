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

namespace PFW.Model.Armory
{
    public class Deck
    {
        public List<Unit>[] Units;

        public Deck()
        {
            Units = new List<Unit>[(int)UnitCategory._SIZE];
            for (int i = 0; i < (int)UnitCategory._SIZE; i++) {
                Units[i] = new List<Unit>();
            }

            Units[(int)UnitCategory.LOG].Add(
                    new Unit("HEMIT", 20, Resources.Load<GameObject>("Tank")));
            Units[(int)UnitCategory.INF].Add(
                    new Unit("Riflemen", 20, Resources.Load<GameObject>("Tank")));
            Units[(int)UnitCategory.INF].Add(
                    new Unit("Marines", 20, Resources.Load<GameObject>("AFV")));
            Units[(int)UnitCategory.SUP].Add(
                    new Unit("PLZ-5", 20, Resources.Load<GameObject>("Arty")));

            Units[(int)UnitCategory.TNK].Add(
                    new Unit("M1A2 Abrams", 20, Resources.Load<GameObject>("Tank")));
            Units[(int)UnitCategory.TNK].Add(
                    new Unit("M1A1 Abrams", 5, Resources.Load<GameObject>("Tank")));

            Units[(int)UnitCategory.REC].Add(
                    new Unit("Army Rangers", 20, Resources.Load<GameObject>("Tank")));

            Units[(int)UnitCategory.SUP].Add(
                    new Unit("ARTY", 500, Resources.Load<GameObject>("Arty")));

            Units[(int)UnitCategory.HEL].Add(
                    new Unit("AH-64D Apache", 20, Resources.Load<GameObject>("Tank")));
        }

        public List<Unit> ByCategory(UnitCategory cat) => Units[(int)cat];
    }
}
