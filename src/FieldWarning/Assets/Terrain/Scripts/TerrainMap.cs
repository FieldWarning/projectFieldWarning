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

using EasyRoads3Dv3;
using System.IO;
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Water;

public class TerrainMap
{
    public const int PLAIN = 0;
    public const int ROAD = 1;
    public const int WATER = 2;
    public const int FOREST = 3;
    public const int BRIDGE = 4;
    public const int BUILDING = 5;

    private const float MAP_SPACING = 1.5f * TerrainConstants.MAP_SCALE;
    private const int EXTENSION = 100;

    private const float ROAD_WIDTH_MULT = 0.5f;
    private const float TREE_RADIUS = 7f * TerrainConstants.MAP_SCALE;

    public const float BRIDGE_WIDTH = 3f * TerrainConstants.MAP_SCALE;
    public const float BRIDGE_HEIGHT = 1.0f; // temporary

    public const string HEIGHT_MAP_SUFFIX = "_sample_height.dat";
    private readonly string _HEIGHT_MAP_PATH;

    private byte[,] _map;
    private int _mapSize;
    private Terrain terrain;
    private Loading _loader;

    public readonly float WATER_HEIGHT;

    private List<Vector3> _trees = new List<Vector3>();
    private GameObject[] _bridges;
    private ERModularRoad[] _roads;

    // this is only needed for map testing
    private byte[,] originalTestMap = null;

    public TerrainMap(Terrain terrain)
    {
        this.terrain = terrain;
        WaterBasic water = (WaterBasic)GameObject.FindObjectOfType(typeof(WaterBasic));
        WATER_HEIGHT = water.transform.position.y;

        _mapSize = (int)(Mathf.Max(this.terrain.terrainData.size.x, this.terrain.terrainData.size.z) / 2f / MAP_SPACING);

        string sceneName = SceneManager.GetActiveScene().name;
        string scenePathWithFilename = SceneManager.GetActiveScene().path;
        string sceneDirectory = Path.GetDirectoryName(scenePathWithFilename);
        _HEIGHT_MAP_PATH = Path.Combine(sceneDirectory, sceneName + HEIGHT_MAP_SUFFIX);

        //TODO create some debug UI to dump the map when needed
        if (!File.Exists(_HEIGHT_MAP_PATH))
        {
            WriteHeightMap(_HEIGHT_MAP_PATH);
        }

        _roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        _bridges = GameObject.FindGameObjectsWithTag("Bridge");

        // get the trees
        foreach (TreeInstance tree in terrain.terrainData.treeInstances)
        {
            Vector3 treePosition = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;
            _trees.Add(treePosition);
        }

        int nEntry = 2 * _mapSize + 2 * EXTENSION;

        // leave this commented out until we make a change and need to retest the map
        // need to run this before the worker threads start
        //originalTestMap = CreateOriginalMap();

        _map = new byte[nEntry, nEntry];

        _loader = new Loading("Terrain");
        _loader.AddWorker(LoadHeightMap, "Loading height map");
        _loader.AddWorker(LoadTrees, "Setting tree positions");
        _loader.AddWorker(LoadRoads, "Connecting roads");
        _loader.AddWorker(LoadBridges, "Loading bridges");

        // leave this commented out until we make a change and need to retest the map
        //_loader.AddWorker(MapTester);

    }

    private void MapTester()
    {
        int nEntry = 2 * _mapSize + 2 * EXTENSION;
        for (var x = 0; x < nEntry; x++)
        {
            for (var z = 0; z < nEntry; z++)
            {
                if (originalTestMap[x, z] != _map[x, z])
                {
                    Debug.Log("Map mismatch:");
                    Debug.Log(x + "," + z);
                }

            }
        }

        Debug.Log("Map tester finished!");
    }

    /// <summary>
    /// Map unit test. Compares the map from compressed to the actual terrain map
    /// </summary>
    private byte[,] CreateOriginalMap()
    {

        Debug.Log("Creating original test map.");

        int nEntry = 2 * _mapSize + 2 * EXTENSION;

        byte[,] map_original = new byte[nEntry, nEntry];

        _map = map_original;

        for (var x = 0; x < nEntry; x++)
        {
            for (var z = 0; z < nEntry; z++)
            {
                _map[x, z] = (byte)(terrain.SampleHeight(PositionOf(x, z)) > WATER_HEIGHT ? PLAIN : WATER);

            }
        }

        LoadTrees();
        LoadRoads();
        LoadBridges();

        // assign the original back
        map_original = _map;

        Debug.Log("Done creating original test map.");

        return map_original;
    }

