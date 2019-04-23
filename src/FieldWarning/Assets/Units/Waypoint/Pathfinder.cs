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

using PFW.Units.Component.Movement;

public class Pathfinder
{
    public const float Forever = float.MaxValue/2;
    public static Vector3 NoPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

    public const float StepSize = 12f * TerrainConstants.MAP_SCALE; // Any object that the pathfinder is able to navigate around must have at least this radius
    private const float AngSearchInc = 12f; // Angluar search increment for local path finding
    private const float MaxAngle = 85f; // Maximum turn a unit can make to either side to get around an obstacle
    private const float CompletionDist = 30f * TerrainConstants.MAP_SCALE; // Good enough if we can get within this distance of an intermediate waypoint
    private const float UpdateInterval = 0.25f;

    private static bool straightStep; // Tells if the most recent call to TakeStep gave a stright forward step

    public PathfinderData data { get; private set; }
    public MoveCommandType command { get; private set; }
    public readonly float finalCompletionDist;

    private MovementComponent unit;
    private List<PathNode> path;  // path[0] is the final destination
    private PathNode previousNode;
    private Vector3 waypoint;
    private float nextUpdateTime;

    public Pathfinder(MovementComponent unit, PathfinderData data)
    {
        this.unit = unit;
        this.data = data;
        path = new List<PathNode>();
        finalCompletionDist = 0.5f*TerrainConstants.MAP_SCALE + unit.Data.minTurnRadius;
        nextUpdateTime = 0f;
    }

    // Generate and store the sequence of nodes leading to the destination using the global graph
    // Returns the total normalized path time
    // If no path was found, return 'forever' and set no destination
    public float SetPath(Vector3 destination, MoveCommandType command)
    {
        previousNode = null;
        nextUpdateTime = 0f;

        if (destination == NoPosition) {
            path.Clear();
            return Forever;
        }

        this.command = command;

        float pathTime = data.FindPath(path, unit.transform.position, destination, unit.Data.mobility, 0f, command);
        if (pathTime >= Forever)
            path.Clear();
        return pathTime;
    }

    // Gives the next step along the previously computed path
    // For speed, this will only update the waypoint on some frames
    // Returns 'NoPosition' if there is no destination or a step cannot be found
    public Vector3 GetWaypoint()
    {
        if (!HasDestination()) { // Nowhere to go
            waypoint = NoPosition;
        } else if (Time.time > nextUpdateTime) {
            nextUpdateTime = Time.time + UpdateInterval;
            UpdateWaypoint();
        }

        if (waypoint == NoPosition)
            command = MoveCommandType.Slow;

        return waypoint;
    }

    private void UpdateWaypoint()
    {
        PathNode targetNode = path[path.Count - 1];

        float distance = Vector3.Distance(unit.transform.position, PathfinderData.Position(targetNode));
        if (distance < (path.Count > 1 ? CompletionDist : finalCompletionDist)) { // Unit arrived at the next path node
            path.RemoveAt(path.Count - 1);
            if (!HasDestination()) { // Unit arrived at the destination
                waypoint = NoPosition;
                return;
            } else {
                previousNode = targetNode;
                targetNode = path[path.Count - 1];
            }
        }

        Vector3 targetPosition = PathfinderData.Position(targetNode);
        //Debug.Log(targetNode.isRoad + (previousNode != null ? " "+previousNode.isRoad : ""));
        if (targetNode.isRoad && previousNode != null && previousNode.isRoad)
            targetPosition = GetRoadIntersection(PathfinderData.Position(targetNode), PathfinderData.Position(previousNode), unit.transform.position);

        Vector3 newWaypoint = TakeStep(
            data, unit.transform.position, targetPosition, unit.Data.mobility, unit.Data.radius);

        if (newWaypoint != NoPosition) {
            waypoint = straightStep ? targetPosition : newWaypoint;
        } else {

            // The unit has gotten stuck when following the previously computed path.
            // Now recompute a new path to the destination using the global graph, this time using finite radius
            float pathTime = data.FindPath(path, unit.transform.position, PathfinderData.Position(path[0]), unit.Data.mobility, unit.Data.radius, command);
            //float pathTime = SetPath (path[0].position, command);

            if (pathTime == Forever) {  // The unit has somehow gotten itself trapped
                Debug.Log("I am stuck!!!");
                waypoint = NoPosition;
            } else {
                // If this is an intermediate step of the path, then the pre-computed global graph might
                // be broken and the corresponding arc should be recomputed to avoid having units cycle forever
                if (previousNode != null && path.Count > 1) {
                    data.RemoveArc(previousNode, targetNode);
                    data.AddArc(previousNode, targetNode);
                }
            }

        }
    }

    private Vector3 GetRoadIntersection(Vector3 forward, Vector3 behind, Vector3 position)
    {
        Vector3 roadUnit = (forward - behind).normalized;
        Vector3 direct = forward - position;
        float projection = direct.x*roadUnit.x + direct.z*roadUnit.z;
        if (projection < 0)
            return forward;
        Vector3 parallel = roadUnit * projection;
        Vector3 normal = direct - parallel;
        //float ratio = parallel.magnitude / direct.magnitude;
        return position + 3*normal + roadUnit*Mathf.Min(0.8f*projection, 10*StepSize);
    }

    public bool HasDestination()
    {
        return path.Count > 0;
    }

    public Vector3 GetDestination()
    {
        return path.Count > 0 ? PathfinderData.Position(path[0]) : NoPosition;
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

        while (distance > CompletionDist) {
            Vector3 previous = waypoint;
            waypoint = TakeStep(data, waypoint, destination, mobility, radius);
            if (waypoint == NoPosition)
                return Forever;
            time += StepSize / mobility.GetUnitSpeed(data.terrain, data.map, waypoint, radius, (waypoint - previous).normalized);
            distance = (destination - waypoint).magnitude;
        }
        time += distance / mobility.GetUnitSpeed(data.terrain, data.map, waypoint, radius, (destination - waypoint).normalized);

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
        straightStep = false;

        // Fan out in a two-point horizontal pattern to find a way forward
        for (float ang1 = 0f; ang1 <= MaxAngle; ang1 += AngSearchInc) {

            for (int direction = -1; direction <= 1; direction += 2) {

                Vector3 direction1 = ang1 > 0f ? Quaternion.AngleAxis(ang1 * direction, Vector3.up) * straight : straight;
                Vector3 midpoint = start + direction1 * StepSize;
                float midspeed = mobility.GetUnitSpeed(data.terrain, data.map, midpoint, radius, direction1);

                if (midspeed > 0f) {
                    for (float ang2 = 0f; ang2 <= ang1; ang2 += AngSearchInc) {

                        Vector3 direction2 = ang2 > 0f ? Quaternion.AngleAxis(ang2 * direction, Vector3.up) * straight : straight;
                        Vector3 endpoint = midpoint + straight * StepSize;
                        float endspeed = mobility.GetUnitSpeed(data.terrain, data.map, endpoint, radius, direction2);

                        if (endspeed > 0f) {
                            straightStep = ang1 == 0f && ang2 == 0f;
                            return straightStep ? endpoint : midpoint;
                        }
                    }
                }

            }

        }

        // No step was found
        return NoPosition;
    }

}

