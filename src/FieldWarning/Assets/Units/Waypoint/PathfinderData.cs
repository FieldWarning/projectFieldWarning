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

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using EasyRoads3Dv3;

public class PathfinderData
{
    private const string GraphFile = "graph.dat";

    private static PathArc InvalidArc = new PathArc(null, null);
    private const float SparseGridSpacing = 1000f * TerrainConstants.MAP_SCALE;
    private const float RoadGridSpacing = 200f * TerrainConstants.MAP_SCALE;
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

        if (!ReadGraph(GraphFile)) {
            BuildGraph();
            WriteGraph(GraphFile);
        }
    }

    private bool ReadGraph(string file)
    {
        if (!File.Exists(file))
            return false;

        try {
            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            graph = (List<PathNode>)formatter.Deserialize(stream);
            stream.Close();

            openSet = new FastPriorityQueue<PathNode>(graph.Count + 1);
            return true;
        } catch (Exception exception) {
            Debug.Log("Error reading graph file: " + exception.Message);
            return false;
        }
    }

    private void WriteGraph(String file)
    {
        Stream stream = File.Open(file, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, graph);
        stream.Close();
    }

    private void BuildGraph()
    {
        graph.Clear();
        Vector3 size = terrain.terrainData.size;

        // TODO: Add nodes for terrain features

        // Add nodes for roads
        ERModularRoad[] roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        foreach (ERModularRoad road in roads) {
            for (int i = 0; i < road.middleIndentVecs.Count; i++) {
                Vector3 roadVert = road.middleIndentVecs[i];
                if (Mathf.Abs(roadVert.x) < size.x/2 && Mathf.Abs(roadVert.z) < size.z/2)
                    graph.Add(new PathNode(roadVert, true));

                if (i < road.middleIndentVecs.Count - 1) {
                    Vector3 nextRoadVert = road.middleIndentVecs[i+1];
                    Vector3 stretch = nextRoadVert - roadVert;
                    int nIntermediate = (int)(stretch.magnitude / RoadGridSpacing);
                    stretch /= 1 + nIntermediate;
                    Vector3 intermediatePosition = roadVert;
                    for (int j = 0; j < nIntermediate; j++) {
                        intermediatePosition += stretch;
                        if (Mathf.Abs(intermediatePosition.x) < size.x / 2 && Mathf.Abs(intermediatePosition.z) < size.z / 2)
                            graph.Add(new PathNode(intermediatePosition, true));
                    }
                }
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
                if ((Position(graph[i]) - Position(graph[j])).magnitude < NodePruneDistThresh)
                    graph.RemoveAt(j);
            }
        }

        openSet = new FastPriorityQueue<PathNode>(graph.Count + 1);
        
        // Compute arcs for all pairs of nodes within cutoff distance
        for (int i = 0; i < graph.Count; i++) {
            for (int j = i + 1; j < graph.Count; j++) {
                if ((Position(graph[i]) - Position(graph[j])).magnitude < ArcMaxDist) 
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
                        Position(graph[i]), Position(graph[j]),
                        mobility, 0f, MoveCommandType.Fast);

                    if (arc.time[mobility.Index] < 1.5 * time) {
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
                this, Position(node1), Position(node2), mobility, 0f);
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
        path.Add(new PathNode(destination, false));

        PathNode cameFromDest = null;
        float gScoreDest = Pathfinder.FindLocalPath(this, start, destination, mobility, unitRadius);

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
            Vector3 neighborPos = Position(neighbor);
            
            if ((start - neighborPos).magnitude < ArcMaxDist) {
                float gScoreNew = Pathfinder.FindLocalPath(this, start, neighborPos, mobility, unitRadius);
                if (gScoreNew < Pathfinder.Forever) {
                    neighbor.gScore = gScoreNew;
                    float fScoreNew = gScoreNew + TimeHeuristic(neighborPos, destination, mobility);
                    openSet.Enqueue(neighbor, fScoreNew);
                }
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

                float fScoreNew = gScoreNew + TimeHeuristic(Position(neighbor), destination, mobility);

                if (!openSet.Contains(neighbor)) {
                    openSet.Enqueue(neighbor, fScoreNew);
                } else {
                    openSet.UpdatePriority(neighbor, fScoreNew);
                }
                neighbor.gScore = gScoreNew;
                neighbor.cameFrom = current;
            }

            float arcTimeDest = Pathfinder.Forever;
            if (Vector3.Distance(Position(current), destination) < ArcMaxDist)
                arcTimeDest = Pathfinder.FindLocalPath(this, Position(current), destination, mobility, unitRadius);
           // Debug.Log(openSet.Count + " " + Position(current) + " " + current.isRoad + " " + Vector3.Distance(Position(current), destination) + " " + (current.gScore + arcTimeDest) + " " + gScoreDest);
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
        return Vector3.Distance(pos1, pos2)*3/4;
    }

    // There is no way to mark the Vector3 class as Serializable, so this is the ugly workaround
    public static Vector3 Position(PathNode node)
    {
        return new Vector3(node.x, node.y, node.z);
    }
}

[Serializable()]
public class PathNode : FastPriorityQueueNode
{
    //public readonly Vector3 position;
    public readonly bool isRoad;
    public readonly List<PathArc> arcs;
    public readonly float x, y, z;

    public float gScore;
    public bool isClosed;
    public PathNode cameFrom;

    public PathNode(Vector3 position, bool isRoad)
    {
        //this.position = position;
        x = position.x;
        y = position.y;
        z = position.z;
        this.isRoad = isRoad;
        arcs = new List<PathArc>(4);
    }
}

[Serializable()]
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
