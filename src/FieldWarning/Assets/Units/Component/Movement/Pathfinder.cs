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


namespace PFW.Units.Component.Movement
{
    public class Pathfinder
    {
        public const float FOREVER = float.MaxValue / 2;
        public static readonly Vector3 NO_POSITION = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        /// <summary>
        /// Any object that the pathfinder is able to
        /// navigate around shall have at least this radius.
        /// </summary>
        public const float STEP_SIZE = 12f * TerrainConstants.MAP_SCALE;

        /// <summary>
        /// Angular search increment for local path finding.
        /// </summary>
        private const float ANGLE_SEARCH_INC = 12f;

        /// <summary>
        /// Maximum turn a unit can make to either side to get around
        /// an obstacle.
        /// </summary>
        private const float MAX_ANGLE = 85f;

        /// <summary>
        /// We want to get within this distance of any intermediate waypoints.
        /// </summary>
        private const float COMPLETION_DIST = 30f * TerrainConstants.MAP_SCALE;

        /// <summary>
        /// Throttle the frequency of waypoint recalculation, for performance.
        /// </summary>
        private const float UPDATE_INTERVAL = 0.25f;

        /// <summary>
        /// Did the most recent call to TakeStep return a straight forward step?
        /// </summary>
        private static bool _s_straightStep;

        public PathfinderData Data { get; private set; }
        public MoveCommandType Command { get; private set; }
        public readonly float FinalCompletionDist;

        private MovementComponent _unit;
        private List<PathNode> _path;  // path[0] is the final destination
        private PathNode _previousNode;
        private Vector3 _waypoint;
        private float _nextUpdateTime;

        public Pathfinder(MovementComponent unit, PathfinderData data)
        {
            _unit = unit;
            Data = data;
            _path = new List<PathNode>();
            FinalCompletionDist =
                    2f * TerrainConstants.MAP_SCALE + unit.Data.MinTurnRadius;
            _nextUpdateTime = 0f;
        }

        // Generate and store the sequence of nodes leading to the destination using the global graph
        // Returns the total normalized path time
        // If no path was found, return 'forever' and set no destination
        public float SetPath(Vector3 destination, MoveCommandType command)
        {
            _previousNode = null;
            _nextUpdateTime = 0f;

            if (destination == NO_POSITION)
            {
                _path.Clear();
                return FOREVER;
            }

            this.Command = command;

            float pathTime = Data.FindPath(
                    _path, _unit.transform.position, destination, _unit.Mobility, 0f, command);
            if (pathTime >= FOREVER)
                _path.Clear();
            return pathTime;
        }

        // Gives the next step along the previously computed path
        // For speed, this will only update the waypoint on some frames
        // Returns 'NoPosition' if there is no destination or a step cannot be found
        public Vector3 GetWaypoint()
        {
            if (!HasDestination())
            { // Nowhere to go
                _waypoint = NO_POSITION;
            }
            else if (Time.time > _nextUpdateTime)
            {
                _nextUpdateTime = Time.time + UPDATE_INTERVAL;
                UpdateWaypoint();
            }

            if (_waypoint == NO_POSITION)
                Command = MoveCommandType.NORMAL;

            return _waypoint;
        }

        private void UpdateWaypoint()
        {
            PathNode targetNode = _path[_path.Count - 1];

            float distance = Vector3.Distance(
                    _unit.transform.position, PathfinderData.Position(targetNode));
            if (distance < (_path.Count > 1 ? COMPLETION_DIST : FinalCompletionDist))
            { // Unit arrived at the next path node
                _path.RemoveAt(_path.Count - 1);
                if (!HasDestination())
                { // Unit arrived at the destination
                    _waypoint = NO_POSITION;
                    return;
                }
                else
                {
                    _previousNode = targetNode;
                    targetNode = _path[_path.Count - 1];
                }
            }

            Vector3 targetPosition = PathfinderData.Position(targetNode);
            //Debug.Log(targetNode.isRoad + (previousNode != null ? " "+previousNode.isRoad : ""));
            if (targetNode.IsRoad && _previousNode != null && _previousNode.IsRoad)
                targetPosition = GetRoadIntersection(
                        PathfinderData.Position(targetNode),
                        PathfinderData.Position(_previousNode),
                        _unit.transform.position);

            Vector3 newWaypoint = TakeStep(
                    Data, 
                    _unit.transform.position, 
                    targetPosition, 
                    _unit.Mobility, 
                    _unit.Data.Radius);

            if (newWaypoint != NO_POSITION)
            {
                _waypoint = _s_straightStep ? targetPosition : newWaypoint;
            }
            else
            {

                // The unit has gotten stuck when following the previously computed path.
                // Now recompute a new path to the destination using the global graph, this time using finite radius
                float pathTime = Data.FindPath(
                        _path, 
                        _unit.transform.position, 
                        PathfinderData.Position(_path[0]), 
                        _unit.Mobility, 
                        _unit.Data.Radius, 
                        Command);
                //float pathTime = SetPath (path[0].position, command);

                bool isTrapped = pathTime == FOREVER;
                if (isTrapped)
                {
                    Debug.Log("I am stuck!!!");
                    _waypoint = NO_POSITION;
                }
                else
                {
                    // If this is an intermediate step of the path, then the pre-computed global graph might
                    // be broken and the corresponding arc should be recomputed to avoid having units cycle forever
                    if (_previousNode != null && _path.Count > 1)
                    {
                        Data.RemoveArc(_previousNode, targetNode);
                        Data.AddArc(_previousNode, targetNode);
                    }
                }
            }
        }

