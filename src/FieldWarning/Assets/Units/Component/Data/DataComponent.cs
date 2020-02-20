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
using PFW.Units.Component.Damage;

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
        public int MobilityTypeIndex;

        // These variables are not read in from an external file
        public float Radius;
        public float OptimumTurnSpeed;
        public float SuspensionForward, SuspensionSide;
        public float AccelDampTime;
        public int[] Armor = new int[4];

        public static DataComponent CreateDataComponent(
                GameObject parent,
                UnitDataConfig config,
                MobilityConfig mobilityConfig)
        {
            parent.AddComponent<DataComponent>();
            var c = parent.GetComponent<DataComponent>();

            c.MovementSpeed =    config.MovementSpeed      * TerrainConstants.MAP_SCALE;
            c.ReverseSpeed =     config.ReverseSpeed       * TerrainConstants.MAP_SCALE;
            c.AccelRate =        config.AccelRate          * TerrainConstants.MAP_SCALE;
            c.MaxRotationSpeed = config.MaxRotationSpeed;
            c.MinTurnRadius =    config.MinTurnRadius      * TerrainConstants.MAP_SCALE;
            c.MaxLateralAccel =  config.MaxLateralAccel    * TerrainConstants.MAP_SCALE;
            c.Suspension =       config.Suspension         / TerrainConstants.MAP_SCALE;
            c.MaxHealth =        config.MaxHealth;
            c.Length =           config.Length             * TerrainConstants.MAP_SCALE;
            c.Width =            config.Width              * TerrainConstants.MAP_SCALE;

            c.MobilityTypeIndex = MobilityType.GetIndexForConfig(mobilityConfig);

            c.Radius = Mathf.Sqrt(c.Length * c.Width) / 2;
            c.OptimumTurnSpeed = Mathf.Sqrt(c.MaxLateralAccel * c.MinTurnRadius);

            c.SuspensionForward = c.Suspension * c.Radius / c.Length;
            c.SuspensionSide = c.Suspension * c.Radius / c.Width;

            c.AccelDampTime = 0.15f * c.MovementSpeed / c.AccelRate;

            return c;
        }
    }
}
