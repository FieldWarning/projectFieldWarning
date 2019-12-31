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
using PFW.Units.Component.Data;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Health;
using PFW.Units.Component.Armor;

using Mirror;

namespace PFW.Units
{
    public static class UnitFitter
    {
        static GameObject prototypeRoot;

        // TODO move this..:
        static NetworkManager networkManager;

        public static GameObject CreatePrefab(UnitConfig config)
        {
            if (prototypeRoot == null)
                prototypeRoot = new GameObject("Unit Prototypes");
            if (networkManager == null)
                networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

            //GameObject basePrefab = GameObject.Find("m1ax");
            GameObject basePrefab = Resources.Load<GameObject>(config.PrefabPath);
            GameObject prototype = GameObject.Instantiate(basePrefab, prototypeRoot.transform);
            prototype.name = config.Name;
            prototype.SetActive(false);

            //prototype.AddComponent<NetworkIdentity>();
            //networkManager.spawnPrefabs.Add(prototype);

            // TODO: if (isClient..)
            //ClientScene.RegisterPrefab(prototype);

            DataComponent.CreateDataComponent(prototype, config.Data, config.Mobility);

            prototype.AddComponent<VehicleMovementComponent>();
            prototype.AddComponent<VisionComponent>();
            prototype.AddComponent<HealthComponent>();
            prototype.AddComponent<ArmorComponent>();
            prototype.AddComponent<SelectableBehavior>();
            //prototype.AddComponent<NetworkIdentity>();

            // TODO: if (isClient..)
            //ClientScene.RegisterPrefab(prototype);

            // TODO: Load different voice type depending on Owner country
            var voicePrefab = Resources.Load<GameObject>("VoiceComponent_US");
            var voiceGo = GameObject.Instantiate(voicePrefab, prototype.transform);
            voiceGo.name = "VoiceComponent";

            return prototype;
        }
    }
}