        private Vector3 GetRoadIntersection(Vector3 forward, Vector3 behind, Vector3 position)
        {
            Vector3 roadUnit = (forward - behind).normalized;
            Vector3 direct = forward - position;
            float projection = direct.x * roadUnit.x + direct.z * roadUnit.z;
            if (projection < 0)
                return forward;
            Vector3 parallel = roadUnit * projection;
            Vector3 normal = direct - parallel;
            //float ratio = parallel.magnitude / direct.magnitude;
            return position + 3 * normal + roadUnit * Mathf.Min(0.8f * projection, 10 * STEP_SIZE);
        }

        public bool HasDestination()
        {
            return _path.Count > 0;
        }

        public Vector3 GetDestination()
        {
            return _path.Count > 0 ? PathfinderData.Position(_path[0]) : NO_POSITION;
        }

        // Build a path in an approximately straight line from start to destination by stringing together steps
        // This is NOT guaranteed to not get stuck in a local terrain feature
        // Returns the total normalized path time, or 'forever' if stuck
        public static float FindLocalPath(
            PathfinderData data,
            Vector3 start, Vector3 destination,
            MobilityType mobility,
            float radius)
        {
            float distance = (destination - start).magnitude;
            Vector3 waypoint = start;
            float time = 0f;

            while (distance > COMPLETION_DIST)
            {
                Vector3 previous = waypoint;
                waypoint = TakeStep(data, waypoint, destination, mobility, radius);
                if (waypoint == NO_POSITION)
                    return FOREVER;
                time += STEP_SIZE / mobility.GetUnitSpeedMultiplier(data._map, waypoint, radius, (waypoint - previous).normalized);
                distance = (destination - waypoint).magnitude;
            }
            time += distance / mobility.GetUnitSpeedMultiplier(data._map, waypoint, radius, (destination - waypoint).normalized);

            return time;
        }

        // Finds an intermediate step along the way from start to destination
        // Returns 'NoPosition' if stuck
        private static Vector3 TakeStep(
            PathfinderData data,
            Vector3 start, Vector3 destination,
            MobilityType mobility,
            float radius)
        {
            Vector3 straight = (destination - start).normalized;
            _s_straightStep = false;

            // Fan out in a two-point horizontal pattern to find a way forward
            for (float ang1 = 0f; ang1 <= MAX_ANGLE; ang1 += ANGLE_SEARCH_INC)
            {

                for (int direction = -1; direction <= 1; direction += 2)
                {

                    Vector3 direction1 = ang1 > 0f ? Quaternion.AngleAxis(ang1 * direction, Vector3.up) * straight : straight;
                    Vector3 midpoint = start + direction1 * STEP_SIZE;
                    float midspeed = mobility.GetUnitSpeedMultiplier(data._map, midpoint, radius, direction1);

                    if (midspeed > 0f)
                    {
                        for (float ang2 = 0f; ang2 <= ang1; ang2 += ANGLE_SEARCH_INC)
                        {

                            Vector3 direction2 = ang2 > 0f ? Quaternion.AngleAxis(ang2 * direction, Vector3.up) * straight : straight;
                            Vector3 endpoint = midpoint + straight * STEP_SIZE;
                            float endspeed = mobility.GetUnitSpeedMultiplier(data._map, endpoint, radius, direction2);

                            if (endspeed > 0f)
                            {
                                _s_straightStep = ang1 == 0f && ang2 == 0f;
                                return _s_straightStep ? endpoint : midpoint;
                            }
                        }
                    }
                }
            }

            // No step was found
            return NO_POSITION;
        }
    }
}