    /// <summary>
    /// Takes the sampled height from the terrain and packs/compresses it to a binary file with the format of:
    /// <height><number_of_times_it_occurs_consecutively>["\n"]
    /// <4bytes><4bytes><4bytes>
    /// This can possibly be compressed more by creating a range of height to be included when it writes
    /// the number of times the height occurs. For example, if height is 1.5 and 1.7 and our range is +- .2.. we
    /// would combine 1.5 and 1.7 to be 1.5 because they are so close together.
    ///
    /// </summary>
    /// <param name="path"></param>
    public void WriteHeightMap(string path)
    {

        int nEntry = 2 * _mapSize + 2 * EXTENSION;
        BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        for (int x = 0; x < nEntry; x++)
        {
            float temp = 0;
            float last = 0;
            int lastcnt = 0;

            for (int z = 0; z < nEntry; z++)
            {
                temp = terrain.SampleHeight(PositionOf(x, z));

                //map[x, z] = (byte)(terrain.SampleHeight(PositionOf(x, z)) > waterHeight ? PLAIN : WATER);

                if (last == temp || lastcnt == 0)
                {
                    lastcnt++;
                }
                else
                {
                    writer.Write(last);
                    writer.Write(lastcnt);

                    lastcnt = 1;
                }

                last = temp;
            }

            writer.Write(temp);
            writer.Write(lastcnt);


            writer.Write((int)'\n');
        }

        writer.Close();

    }

    /// <summary>
    /// This unpacks/uncomporesses the binary height map.
    /// The compression and structure of this height map is simple.
    /// Every height value is stored as a fload with a corresponding number of times it appears with
    /// maybe a newline to designate its now a different x coordinate.
    ///
    /// <height><number_of_times_it_occurs_consecutively>["\n"]
    /// <4bytes><4bytes><4bytes>
    ///
    /// Everything is 4 bytes for simplicity, including the newline.
    ///
    /// </summary>
    /// <param name="path"></param>
    public void ReadHeightMap(string path)
    {
        //TODO : not much error checking is done in thsi function

        int nEntry = 2 * _mapSize + 2 * EXTENSION;

        // read the entire file into memory
        var file = File.ReadAllBytes(path);

        //var last_notify_msec = 0;
        int xCoord = 0;
        int zCoord = 0;

        BinaryReader reader = new BinaryReader(new MemoryStream(file));

        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {

            // the sampled height of terrain (4 bytes float) or a newline
            byte[] heightOrNL = reader.ReadBytes(4);

            // check to see if this is height or newline
            if (BitConverter.ToInt32(heightOrNL, 0) == (int)0x0a)
            {
                zCoord = 0;
                xCoord++;
            }
            else
            {

                // since we already read a byte but we need 4 bytes
                int numOfValues = reader.ReadInt32();

                // convert this height into a terrain type.. water, forest etc - byte
                var bType = (byte)(BitConverter.ToSingle(heightOrNL, 0) > WATER_HEIGHT ? PLAIN : WATER);

                // this tells us how far to unpack the compression
                var zEnd = zCoord + numOfValues;

                // populate the rest of the same type
                while (zCoord < zEnd)
                {
                    _map[xCoord, zCoord] = bType;
                    zCoord++;
                }
            }

            if (_loader != null)
                // this is our loading screen status
                _loader.PercentDone = ((double)reader.BaseStream.Position / (double)reader.BaseStream.Length) * 100.0;
        }

        reader.Close();
    }


    private void LoadTrees()
    {
        // assign tree positions
        var currIdx = 0;

        foreach (var tree in _trees)
        {
            AssignCircularPatch(tree, TREE_RADIUS, FOREST);
            currIdx++;
            if (_loader != null)
                _loader.PercentDone = ((double)currIdx / (double)_trees.Count) * 100.0;
        }
    }

