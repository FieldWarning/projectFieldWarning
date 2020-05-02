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

        public Armory(List<UnitConfig> configs)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];
            Units = new Dictionary<string, Unit>();

            for (int i = 0; i < (int)UnitCategory._SIZE; i++)
            {
                Categories[i] = new List<Unit>();
            }

            foreach (UnitConfig unitConfig in configs)
            {
                int i = (int)Enum.Parse(
                        typeof(UnitCategory), unitConfig.CategoryKey);

                Unit unit = new Unit(unitConfig);
                unit.CategoryId = (byte)i;
                unit.Id = Categories[i].Count;
                Categories[i].Add(unit);
                Units.Add(unitConfig.ID, unit);
            }
        }

        public List<MobilityType> CalculateUniqueMobilityTypes() 
        {
            // List<MobilityType> result = new List<MobilityType>();

            foreach(Unit unit in Units.Values)
            {
                // TODO remove this static, figure out how to store the index,
                // and manage this here
                MobilityType.GetIndexForConfig(unit.Config.Mobility);
            }

            return MobilityType.MobilityTypes;
        }

        public List<Unit> ByCategory(UnitCategory cat) => Categories[(int)cat];
    }
}
