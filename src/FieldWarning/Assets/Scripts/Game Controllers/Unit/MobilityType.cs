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
using System.Collections.Generic;

using PFW.Units;
using PFW.Units.Component.Movement;

// The purpose of having MobilityType as a separate class from UnitData is
//     so that only a few pathfinding graphs are needed, instead of having a separate
//     one for each type of unit.
public sealed class MobilityType
{
    // This list needs to be instantiated before the PathfinderData
    public static readonly List<MobilityType> MobilityTypes = new List<MobilityType>();

    private static MobilityType _tmp = new MobilityType();

    public readonly int Index;

    // More all-terrain units like infantry should have reduced slope sensitivity
    public readonly float SlopeSensitivity;

    // A value of 0.5 means the unit will go the same speed on flat terrain as it does on a 30 degree downhill incline
    public readonly float DirectionalSlopeSensitivity;

    public readonly float PlainSpeed, ForestSpeed, WaterSpeed;

    public MobilityType()
    {
        SlopeSensitivity = 2.0f;
        DirectionalSlopeSensitivity = 0.6f;

        PlainSpeed = 0.4f;
        ForestSpeed = 0.2f;
        WaterSpeed = 0.0f;

        Index = MobilityTypes.Count;
        MobilityTypes.Insert(Index, this);
    }

    // Gives the relative speed of a unit with the given MobilityType at the given location
    // Relative speed is 0 if the terrain is impassible and 1 for road, otherwise between 0 and 1
    // If radius > 0, check for units in the way, otherwise just look at terrain
    public float GetUnitSpeed(Terrain terrain, TerrainMap map, Vector3 location, float unitRadius, Vector3 direction)
    {
        // This is a slow way to do it, and we will probably need a fast, generic method to find units within a given distance of a location
        if (unitRadius > 0f) {
            // TODO use unit list from game/match session
            // TODO maybe move this logic into its own method?
            var session = GameObject.FindObjectOfType<PFW.Model.Game.MatchSession>();

            foreach (UnitDispatcher unit in session.Units) {
                float dist = Vector3.Distance(location, unit.Transform.position);
                if (dist < unitRadius + unit.GetComponent<MovementComponent>().Data.radius)
                    return 0f;
            }
        }

        // Find unit speed on terrain
        int terrainType = map.GetTerrainType(location);

        float speed = 0f;
        if (terrainType == TerrainMap.BRIDGE) {
            speed = 1.0f;
        } else if (terrainType == TerrainMap.BUILDING) {
            speed = 0.0f;
        } else if (terrainType == TerrainMap.ROAD) {
            speed = 1.0f;
        } else if (terrainType == TerrainMap.FOREST) {
            speed = ForestSpeed;
        } else if (terrainType == TerrainMap.PLAIN) {
            speed = PlainSpeed;
        } else if (terrainType == TerrainMap.WATER) {
            speed = WaterSpeed;
        }

        if (speed <= 0)
            return 0f;

        if (terrainType == TerrainMap.BRIDGE || terrainType == TerrainMap.WATER)
            return speed;

        return speed*GetSlopeFactor(terrain, location, direction);
    }

    private float GetSlopeFactor(Terrain terrain, Vector3 location, Vector3 direction)
    {
        direction.y = 0f;
        direction.Normalize();
        direction *= 10f * TerrainConstants.MAP_SCALE;
        Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x);

        float height = terrain.SampleHeight(location);
        float forwardHeight = terrain.SampleHeight(location - direction);
        float sideHeight = terrain.SampleHeight(location + perpendicular);

        float forwardSlope = forwardHeight - height;
        float sideSlope = sideHeight - height;
        float slopeSquared = forwardSlope * forwardSlope + sideSlope * sideSlope;

        float overallSlopeFactor = SlopeSensitivity * slopeSquared;
        float directionalSlopeFactor = SlopeSensitivity * DirectionalSlopeSensitivity * forwardSlope;
        float speed = 1.0f / (1.0f + overallSlopeFactor + directionalSlopeFactor);
        return Mathf.Max(speed - 0.1f, 0f);
    }
}
