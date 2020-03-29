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

            Color minimapColor = platoon.Owner.Team.Color;
            AddMinimapIcon(unit, minimapColor);

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

        private void AddMinimapIcon(GameObject unit, Color minimapColor)
        {
            GameObject minimapIcon = Object.Instantiate(
                    Resources.Load<GameObject>("MiniMapIcon"));
            minimapIcon.GetComponent<SpriteRenderer>().color = minimapColor;
            minimapIcon.transform.parent = unit.transform;
            // The icon is placed slightly above ground to prevent flickering
            minimapIcon.transform.localPosition = new Vector3(0,0.01f,0);
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
                    freshUnit, armoryUnit.Config);

            // freshUnit.AddComponent<UnitDispatcher>().enabled = false;
            // freshUnit.AddComponent<MovementComponent>().enabled = false;
            freshUnit.AddComponent<SelectableBehavior>();
            // prototype.AddComponent<NetworkIdentity>();

            if (armoryUnit.Prefab.name == "2Turret")
            {
                AssociateTurretComponentsToArt(freshUnit, armoryUnit);
            }
            else
            {
                TurretSystem turretSystem = freshUnit.GetComponent<TurretSystem>();
                turretSystem.Initialize(freshUnit, armoryUnit);
            }
        }

        private static GameObject _shotEmitterResource;
        private static GameObject _muzzleFlashResource;

        /// <summary>
        /// The turret and targeting components on the base prefab
        /// need to be given references to the parts of the art that
        /// they can rotate.
        /// </summary>
        private static void AssociateTurretComponentsToArt(
                GameObject freshUnit, Unit armoryUnit) 
        {
            TargetingComponent[] targetingComponents = freshUnit.GetComponents<TargetingComponent>();
            TurretComponent[] turretComponents = freshUnit.GetComponents<TurretComponent>();

            TurretConfig turretConfig = armoryUnit.Config.Turrets[0];
            // TODO Currently only supporting the prefab with 
            // 2 turrets, 2 targeting components, and one parent turret
            if (targetingComponents.GetLength(0) == 2 && turretComponents.GetLength(0) == 3)
            {
                // Use the extra turret as the parent:
                TurretComponent toplevelTurret = turretComponents[2];
                toplevelTurret.Initialize(
                        RecursiveFindChild(freshUnit.transform, turretConfig.MountRef),
                        RecursiveFindChild(freshUnit.transform, turretConfig.TurretRef),
                        null,
                        turretConfig.ArcHorizontal,
                        turretConfig.ArcUp,
                        turretConfig.ArcDown,
                        turretConfig.RotationRate,
                        false);

                for (int i = 0; i < targetingComponents.GetLength(0); i++)
                {
                    // Hack: The old tank prefab has a particle system for shooting that we want to remove,
                    // so instead of adding it to the models or having it in the config 
                    // we hardcode it in here.
                    // TODO might have to use a different object for the old arty effect.
                    if (!_shotEmitterResource)
                    {
                        _shotEmitterResource = Resources.Load<GameObject>("shot_emitter");
                    }
                    if (!_muzzleFlashResource)
                    {
                        _muzzleFlashResource = Resources.Load<GameObject>("muzzle_flash");
                    }

                    Transform turretArt = RecursiveFindChild(
                            freshUnit.transform, turretConfig.Children[i].TurretRef);

                    GameObject shotGO = GameObject.Instantiate(_shotEmitterResource, turretArt);
                    GameObject muzzleFlashGO = GameObject.Instantiate(_muzzleFlashResource, turretArt);

                    targetingComponents[i].Initialize(
                            turretComponents[i],
                            turretConfig.Children[i],
                            shotGO.GetComponent<ParticleSystem>(),
                            muzzleFlashGO.GetComponent<ParticleSystem>()
                            );
                    turretComponents[i].Initialize(
                            RecursiveFindChild(freshUnit.transform, turretConfig.Children[i].MountRef),
                            turretArt,
                            toplevelTurret,
                            turretConfig.Children[i].ArcHorizontal,
                            turretConfig.Children[i].ArcUp,
                            turretConfig.Children[i].ArcDown,
                            turretConfig.Children[i].RotationRate,
                            false);
                }
            }
        }
    }
}
