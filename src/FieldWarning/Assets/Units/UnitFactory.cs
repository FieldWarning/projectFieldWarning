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
using PFW.Units;
using PFW.Units.Component.Data;
using PFW.Units.Component.Weapon;
using static PFW.Util;

namespace PFW.UI.Prototype
{
    public class UnitFactory
    {
        /// <summary>
        ///     After a unit is spawned in a basic state that mirror
        ///     can transmit, augment it with 'art'
        ///     (aka non-networking components) based on the config.
        ///     This can't be done before spawning the unit as prefabs
        ///     have to be avaialble at compile time for 
        ///     Mirror to be able to spawn them.
        ///     Assumption: The config is the same for all clients.
        /// </summary>
        public void MakeUnit(Unit armoryUnit, GameObject unit, PlatoonBehaviour platoon)
        {
            MakeUnitCommon(unit, armoryUnit);

            // TODO: Load different voice type depending on Owner country
            GameObject voicePrefab = Resources.Load<GameObject>("VoiceComponent_US");
            GameObject voiceGo = Object.Instantiate(voicePrefab, unit.transform);
            voiceGo.name = "VoiceComponent";

            UnitDispatcher unitDispatcher =
                    unit.GetComponent<UnitDispatcher>();
            unitDispatcher.Initialize(platoon);
            unitDispatcher.enabled = true;
        }

        public void MakeGhostUnit(Unit armoryUnit, GameObject unit)
        {
            MakeUnitCommon(unit, armoryUnit);

            Object.Destroy(unit.GetComponent<UnitDispatcher>());
            Object.Destroy(unit.GetComponent<TurretSystem>());

            unit.SetActive(true);
            unit.name = "Ghost" + unit.name;

            Shader shader = Resources.Load<Shader>("Ghost");
            unit.ApplyShaderRecursively(shader);
            unit.transform.position = 100 * Vector3.down;
        }

        /// <summary>
        ///     Unit initialization shared by both real units and their ghosts.
        /// </summary>
        /// <param name="freshUnit"></param>
        /// <param name="config"></param>
        private static void MakeUnitCommon(GameObject freshUnit, Unit armoryUnit)
        {
            freshUnit.name = armoryUnit.Name;
            freshUnit.SetActive(false);

            GameObject art = Object.Instantiate(armoryUnit.ArtPrefab);
            art.transform.parent = freshUnit.transform;

            DataComponent.CreateDataComponent(
                    freshUnit, armoryUnit.Config, armoryUnit.MobilityData);

            // freshUnit.AddComponent<UnitDispatcher>().enabled = false;
            // freshUnit.AddComponent<MovementComponent>().enabled = false;
            // prototype.AddComponent<NetworkIdentity>();

            TurretSystem turretSystem = freshUnit.GetComponent<TurretSystem>();
            turretSystem.Initialize(freshUnit, armoryUnit);
        }
    }
}
