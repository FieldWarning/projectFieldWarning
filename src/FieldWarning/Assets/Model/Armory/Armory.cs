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

using PFW.Model.Armory.JsonContents;

namespace PFW.Model.Armory
{
    /// <summary>
    ///     The parsed and dereferenced model containing all of our
    ///     unit data.
    /// </summary>
    public class Armory
    {
        public readonly List<Unit>[] Categories;
        public readonly Dictionary<string, Unit> Units;

        // Separate pathfinding data is generated for each of these
        public readonly List<MobilityData> UniqueMobilityTypes;

        /// <summary>
        /// Using the contents of the unit configs,
        /// finish the interpretation of the unit configs, and
        /// create an armory from them.
        /// </summary>
        /// <param name="configs">
        /// The unit configs that will go into the armory.
        /// </param>
        public Armory(
                Dictionary<string, UnitConfig> configs)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];
            Units = new Dictionary<string, Unit>();
            UniqueMobilityTypes = new List<MobilityData>();

            for (int i = 0; i < (int)UnitCategory._SIZE; i++)
            {
                Categories[i] = new List<Unit>();
            }

            foreach (KeyValuePair<string, UnitConfig> pair in configs)
            {
                UnitConfig unitConfig = pair.Value;
                bool valid = unitConfig.ParsingDone();
                if (!valid)
                    continue;

                MobilityData mobility = null;
                foreach (MobilityData m in UniqueMobilityTypes)
                {
                    if (m.Equals(unitConfig.Mobility))
                    {
                        mobility = m;
                        break;
                    }
                }

                if (mobility == null)
                {
                    mobility = new MobilityData(
                            unitConfig.Mobility, UniqueMobilityTypes.Count);
                    UniqueMobilityTypes.Add(mobility);
                }


                int i = (int)Enum.Parse(
                        typeof(UnitCategory), unitConfig.CategoryKey);

                Unit unit = new Unit(unitConfig, mobility)
                {
                    CategoryId = (byte)i,
                    Id = Categories[i].Count
                };
                Categories[i].Add(unit);

                Units.Add(pair.Key, unit);
            }
        }

        public List<Unit> ByCategory(UnitCategory cat) => Categories[(int)cat];
    }
}