    private void LoadRoads()
    {
        var currRoadIdx = 0;
        foreach (ERModularRoad road in _roads)
        {
            var currRoadVertIdx = 1;
            // Loop over linear road stretches
            Vector3 previousVert = Vector3.zero;
            foreach (Vector3 roadVert in road.middleIndentVecs)
            {
                if (previousVert != Vector3.zero)
                    AssignRectanglarPatch(previousVert, roadVert, ROAD_WIDTH_MULT * road.roadWidth, ROAD);
                previousVert = roadVert;
                currRoadVertIdx++;
                if (_loader != null)
                    _loader.PercentDone =  ((currRoadVertIdx / road.middleIndentVecs.Count) * 100) * (currRoadIdx / _roads.Length);
            }

            currRoadIdx++;
        }
    }

    private void LoadBridges()
    {
        for (int i = 0; i < _bridges.Length; i++)
        {
            GameObject bridge = _bridges[i];

            // Bridge starts and ends at the two closest road nodes
            Vector3 start = Vector3.zero;
            float startDist = float.MaxValue;
            foreach (ERModularRoad road in _roads)
            {
                foreach (Vector3 roadVert in road.middleIndentVecs)
                {
                    float dist = (roadVert - bridge.transform.position).magnitude;
                    if (dist < startDist)
                    {
                        startDist = dist;
                        start = roadVert;
                    }
                }
            }


            Vector3 end = Vector3.zero;
            float endDist = float.MaxValue;
            foreach (ERModularRoad road in _roads)
            {
                foreach (Vector3 roadVert in road.middleIndentVecs)
                {
                    float dist = (roadVert - bridge.transform.position).magnitude;
                    if (roadVert != start && dist < endDist)
                    {
                        endDist = dist;
                        end = roadVert;
                    }
                }
            }

            float boundaryWidth = BRIDGE_WIDTH + Pathfinder.STEP_SIZE;
            Vector3 inset = (boundaryWidth + MAP_SPACING) * (end - start).normalized;
            AssignRectanglarPatch(start + inset, end - inset, boundaryWidth, BUILDING);
            AssignRectanglarPatch(start, end, BRIDGE_WIDTH, BRIDGE);
        }
    }

    private void LoadHeightMap()
    {
        ReadHeightMap(_HEIGHT_MAP_PATH);
    }

    private void AssignRectanglarPatch(Vector3 start, Vector3 end, float width, byte value)
    {
        float stretch = (end - start).magnitude;
        Vector3 directionLong = (end - start).normalized;
        Vector3 directionWide = new Vector3(-directionLong.z, 0f, directionLong.x);

        // Step along length of patch
        float distLong = 0f;
        while (distLong < stretch) {
            Vector3 positionLong = start + distLong * directionLong;

            // Step along width of patch
            int nPointWide = (int)(width / (MAP_SPACING / 2));
            for (int iWidth = -nPointWide; iWidth <= nPointWide; iWidth++) {
                Vector3 position = positionLong + iWidth * (MAP_SPACING / 2) * directionWide;
                int indexX = MapIndex(position.x);
                int indexZ = MapIndex(position.z);
                if (indexX >= 0 && indexX < _map.Length && indexZ >= 0 && indexZ < _map.Length)
                    _map[MapIndex(position.x), MapIndex(position.z)] = value;
            }

            distLong += MAP_SPACING / 2;
        }
    }

    private void AssignCircularPatch(Vector3 position, float radius, byte value)
    {
        for (float x = -radius; x < radius; x += MAP_SPACING / 2) {
            for (float z = -radius; z < radius; z += MAP_SPACING / 2) {
                if (Mathf.Sqrt(x*x + z*z) < radius)
                    _map[MapIndex(position.x + x), MapIndex(position.z + z)] = value;
            }
        }
    }

    private Vector3 PositionOf(int x, int z)
    {
        return MAP_SPACING * new Vector3(x - EXTENSION + 0.5f - _mapSize, 0f, z - EXTENSION + 0.5f - _mapSize);
    }

    private int MapIndex(float position)
    {
        int index = (int)(position / MAP_SPACING) + _mapSize + EXTENSION;
        return index;
    }

    public int GetTerrainType(Vector3 position)
    {
        return _map[MapIndex(position.x), MapIndex(position.z)];
    }

    public float GetTerrainHeight(Vector3 position, int type)
    {
        if (type == WATER) {
            return WATER_HEIGHT;
        }else if (type == BRIDGE) {
            return BRIDGE_HEIGHT;
        } else {
            return terrain.SampleHeight(position);
        }
    }

}
