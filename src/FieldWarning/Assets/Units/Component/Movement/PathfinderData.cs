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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

using Priority_Queue;
using EasyRoads3Dv3;

using PFW.Loading;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PFW.Units.Component.Movement
{
    public class PathfinderData : Loader
    {
        private const string GRAPH_FILE_SUFFIX = "_pathfinder_graph.dat";

        private static PathArc INVALID_ARC = new PathArc(null, null, 0);
        private const float SPARSE_GRID_SPACING = 1000f * Constants.MAP_SCALE;
        private const float ROAD_GRID_SPACING = 200f * Constants.MAP_SCALE;
        private const float NODE_PRUNE_DIST_THRESH = 10f * Constants.MAP_SCALE;
        private const float ARC_MAX_DIST = 1200f * Constants.MAP_SCALE;

        public TerrainMap _map;
        public List<PathNode> _graphFastMove;
        public List<PathNode> _graphRegularMove;
        private List<MobilityData> _mobilityTypes;

        private string _graphFile;

        /// <summary>
        /// If a node is in the open set, it has been seen
        /// but it hasn't been visited yet.
        /// </summary>
        //private FastPriorityQueue<PathNode> _openSet;

        /// <summary>
        ///     Create a pathfinder graph by either
        ///     reading it from a file or generating it from scratch.
        /// </summary>
        /// <param name="map">
        ///     A sampling of the terrain topology, 
        ///     used if generating from scratch.
        /// </param>
        public PathfinderData(
                TerrainMap map, 
                List<MobilityData> mobilityTypes, 
                int sceneBuildId)
        {
            _map = map;
            _graphFastMove = new List<PathNode>();
            _graphRegularMove = new List<PathNode>();
            _mobilityTypes = mobilityTypes;

            string scenePathWithFilename = SceneUtility.GetScenePathByBuildIndex(
                    sceneBuildId);
            string sceneName = Path.GetFileNameWithoutExtension(
                    scenePathWithFilename);
            string sceneDirectory = Path.GetDirectoryName(scenePathWithFilename);
            _graphFile = Path.Combine(sceneDirectory, sceneName + GRAPH_FILE_SUFFIX);


            // maybe turn this into a multithreaded worker later
            //if (!ReadGraph(_graphFile))
            //{

            GenerateGraphRunner();
            //}
        }

        /// <summary>
        /// Load the precomputed pathfinding graph from file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool ReadGraph(string file)
        {
            if (!File.Exists(file))
                return false;

            try
            {
                Stream stream = File.Open(file, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                _graphFastMove = (List<PathNode>)formatter.Deserialize(stream);
                stream.Close();

                //_openSet = new FastPriorityQueue<PathNode>(_graphFastMove.Count + 1);

                if (SanityCheckGraph())
                {
                    return true;
                }
                //else 
                //{
                //    _graphFastMove = null;
                //    _openSet = null;
                //    return false;
                //}
            }
            catch (Exception exception)
            {
                Debug.Log("Error reading graph file: " + exception.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check that the read pathfinding data is plausible
        /// and does not need to be generated.
        /// 
        /// This is just a sanity check and can return true even with
        /// bad graph data.
        /// </summary>
        /// <returns></returns>
        private bool SanityCheckGraph()
        {
            // The arcs in the graph need to have values for as many mobility 
            // types as there are in the unit roster, otherwise 
            // they were generated from different data.
            if (_graphFastMove[0].Arcs[0].Time.Length != _mobilityTypes.Count)
            {
                return false;
            }

            return true;
        }

        private void WriteGraph()
        {
            Stream stream = File.Open(_graphFile, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, _graphFastMove);
            stream.Close();
        }

        private void GenerateGraphRunner()
        {
            AddCouroutine(BuildRoadNodesRunner, "Creating Pathfinding road nodes");
            AddMultithreadedRoutine(BuildOpenSpaceNodes, "Building open space nodes");
            AddMultithreadedRoutine(BuildGraphRunner, "Creating the rest of Pathfinding nodes...");

            // leave just in case we decide to create cache again
            //AddMultithreadedRoutine(WriteGraph, "Writing pathfinding cache to disk");
        }

        private void BuildGraphRunner()
        {
            BuildGraph();
        }

        private IEnumerator BuildRoadNodesRunner()
        {
            yield return BuildRoadNodes();
        }

        private IEnumerator BuildRoadNodes()
        {
            Logger.LogPathfinding(LogLevel.DEBUG, $"PathfinderData::BuildRoadNodes()");
            _graphFastMove.Clear();
            // Add nodes for roads
            ERModularRoad[] roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));

            int roadsEvaluated = 0;
            foreach (ERModularRoad road in roads)
            {
                Logger.LogPathfinding(
                        LogLevel.DEBUG,
                        $"Building nodes for road {road.roadName}");
                for (int i = 0; i < road.middleIndentVecs.Count; i++)
                {
                    Vector3 roadVert = road.middleIndentVecs[i];
                    if (_map.IsInMap(roadVert))
                        _graphFastMove.Add(new PathNode(roadVert, true));

                    if (i < road.middleIndentVecs.Count - 1)
                    {
                        Vector3 nextRoadVert = road.middleIndentVecs[i + 1];
                        Vector3 stretch = nextRoadVert - roadVert;
                        int nIntermediate = (int)(stretch.magnitude / ROAD_GRID_SPACING);
                        stretch /= 1 + nIntermediate;
                        Vector3 intermediatePosition = roadVert;

                        for (int j = 0; j < nIntermediate; j++)
                        {
                            intermediatePosition += stretch;
                            if (_map.IsInMap(intermediatePosition))
                                _graphFastMove.Add(new PathNode(intermediatePosition, true));
                        }
                    }
                }


                roadsEvaluated++;
                SetPercentComplete(
                        ((double)roadsEvaluated / (double)roads.Length) * 100.0);
                yield return null;
            }
        }

        private void BuildOpenSpaceNodes()
        {
            var min = _map.MapMin;
            var max = _map.MapMax;
        

            var range = ARC_MAX_DIST - 10;

            // yea O(N^3) .. bad
            for (float x = min.x + range; x < max.x - range; x+= range)
            {
                for (float z = min.z + range; z < max.z - range; z += range)
                {
                    var pnt = new Vector3(x, _map.GetTerrainCachedHeight(new Vector3(x,z)), z);
                    var type = _map.GetTerrainType(pnt);

                    if (type == TerrainMap.PLAIN)
                    {
                        bool near_wp = false;
                        foreach (PathNode pn in _graphFastMove)
                        {
                            if ((Position(pn) - pnt).magnitude < range)
                            {
                                near_wp = true;
                            }
                        }

                        if (!near_wp)
                        {
                            _graphFastMove.Add(new PathNode(pnt, false));
                        }
                    }
                }

                double percent = ((double)x / (double)_graphFastMove.Count) * 100.0;
                SetPercentComplete(percent);
            }

            SetPercentComplete(100);
        }

        //TODO: maybe need to generate a height map file as well just for this, because it takes a long time
        private void BuildGraph()
        {
            Logger.LogPathfinding(
                    LogLevel.DEBUG,
                    $"PathfinderData.BuildGraph()");
            // TODO: Add nodes for terrain features


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
            for (int i = 0; i < _graphFastMove.Count; i++)
            {
                for (int j = i + 1; j < _graphFastMove.Count; j++)
                {
                    if ((Position(_graphFastMove[i]) - Position(_graphFastMove[j])).magnitude < NODE_PRUNE_DIST_THRESH)
                        _graphFastMove.RemoveAt(j);
                }
            }

            //_openSet = new FastPriorityQueue<PathNode>(_graphFastMove.Count + 1);


            // find all nodes that are around bridges. This will help pathfinding
            // when not in fast move mode.
            foreach (PathNode pn in _graphFastMove)
            {
                foreach (Vector3 bridge in _map.Bridges())
                {
                    Vector3 pnPos = new Vector3(pn.x,pn.y,pn.z);
                    if (pn.IsRoad && (pnPos - bridge).magnitude < ARC_MAX_DIST/2)
                    {
                        _graphRegularMove.Add(new PathNode(pnPos, true));
                    }
                }
            }

            for (int i = 0; i < _graphRegularMove.Count; i++)
            {
                for (int j = i + 1; j < _graphRegularMove.Count; j++)
                {
                    if ((Position(_graphRegularMove[i]) - Position(_graphRegularMove[j])).magnitude < ARC_MAX_DIST)
                        AddArc(_graphRegularMove[i], _graphRegularMove[j]);

                }

            }

            // Compute arcs for all pairs of nodes within cutoff distance
            // we are attempting to create a connected graph here with each edge
            // representing time/value for each mobility type at the node
            for (int i = 0; i < _graphFastMove.Count; i++)
            {
                for (int j = i + 1; j < _graphFastMove.Count; j++)
                {
                    if ((Position(_graphFastMove[i]) - Position(_graphFastMove[j])).magnitude < ARC_MAX_DIST)
                        AddArc(_graphFastMove[i], _graphFastMove[j]);

                }

                double percent = ((double)i / (double)_graphFastMove.Count) * 100.0;
                SetPercentComplete(percent);
            }




            // leave this for now in case things get a little crazy for path finding later

            // Remove unnecessary arcs.
            // An arc in necessary if for any MobilityType, the direct path
            // between the nodes is at least as good as the optimal global path.
            // This is a brute force approach and it might be too slow.
            //List<PathNode> path = new List<PathNode>();

            //for (int i = 0; i < _graph.Count; i++)
            //{
            //    for (int j = i + 1; j < _graph.Count; j++)
            //    {
            //        PathArc arc = GetArc(_graph[i], _graph[j]);
            //        if (arc.Equals(INVALID_ARC))
            //            continue;

            //        bool necessary = false;
            //        foreach (MobilityData mobility in _mobilityTypes)
            //        {
            //            if (arc.Time[mobility.Index] == Pathfinder.FOREVER)
            //                continue;

            //            float time = FindPath(path,
            //                    Position(_graph[i]), Position(_graph[j]),
            //                    mobility, 0f, MoveCommandType.FAST);



            //            if (arc.Time[mobility.Index] < 1.5 * time)
            //            {
            //                necessary = true;
            //                break;
            //            }

            //        }

            //        if (!necessary)
            //            RemoveArc(_graph[i], _graph[j]);
            //    }

            //    double percent = ((double)i / (double)_graph.Count) * 100.0;
            //    SetPercentComplete(percent);

            //if ((int)percent % 2 == 0)
            //{ 
            //    yield return null;
            //}

            //}

        }

        public List<PathNode> GetWaypointGraph()
        {
            return _graphFastMove;
        }

        public PathArc GetArc(PathNode node1, PathNode node2)
        {
            for (int i = 0; i < node1.Arcs.Count; i++)
            {
                PathArc arc = node1.Arcs[i];
                if (arc.Node1 == node2 || arc.Node2 == node2)
                    return arc;
            }
            return INVALID_ARC;
        }

        public void AddArc(PathNode node1, PathNode node2)
        {
            PathArc arc = new PathArc(node1, node2, _mobilityTypes.Count);
            node1.Arcs.Add(arc);
            node2.Arcs.Add(arc);

            // Compute the arc's traversal time for each MobilityType
            foreach (MobilityData mobility in _mobilityTypes)
            {

                arc.Time[mobility.Index] = Pathfinder.FindLocalPath(this,
                                                                    Position(node1),
                                                                    Position(node2),
                                                                    mobility,
                                                                    0f);

            }
        }

        public void RemoveArc(PathNode node1, PathNode node2)
        {
            PathArc arc = GetArc(node1, node2);
            node1.Arcs.Remove(arc);
            node2.Arcs.Remove(arc);
        }

        /// <summary>
        /// Run the A* algorithm and put the result in path.
        /// If no path was found, return 'forever' and put only the destination in path.
        /// Note: this method MUST be syncrhonized as it is called from multiple threads.
        /// Returns the total path time.
        /// </summary> 
        public float FindPath(
                List<PathNode> path,
                Vector3 start,
                Vector3 destination,
                MobilityData mobility,
                float unitRadius,
                MoveCommandType command)
        {
            lock (this)
            {

                path.Clear();
                path.Add(new PathNode(destination, false));

                PathNode cameFromDest = null;
                float gScoreDest = Pathfinder.FindLocalPath(this,
                                                            start,
                                                            destination,
                                                            mobility,
                                                            unitRadius);

                if (gScoreDest < Pathfinder.FOREVER)
                {
                    if (command == MoveCommandType.NORMAL || command == MoveCommandType.REVERSE)
                        return gScoreDest;
                }

                // Initialize with all nodes accessible from the starting point
                // (this can be optimized later by throwing out some from the start)
                //_openSet.Clear();
                FastPriorityQueue<PathNode> _openSet = new FastPriorityQueue<PathNode>(_graphRegularMove.Count + 1);

                List<PathNode> graph = _graphRegularMove;
                float neighborSearchDistance = Pathfinder.FOREVER;

                // if we are in fast mode our graph is much more extensive and we have to 
                // limit our neighor distance to use that extensive network of nodes
                if (command == MoveCommandType.FAST)
                {
                    graph = _graphFastMove;
                    neighborSearchDistance = ARC_MAX_DIST;
                    _openSet = new FastPriorityQueue<PathNode>(_graphFastMove.Count + 1);
                }

                // find the nearest neighbor start A* search
                foreach (PathNode neighbor in graph)
                {

                    neighbor.IsClosed = false;
                    neighbor.CameFrom = null;
                    neighbor.GScore = Pathfinder.FOREVER;

                    Vector3 neighborPos = Position(neighbor);

                    // and optimal search
                    if ((start - neighborPos).magnitude < neighborSearchDistance)
                    {
                        float gScoreNew = Pathfinder.FindLocalPath(this,
                                                                    start,
                                                                    neighborPos,
                                                                    mobility,
                                                                    unitRadius);
                        if (gScoreNew < Pathfinder.FOREVER)
                        {
                            neighbor.GScore = gScoreNew;

                            float fScoreNew = gScoreNew + TimeHeuristic(neighborPos, destination, mobility);
                            _openSet.Enqueue(neighbor, fScoreNew);
                        }
                    }
                }

                // generic A* algorithm based on distance to destination and arc time's as hueristic function weights
                while (_openSet.Count > 0)
                {
                    PathNode current = _openSet.Dequeue();
                    current.IsClosed = true;

                    if (gScoreDest < current.Priority)
                        break;

                    foreach (PathArc arc in current.Arcs)
                    {
                        PathNode neighbor = arc.Node1 == current ? arc.Node2 : arc.Node1;

                        if (neighbor.IsClosed)
                            continue;

                        float arcTime = arc.Time[mobility.Index];
                        if (arcTime >= Pathfinder.FOREVER)
                            continue;

                        float gScoreNew = current.GScore + arcTime;
                        if (gScoreNew >= neighbor.GScore)
                            continue;

                        float fScoreNew = gScoreNew + TimeHeuristic(Position(neighbor),
                                                                    destination,
                                                                    mobility);

                        if (!_openSet.Contains(neighbor))
                        {
                            _openSet.Enqueue(neighbor, fScoreNew);
                        }
                        else
                        {
                            _openSet.UpdatePriority(neighbor, fScoreNew);
                        }
                        neighbor.GScore = gScoreNew;
                        neighbor.CameFrom = current;
                    }

                    float arcTimeDest = Pathfinder.FOREVER;
                    // checks can we get to the last mile without more pathfinding
                    if (Vector3.Distance(Position(current), destination) < neighborSearchDistance)
                        arcTimeDest = Pathfinder.FindLocalPath(this, Position(current), destination, mobility, unitRadius);
                    // Debug.Log(openSet.Count + " " + Position(current) + " " + current.isRoad + " " + Vector3.Distance(Position(current), destination) + " " + (current.gScore + arcTimeDest) + " " + gScoreDest);
                    if (arcTimeDest >= Pathfinder.FOREVER)
                        continue;
                    if (arcTimeDest < Pathfinder.FOREVER && command == MoveCommandType.NORMAL)
                        arcTimeDest = 0f;

                    float gScoreDestNew = current.GScore + arcTimeDest;
                    if (gScoreDestNew < gScoreDest)
                    {
                        gScoreDest = gScoreDestNew;
                        cameFromDest = current;
                    }

                }

                // Reconstruct best path
                PathNode node = cameFromDest;
                while (node != null)
                {
                    path.Add(node);
                    node = node.CameFrom;
                }


                return gScoreDest;
            }
        }

        private float TimeHeuristic(Vector3 pos1, Vector3 pos2, MobilityData mobility)
        {
            return Vector3.Distance(pos1, pos2) * 3 / 4;
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
        public readonly bool IsRoad;
        public readonly List<PathArc> Arcs;
        public readonly float x, y, z;

        /// <summary>
        /// Cost from the start node to this node.
        /// </summary>
        public float GScore;

        /// <summary>
        /// A closed node is a node that has already been visited (evaluated).
        /// </summary>
        public bool IsClosed;
        public PathNode CameFrom;

        public PathNode(Vector3 position, bool isRoad)
        {
            Logger.LogPathfinding(
                    LogLevel.DUMP,
                    $"new PathNode(pos = {position}, isRoad = {isRoad})");
            //this.position = position;
            x = position.x;
            y = position.y;
            z = position.z;
            IsRoad = isRoad;
            Arcs = new List<PathArc>(4);
        }
    }

    [Serializable()]
    public struct PathArc
    {
        public PathNode Node1, Node2;

        /// <summary>
        /// All mobility data shared one path arc.
        /// This array of times has one entry for each known
        /// unique mobility data.
        /// </summary>
        public float[] Time;

        public PathArc(PathNode node1, PathNode node2, int MobilityTypesCount)
        {
            Time = new float[MobilityTypesCount];
            this.Node1 = node1;
            this.Node2 = node2;
        }
    }
}
