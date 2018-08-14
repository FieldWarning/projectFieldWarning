﻿/**
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

using System.Collections.Generic;
using PFW.Weapons;
using UnityEngine;

public class UnitData
{
    //create from xml file or something
    public float movementSpeed = 6f;
    public float accelRate = 1.0f;
    public float maxRotationSpeed = 50f;  // Units of degrees per second
    public float minTurnRadius = 0f;
    public float maxLateralAccel = 1.5f;
    public float maxHealth = 10f;
    public List<WeaponData> weaponData;
    public float length = 1.2f; // length and width are used for pivoting on terrain, and to define radius
    public float width = 0.75f;
    public MobilityType mobility;

    // These variables are not read in from an external file
    public float radius;  // Used for pathfinding and collisions
    public float optimumTurnSpeed; // The linear speed which allows for the highest turn rate

    public UnitData()
    {
        weaponData = new List<WeaponData>();
        mobility = MobilityType.MobilityTypes[0];

        radius = Mathf.Sqrt(length * width) / 2;
        optimumTurnSpeed = Mathf.Sqrt(maxLateralAccel*minTurnRadius);
    }

    public static UnitData GenericUnit() //used in Unit Behaviour because both tanks and infantry have 10HP
    {
        var d = new UnitData();
        d.movementSpeed = 6f;
        d.maxRotationSpeed = 50;
        d.maxHealth = 10f;
        d.weaponData.Add(new WeaponData());
        return d;
    }

    public static UnitData Tank()
    {
        var d = new UnitData();
        d.movementSpeed = 6f;
        d.maxRotationSpeed = 50;
        d.weaponData.Add(new WeaponData(200, 2, 8, 1, 30)); //will use tanks for the damage tests
        d.weaponData.Add(new WeaponData(20, 0, 1.5f, 1, 40)); // minigun
        return d;
    }

    public static UnitData Infantry()
    {
        var d = new UnitData();
        d.movementSpeed = 3f;
        d.maxRotationSpeed = 50;
        d.weaponData.Add(new WeaponData());
        return d;
    }
}

/*public enum MobilityType
{
    Inf,
    InfAmphib,
    Wheel,
    WheelAmphib,
    Track,
    TrackAmphib
};*/

