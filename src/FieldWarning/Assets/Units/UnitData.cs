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
using PFW.Units.Component.Damage;

public class UnitData
{
    //create from xml file or something
    public float movementSpeed = 500f * TerrainConstants.MAP_SCALE;
    public float reverseSpeed = 15f * TerrainConstants.MAP_SCALE;
    public float accelRate = 13f * TerrainConstants.MAP_SCALE;
    public float maxRotationSpeed = 50f;  // Units of degrees per second
    public float minTurnRadius = 0f * TerrainConstants.MAP_SCALE;
    public float maxLateralAccel = 15f * TerrainConstants.MAP_SCALE;
    public float suspension = 0.01f / TerrainConstants.MAP_SCALE;
    public float maxHealth = 10f;
    // length and width are used for pivoting on terrain, and to define radius
    public float length = 12f * TerrainConstants.MAP_SCALE;
    public float width = 7f * TerrainConstants.MAP_SCALE;
    public MobilityType mobility;

    // These variables are not read in from an external file
    public float radius;  // Used for pathfinding and collisions
    public float optimumTurnSpeed; // The linear speed which allows for the highest turn rate
    public float suspensionForward, suspensionSide;
    public float accelDampTime;

    public struct ArmorAttributes
    {
        public float Armor;
        public DamageData.Era EraData;
    }

    public ArmorAttributes[] armorData = new ArmorAttributes[4];

    public UnitData()
    {
        mobility = MobilityType.MobilityTypes[0];

        radius = Mathf.Sqrt(length * width) / 2;
        optimumTurnSpeed = Mathf.Sqrt(maxLateralAccel*minTurnRadius);

        suspensionForward = suspension * radius / length;
        suspensionSide = suspension * radius / width;

        accelDampTime = 0.15f * movementSpeed / accelRate;
    }

    public static UnitData GenericUnit() //used in Unit Behaviour because both tanks and infantry have 10HP
    {
        var d = new UnitData();
        //d.movementSpeed = 6f;
        //d.maxRotationSpeed = 50;
        //d.maxHealth = 10f;
        return d;
    }

    public static UnitData Tank()
    {
        var d = new UnitData();
        //d.movementSpeed = 6f;
        //d.maxRotationSpeed = 50;
        return d;
    }

    public static UnitData Infantry()
    {
        var d = new UnitData();
        //d.movementSpeed = 3f;
        //d.maxRotationSpeed = 50;
        //d.weaponData.Add(new WeaponData());
        return d;
    }
}

