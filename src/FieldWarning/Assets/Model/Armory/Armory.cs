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
        /// Using the contents of the unit configs and their templates,
        /// finish the interpretation of the unit configs, resolving any
        /// references inside them including inheritance from the templates, and
        /// create an armory from them.
        /// </summary>
        /// <param name="configs">
        /// The unit configs that will go into the armory.
        /// </param>
        /// <param name="templateConfigs">
        /// Configs that exist to be inherited from and don't turn into units themselves.
        /// </param>
        public Armory(
                Dictionary<string, UnitConfig> configs, 
                Dictionary<string, UnitConfig> templateConfigs)
        {
            Categories = new List<Unit>[(int)UnitCategory._SIZE];
            Units = new Dictionary<string, Unit>();
            UniqueMobilityTypes = new List<MobilityData>();

            for (int i = 0; i < (int)UnitCategory._SIZE; i++)
            {
                Categories[i] = new List<Unit>();
            }

            FinalizeTemplateConfigs(ref templateConfigs);

            foreach (KeyValuePair<string, UnitConfig> pair in configs)
            {
                UnitConfig unitConfig = pair.Value;
                bool valid = unitConfig.ParsingDone(templateConfigs, false);
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

        private void FinalizeTemplateConfigs(
                ref Dictionary<string, UnitConfig> templateConfigs)
        {
            // The template configs themselves are allowed to inherit. So, 
            // here we need to take extra care in what order we call them (it is
            // unsafe to call ParsingDone() on a child config if its base config
            // has not had ParsingDone() called on it yet).
            // TODO code a neat dependency tree here instead of brute forcing..
            int retries = 10;
            var finalizedTemplateConfigs = new Dictionary<string, UnitConfig>();
            while (templateConfigs.Count > 0)
            {
                List<string> keysToRemove = new List<string>();

                foreach (KeyValuePair<string, UnitConfig> pair in templateConfigs)
                {
                    UnitConfig templateConfig = pair.Value;
                    bool result = templateConfig.ParsingDone(finalizedTemplateConfigs, true);
                    if (result)
                    {
                        finalizedTemplateConfigs.Add(pair.Key, pair.Value);
                        keysToRemove.Add(pair.Key);
                    }
                }

                // TODO With .net core 3.0 we will be able 
                // to remove from a dictionary while iterating it
                foreach (string key in keysToRemove)
                {
                    templateConfigs.Remove(key);
                }

                retries--;
                if (retries == 0 && templateConfigs.Count > 0)
                {
                    Logger.LogConfig(
                            LogLevel.ERROR,
                            "Could not resolve all template config dependencies. " +
                            "It is possible that there is a circular dependency somewhere. " +
                            "List of unresolved configs: ");
                    foreach (KeyValuePair<string, UnitConfig> pair in templateConfigs)
                    {
                        Logger.LogConfig(
                                LogLevel.ERROR,
                                $"{pair.Key}, with dependencies to {pair.Value.Inherits}");
                    }
                }
            }

            templateConfigs = finalizedTemplateConfigs;
        }

        public List<Unit> ByCategory(UnitCategory cat) => Categories[(int)cat];
    }
}
