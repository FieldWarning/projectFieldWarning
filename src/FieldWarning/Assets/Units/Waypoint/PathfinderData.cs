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
using Priority_Queue;
using EasyRoads3Dv3;

public class PathfinderData
{
    private static PathArc InvalidArc = new PathArc(null, null);
    private const float SparseGridSpacing = 1000f * TerrainConstants.MAP_SCALE;
    private const float NodePruneDistThresh = 10f * TerrainConstants.MAP_SCALE;
    private const float ArcMaxDist = 2500f * TerrainConstants.MAP_SCALE;

    public Terrain terrain;
    public TerrainMap map;
    public List<PathNode> graph;
    FastPriorityQueue<PathNode> openSet;

    public PathfinderData(Terrain terrain)
    {
        this.terrain = terrain;
        map = new TerrainMap(terrain);
        graph = new List<PathNode>();
        BuildGraph();
    }

    private void BuildGraph()
    {
        graph.Clear();
        Vector3 size = terrain.terrainData.size;//TerrainBuilder.size;

        // TODO: Add nodes for terrain features

        // Add nodes for roads
        ERModularRoad[] roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        foreach (ERModularRoad road in roads) {
            foreach (Vector3 roadVert in road.middleIndentVecs) {
                if (Mathf.Abs(roadVert.x) < size.x/2 && Mathf.Abs(roadVert.z) < size.z/2)
                    graph.Add(new PathNode(roadVert));
            }
        }

        /*// Fill in any big open spaces with a sparse grid in case the above missed anything important
        Vector3 newPos = new Vector3(0f, 0f, 0f);
        for (float x = -size.x/2; x < size.x/2; x += SparseGridSpacing / 10) {
            for (float z = -size.z/2; z < size.z/2; z += SparseGridSpacing / 10) {
                newPos.Set(x, 0f, z);

                float minDist = float.MaxValue;
                foreach (PathNode node in graph)
                    minDist = Mathf.Min(minDist, Vector3.Distance(newPos, node.position));

                if (minDist > SparseGridSpacing)
                    graph.Add(new PathNode(newPos));
            }
        }*/
        
        // Remove nodes that are right on top of each other
        for (int i = 0; i < graph.Count; i++) {
            for (int j = i + 1; j < graph.Count; j++) {
                if ((graph[i].position - graph[j].position).magnitude < NodePruneDistThresh)
                    graph.RemoveAt(j);
            }
        }

        openSet = new FastPriorityQueue<PathNode>(graph.Count + 1);
        
        // Compute arcs for all pairs of nodes within cutoff distance
        for (int i = 0; i < graph.Count; i++) {
            for (int j = i + 1; j < graph.Count; j++) {
                if ((graph[i].position - graph[j].position).magnitude < ArcMaxDist) 
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
                        mobility, 0f, MoveCommandType.Fast);

                    if (arc.time[mobility.Index] < 1.05 * time) {
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
                this, node1.position, node2.position, mobility, 0f, MoveCommandType.Fast);
        }
    }

    public void RemoveArc(PathNode node1, PathNode node2)
    {
        PathArc arc = GetArc(node1, node2);
        node1.arcs.Remove(arc);
        node2.arcs.Remove(arc);
    }

    // Run the A* algorithm and put the result in path
    // If no path was found, return 'forever' and put only the destination in path
    // Returns the total path time
    public float FindPath(
        List<PathNode> path,
        Vector3 start, Vector3 destination,
        MobilityType mobility, float unitRadius,
        MoveCommandType command)
    {
        path.Clear();
        path.Add(new PathNode(destination));

        PathNode cameFromDest = null;
        float gScoreDest = Pathfinder.FindLocalPath(this, start, destination, mobility, unitRadius, command);

        if (gScoreDest < Pathfinder.Forever) {
            if (command == MoveCommandType.Slow || command == MoveCommandType.Reverse)
                return gScoreDest;
        }

        // Initialize with all nodes accessible from the starting point
        // (this can be optimized later by throwing out some from the start)
        openSet.Clear();
        foreach (PathNode neighbor in graph) {
            neighbor.isClosed = false;
            neighbor.cameFrom = null;
            neighbor.gScore = Pathfinder.Forever;
            
            if ((start - neighbor.position).magnitude < ArcMaxDist) {
                float gScoreNew = Pathfinder.FindLocalPath(this, start, neighbor.position, mobility, unitRadius, command);
                if (gScoreNew < Pathfinder.Forever) {
                    neighbor.gScore = gScoreNew;
                    float fScoreNew = gScoreNew + TimeHeuristic(neighbor.position, destination, mobility);
                    openSet.Enqueue(neighbor, fScoreNew);
                }
            }
        }

        //Debug.Log("-------------------------------------- " + graph.Count + " " + openSet.Count);
        while (openSet.Count > 0) {
            //Debug.Log(openSet.Count);

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

            float arcTimeDest = Pathfinder.FindLocalPath(
                this, current.position, destination, mobility, unitRadius, command);
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
