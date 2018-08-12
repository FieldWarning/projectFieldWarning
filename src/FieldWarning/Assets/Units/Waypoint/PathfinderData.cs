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
using System;
using System.Collections.Generic;
using Priority_Queue;

public class PathfinderData
{
    public static PathfinderData singleton;
    
    private static PathArc InvalidArc = new PathArc(null, null);
    private const float GraphRadius = 0f;
    private const float SparseGridSpacing = 150f;

    public Terrain terrain;
    public List<PathNode> graph;
    FastPriorityQueue<PathNode> openSet;

    public PathfinderData(Terrain terrain)
    {
        this.terrain = terrain;
        graph = new List<PathNode>();
        BuildGraph();
    }

    private void BuildGraph()
    {
        graph.Clear();

        // TODO: Add nodes for terrain features and roads


        // Fill in any big open spaces with a sparse grid in case the above missed anything important
        Vector3 size = TerrainBuilder.size;
        Vector3 newPos = new Vector3(0f, 0f, 0f);
        for (float x = 0f; x < size.x; x += SparseGridSpacing / 10) {
            for (float z = 0f; z < size.z; z += SparseGridSpacing / 10) {
                newPos.Set(x, 0f, z);

                float minDist = float.MaxValue;
                foreach (PathNode node in graph)
                    minDist = Mathf.Min(minDist, Vector3.Distance(newPos, node.position));

                if (minDist > SparseGridSpacing)
                    graph.Add(new PathNode(newPos));
            }
        }

        openSet = new FastPriorityQueue<PathNode>(graph.Count + 1);

        // Compute arcs for all pairs of nodes
        for (int i = 0; i < graph.Count; i++) {
            for (int j = i + 1; j < graph.Count; j++) {
                AddArc(graph[i], graph[j]);
            }
        }

        // Remove unnecessary arcs
        // An arc in necessary if for any MobilityType, the direct path between the nodes is at least
        // as good as the optimal global path. This is a brute force approach and it might be too slow
        List<PathNode> path = new List<PathNode>();
        for (int i = 0; i < graph.Count; i++) {
            for (int j = i + 1; j < graph.Count; j++) {

                PathArc arc = GetArc(graph[i], graph[j]);
                if (arc.Equals(InvalidArc))
                    continue;

                bool necessary = false;
                foreach (MobilityType mobility in MobilityType.MobilityTypes) {
                    if (arc.time[mobility.Index] == Pathfinder.Forever)
                        continue;

                    float time = FindPath(path,
                        graph[i].position, graph[j].position,
                        mobility, GraphRadius, MoveCommandType.Fast);
                    if (arc.time[mobility.Index] < 1.1 * time) {
                        necessary = true;
                        break;
                    }
                }

                if (!necessary)
                    RemoveArc(graph[i], graph[j]);
            }
        }
    }

    public PathArc GetArc(PathNode node1, PathNode node2)
    {
        for (int i = 0; i < node1.arcs.Count; i++) {
            PathArc arc = node1.arcs[i];
            if (arc.node1 == node2 || arc.node2 == node2)
                return arc;
        }
        return InvalidArc;
    }

    public void AddArc(PathNode node1, PathNode node2)
    {
        PathArc arc = new PathArc(node1, node2);
        node1.arcs.Add(arc);
        node2.arcs.Add(arc);

        // Compute the arc's traversal time for each MobilityType
        foreach (MobilityType mobility in MobilityType.MobilityTypes) {
            arc.time[mobility.Index] = Pathfinder.FindLocalPath(
                this, node1.position, node2.position, mobility, GraphRadius);
        }
    }

    public void RemoveArc(PathNode node1, PathNode node2)
    {
        PathArc arc = GetArc(node1, node2);
        node1.arcs.Remove(arc);
        node2.arcs.Remove(arc);
    }

    // Gives the relative speed of a unit with the given MobilityType at the given location
    // Relative speed is 0 if the terrain is impassible and 1 for road, otherwise between 0 and 1
    // If radius > 0, check for units in the way, otherwise just look at terrain
    public float GetUnitSpeed(MobilityType mobility, Vector3 location, float radius, Vector3 direction)
    {
        // This is a slow way to do it, and we will probably need a fast, generic method to find units within a given distance of a location
        if (radius > 0f) {
            // TODO use unit list from game/match session
            // TODO maybe move this logic into its own method?
            GameObject[] units = GameObject.FindGameObjectsWithTag(UnitBehaviour.UNIT_TAG);
            foreach (GameObject unit in units) {
                float dist = Vector3.Distance(location, unit.transform.position);
                if (dist < radius + unit.GetComponent<UnitBehaviour>().Data.radius)
                    return 0f;
            }
        }

        // Find unit speed on terrain
        // TODO: Make this also depend on the terrain type, not just elevation

        direction.y = 0f;
        direction.Normalize();
        Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x);

