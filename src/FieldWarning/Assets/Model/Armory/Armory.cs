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
    /// <summary>
    ///     The parsed and dereferenced model containing all of our
    ///     unit data.
    /// </summary>
    public class Armory
    {
        public readonly List<Unit>[] Categories;
        public readonly Dictionary<string, Unit> Units;

        public Armory(ArmoryConfig armoryConfig)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];
            Units = new Dictionary<string, Unit>();

            foreach (string categoryKey in Enum.GetNames(typeof(UnitCategory)))
            {
                if (Regex.IsMatch(categoryKey, @"^_"))
                    break;

                int i = (int)Enum.Parse(typeof(UnitCategory), categoryKey);
                if (Categories[i] == null) Categories[i] = new List<Unit>();

                foreach (string unitId in (List<string>)armoryConfig[categoryKey])
                {
                    Unit unit = ConfigReader.ParseUnit(unitId);
                    unit.CategoryId = (byte)i;
                    unit.Id = Categories[i].Count;
                    Categories[i].Add(unit);
                    Units.Add(unitId, unit);
                }
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
