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

using PFW.Model.Armory.JsonContents;

namespace PFW.Model.Armory
{
    public class Deck
    {
        public List<Unit>[] Categories;

        public Deck(DeckConfig deckConfig, Armory armory)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];

            for (int i = 0; i < (int)UnitCategory._SIZE; i++)
            {
                Categories[i] = new List<Unit>();
            }

            foreach (string unitId in deckConfig.UnitIds)
            {
                bool exists = armory.Units.TryGetValue(unitId, out Unit unit);
                if (!exists)
                {
                    Logger.LogConfig(LogLevel.ERROR, $"deck refers to non-existent unit {unitId}");
                }

                Categories[unit.CategoryId].Add(unit);
            }
        }

        public List<Unit> ByCategory(UnitCategory cat) => Categories[(int)cat];
    }
}