        float height = terrain.SampleHeight(location);
        float forwardHeight = terrain.SampleHeight(location - direction);
        float sideHeight = terrain.SampleHeight(location + perpendicular);

        float forwardSlope = forwardHeight - height;
        float sideSlope = sideHeight - height;
        float slopeSquared = forwardSlope*forwardSlope + sideSlope*sideSlope;

        //if (Time.frameCount%100 == 50) Debug.Log(terrain.terrainData.GetInterpolatedNormal(location.x, location.y));

        float overallSlopeFactor = mobility.SlopeSensitivity * slopeSquared;
        float directionalSlopeFactor = mobility.SlopeSensitivity * mobility.DirectionalSlopeSensitivity * forwardSlope;
        float speed = 1.0f / (1.0f + overallSlopeFactor + directionalSlopeFactor);
        speed = Mathf.Max(speed - 0.1f, 0f);

        return speed;
    }

    // Run the A* algorithm and put the result in path
    // If no path was found, return 'forever' and put only the destination in path
    // Returns the total path time
    public float FindPath(
        List<PathNode> path,
        Vector3 start, Vector3 destination,
        MobilityType mobility, float radius,
        MoveCommandType command)
    {
        path.Clear();
        path.Add(new PathNode(destination));

        PathNode cameFromDest = null;
        float gScoreDest = Pathfinder.FindLocalPath(this, start, destination, mobility, radius);

        if (command == MoveCommandType.Slow && gScoreDest < Pathfinder.Forever)
            return gScoreDest;

        // Initialize with all nodes accessible from the starting point
        // (this can be optimized later by throwing out some from the start)
        openSet.Clear();
        foreach (PathNode neighbor in graph) {
            neighbor.isClosed = false;
            neighbor.cameFrom = null;
            neighbor.gScore = Pathfinder.Forever;

            float gScoreNew = Pathfinder.FindLocalPath(this, start, neighbor.position, mobility, radius);
            if (gScoreNew < Pathfinder.Forever) {
                neighbor.gScore = gScoreNew;
                float fScoreNew = gScoreNew + TimeHeuristic(neighbor.position, destination, mobility);
                openSet.Enqueue(neighbor, fScoreNew);
            }
        }

        while (openSet.Count > 0) {

            PathNode current = openSet.Dequeue();
            current.isClosed = true;

            if (gScoreDest < current.Priority)
                break;

            foreach (PathArc arc in current.arcs) {
                PathNode neighbor = arc.node1 == current ? arc.node2 : arc.node1;

                if (neighbor.isClosed)
                    continue;

                float arcTime = arc.time[mobility.Index];
                if (arcTime >= Pathfinder.Forever)
                    continue;

                float gScoreNew = current.gScore + arcTime;
                if (gScoreNew >= neighbor.gScore)
                    continue;

                float fScoreNew = gScoreNew + TimeHeuristic(neighbor.position, destination, mobility);

                if (!openSet.Contains(neighbor)) {
                    openSet.Enqueue(neighbor, fScoreNew);
                } else {
                    openSet.UpdatePriority(neighbor, fScoreNew);
                    neighbor.gScore = gScoreNew;
                    neighbor.cameFrom = current;
                }
            }

            float arcTimeDest = Pathfinder.FindLocalPath(this, current.position, destination, mobility, radius);
            if (arcTimeDest >= Pathfinder.Forever)
                continue;
            if (arcTimeDest < Pathfinder.Forever && command == MoveCommandType.Slow)
                arcTimeDest = 0f;

            float gScoreDestNew = current.gScore + arcTimeDest;
            if (gScoreDestNew < gScoreDest) {
                gScoreDest = gScoreDestNew;
                cameFromDest = current;
            }

        }

        // Reconstruct best path
        PathNode node = cameFromDest;
        while (node != null) {
            path.Add(node);
            node = node.cameFrom;
        }
        return gScoreDest;
    }

    private float TimeHeuristic(Vector3 pos1, Vector3 pos2, MobilityType mobility)
    {
        return Vector3.Distance(pos1, pos2);
    }

}

public class PathNode : FastPriorityQueueNode
{
    public Vector3 position;
    public List<PathArc> arcs;

    public float gScore;
    public bool isClosed;
    public PathNode cameFrom;

    public PathNode(Vector3 position)
    {
        this.position = position;
        arcs = new List<PathArc>(4);
    }
}

public struct PathArc
{
    public PathNode node1, node2;
    public float[] time;

    public PathArc(PathNode node1, PathNode node2)
    {
        time = new float[MobilityType.MobilityTypes.Count];
        this.node1 = node1;
        this.node2 = node2;
    }
}
