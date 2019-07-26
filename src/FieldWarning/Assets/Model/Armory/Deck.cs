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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace PFW.Model.Armory
{
    public class Deck
    {
        public List<Unit>[] Categories;

        public Deck(DeckConfig deckConfig)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];

            foreach (string categoryKey in Enum.GetNames(typeof(UnitCategory))) {
                if (Regex.IsMatch(categoryKey, @"^_"))
                    break;

                int i = (int) Enum.Parse(typeof(UnitCategory), categoryKey);
                if (Categories[i] == null) Categories[i] = new List<Unit>();

                foreach (string unitId in (List<string>) deckConfig[categoryKey]) {
                    UnitConfig unitConfig = ConfigReader.FindUnitConfig(unitId);
                    Categories[i].Add(new Unit(unitConfig));
                }
            }
        }

        public List<Unit> ByCategory(UnitCategory cat) => Categories[(int)cat];
    }
}
