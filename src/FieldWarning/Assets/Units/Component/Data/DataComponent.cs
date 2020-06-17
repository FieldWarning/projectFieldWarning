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

using PFW.Model.Armory.JsonContents;

namespace PFW.Units.Component.Data
{
    public class DataComponent : MonoBehaviour
    {
        // public UnitData Data;
        public float MovementSpeed;
        public float ReverseSpeed;
        public float AccelRate;
        public float MaxRotationSpeed;
        public float MinTurnRadius;
        public float MaxLateralAccel;
        public float Suspension;
        public float MaxHealth;
        public float Length;
        public float Width;
        public MobilityData MobilityData;

        public bool ApImmunity;
        public int FrontArmor;
        public int SideArmor;
        public int RearArmor;
        public int TopArmor;

        public int MaxSpottingRange;
        public float Stealth;
        public float StealthPenetration;

        // These variables are not read in from an external file
        public float Radius;
        public float OptimumTurnSpeed;
        public float SuspensionForward, SuspensionSide;
        public float AccelDampTime;

        public static DataComponent CreateDataComponent(
                GameObject parent,
                UnitConfig config,
                MobilityData mobilityData)
        {
            UnitDataConfig unitConfig = config.Data;
            MobilityConfig mobilityConfig = config.Mobility;
            ArmorConfig armorConfig = config.Armor;

            parent.AddComponent<DataComponent>();
            DataComponent c = parent.GetComponent<DataComponent>();

            c.MovementSpeed =    unitConfig.MovementSpeed      * Constants.MAP_SCALE;
            c.ReverseSpeed =     unitConfig.ReverseSpeed       * Constants.MAP_SCALE;
            c.AccelRate =        unitConfig.AccelRate          * Constants.MAP_SCALE;
            c.MaxRotationSpeed = unitConfig.MaxRotationSpeed;
            c.MinTurnRadius =    unitConfig.MinTurnRadius      * Constants.MAP_SCALE;
            c.MaxLateralAccel =  unitConfig.MaxLateralAccel    * Constants.MAP_SCALE;
            c.Suspension =       unitConfig.Suspension         / Constants.MAP_SCALE;
            c.MaxHealth =        unitConfig.MaxHealth;
            c.Length =           unitConfig.Length             * Constants.MAP_SCALE;
            c.Width =            unitConfig.Width              * Constants.MAP_SCALE;

            c.MobilityData = mobilityData;

            c.Radius = Mathf.Sqrt(c.Length * c.Width) / 2;
            c.OptimumTurnSpeed = Mathf.Sqrt(c.MaxLateralAccel * c.MinTurnRadius);

            c.SuspensionForward = c.Suspension * c.Radius / c.Length;
            c.SuspensionSide = c.Suspension * c.Radius / c.Width;

            c.AccelDampTime = 0.15f * c.MovementSpeed / c.AccelRate;

            c.ApImmunity = armorConfig.ApImmunity;
            c.FrontArmor = armorConfig.FrontArmor;
            c.SideArmor = armorConfig.SideArmor;
            c.RearArmor = armorConfig.RearArmor;
            c.TopArmor = armorConfig.TopArmor;

            ReconConfig reconConfig = config.Recon;
            c.MaxSpottingRange = reconConfig.MaxSpottingRange;
            c.Stealth = reconConfig.Stealth;
            c.StealthPenetration = reconConfig.StealthPenetration;

            return c;
        }
    }
}
