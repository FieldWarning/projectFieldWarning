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

using UnityEngine;

using PFW.Model.Armory;
using PFW.Units.Component.Armor;
using PFW.Units.Component.Data;
using PFW.Units.Component.Health;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Movement;


namespace PFW.Units
{
    public static class UnitFitter
    {
        /// <summary>
        ///     Builds up a unit structure with appropriate components
        ///     based on the provided config.
        ///     This can't be done before spawning the unit as prefabs
        ///     have to be avaialble at compile time for 
        ///     Mirror to be able to spawn them.
        /// </summary>
        /// <returns>A prefab that can be used to instantiate units.</returns>
        public static void Augment(GameObject freshUnit, UnitConfig config, bool isGhost)
        {
            freshUnit.name = config.Name;
            freshUnit.SetActive(false);

            DataComponent.CreateDataComponent(freshUnit, config.Data, config.Mobility);

            // freshUnit.AddComponent<UnitDispatcher>().enabled = false;
            // freshUnit.AddComponent<MovementComponent>().enabled = false;
            freshUnit.AddComponent<SelectableBehavior>();
            // prototype.AddComponent<NetworkIdentity>();

            if (isGhost) 
            {
                UnitDispatcher unitDispatcher = freshUnit.GetComponent<UnitDispatcher>();
                Object.Destroy(unitDispatcher);
            } 
            else
            {
                freshUnit.AddComponent<VisionComponent>();
                freshUnit.AddComponent<HealthComponent>();
                freshUnit.AddComponent<ArmorComponent>();

                // TODO: Load different voice type depending on Owner country
                GameObject voicePrefab = Resources.Load<GameObject>("VoiceComponent_US");
                GameObject voiceGo = Object.Instantiate(voicePrefab, freshUnit.transform);
                voiceGo.name = "VoiceComponent";
            }
        }
    }
}
