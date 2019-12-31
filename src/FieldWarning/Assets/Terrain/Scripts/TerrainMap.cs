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

    // this determines how precise the terrain data is.. the higher.. the faster it will load, lower=more detailed data
    // this value is the minimum because the engine will determine the granularity around this number for best fit
    private const int GRANULARITY = 5;

    private const float MAP_SPACING = 1.5f * TerrainConstants.MAP_SCALE;
    private const int EXTENSION = 100;

    private const float ROAD_WIDTH_MULT = 0.5f;
    private const float TREE_RADIUS = 7f * TerrainConstants.MAP_SCALE;

    public const float BRIDGE_WIDTH = 3f * TerrainConstants.MAP_SCALE;
    public const float BRIDGE_HEIGHT = 1.0f; // temporary

    public const string HEIGHT_MAP_SUFFIX = "_sample_height.dat";
    private readonly string _HEIGHT_MAP_PATH;

    public readonly Vector3 MapMin, MapMax, MapCenter;

    private byte[,] _map;
    private int _mapSize;
    private float _terrainSpacingX, _terrainSpacingZ;

    // 2D array of terrain pieces for quickly finding which piece is at a given location
    private Terrain[,] _terrains;

    private Loading _loader;

    public readonly float WATER_HEIGHT;

    private GameObject[] _bridges;
    private ERModularRoad[] _roads;
    List<Vector3> _treePositions = new List<Vector3>();
    List<Vector3> _bridgePositions = new List<Vector3>();

    // this is only needed for map testing
    private byte[,] originalTestMap = null;

    public TerrainMap(Terrain[] terrains1D)
    {
        WaterBasic water = (WaterBasic)GameObject.FindObjectOfType(typeof(WaterBasic));
        if (water != null) {
            WATER_HEIGHT = water.transform.position.y;
        } else {
            WATER_HEIGHT = -1000;
        }

        // Find limits of the map
        MapMin = new Vector3(99999f, 0f, 99999f);
        MapMax = new Vector3(-99999f, 0f, -99999f);
        foreach (Terrain terrain in terrains1D)
        {
            Vector3 pos = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            MapMin.Set(Mathf.Min(MapMin.x, pos.x), 0.0f, Mathf.Min(MapMin.z, pos.z));
            MapMax.Set(Mathf.Max(MapMax.x, pos.x + size.x), 0.0f, Mathf.Max(MapMax.z, pos.z + size.x));
        }
        MapCenter = (MapMin + MapMax) / 2f;

        // Move terrains from 1D array to 2D array
        int sqrtLen = Mathf.RoundToInt(Mathf.Sqrt(terrains1D.Length));
        _terrains = new Terrain[sqrtLen, sqrtLen];
        _terrainSpacingX = (MapMax.x - MapMin.x) / sqrtLen;
        _terrainSpacingZ = (MapMax.z - MapMin.z) / sqrtLen;
        for (int i = 0; i < sqrtLen; i++)
        {
            for (int j = 0; j < sqrtLen; j++)
            {
                Vector3 corner = MapMin + new Vector3(_terrainSpacingX*i, 0f, _terrainSpacingZ*j);
                foreach (Terrain terrain in terrains1D)
                {
                    if (Mathf.Abs(terrain.transform.position.x - corner.x) < _terrainSpacingX / 2 &&
                            Mathf.Abs(terrain.transform.position.z - corner.z) < _terrainSpacingZ / 2)
                    {
                        _terrains[i,j] = terrain;
                    }
                }
            }
        }

        _mapSize = (int)(Mathf.Max(MapMax.x - MapMin.x, MapMax.z - MapMin.z) / 2f / MAP_SPACING);

        string sceneName = SceneManager.GetActiveScene().name;
        string scenePathWithFilename = SceneManager.GetActiveScene().path;
        string sceneDirectory = Path.GetDirectoryName(scenePathWithFilename);
        _HEIGHT_MAP_PATH = Path.Combine(sceneDirectory, sceneName + HEIGHT_MAP_SUFFIX);

        _roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        _bridges = GameObject.FindGameObjectsWithTag("Bridge");

        // get our trees
        foreach (Terrain terrain in _terrains)
        {
            foreach (TreeInstance tree in terrain.terrainData.treeInstances)
            {
                Vector3 treePosition = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;
                _treePositions.Add(treePosition);
            }
        }

        // get our bridge positions so we dont have to be in the main thread to access bridge into

        for (int i = 0; i < _bridges.Length; i++)
        {
            GameObject bridge = _bridges[i];
            _bridgePositions.Add(bridge.transform.position);
        }

        _loader = new Loading("Terrain");

        //TODO create some debug UI to dump the map when needed
        // TODO need to also somehow verify the height map is valid?? not sure how to do this each time without reading the original data.
        if (!File.Exists(_HEIGHT_MAP_PATH))
        {
            _map = CreateHeightMapFromtTerrainData();
            WriteHeightMap(_HEIGHT_MAP_PATH);
        } else
        {
            _loader.AddWorker(LoadHeightMap, "Loading height map");
        }


        // Loading bridges from a separate thread throws an exception.
        // This is why we first cache the bridge positions outside the thread before doing the below. 
        // same goes for roads and trees.
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
    /// Creates height map from original terrain data, slow
    /// </summary>
    private byte[,] CreateHeightMapFromtTerrainData()
    {

        Debug.Log("Creating original test map.");

        int nEntry = 2 * _mapSize + 2 * EXTENSION;

        byte[,] map = new byte[nEntry, nEntry];

        for (var x = 0; x < nEntry; x++)
        {
            for (var z = 0; z < nEntry; z+= GRANULARITY)
            {
                map[x, z] = (byte)(GetTerrainHeight(PositionOf(x, z)) > WATER_HEIGHT ? PLAIN : WATER);

                if (z + GRANULARITY >= nEntry)
                {
                    byte savedVal = map[x, z];

                    for (int i = 0; i < nEntry - z; i++)
                    {
                        map[x, z + i] = savedVal;
                    }
                }

            }
        }

        Debug.Log("Done creating original test map.");

        return map;
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

            for (int z = 0; z < nEntry; z+= GRANULARITY)
            {
                temp = GetTerrainHeight(PositionOf(x, z));

                //map[x, z] = (byte)(terrain.SampleHeight(PositionOf(x, z)) > waterHeight ? PLAIN : WATER);

                if (last == temp || lastcnt == 0)
                {
                    lastcnt+= (z+GRANULARITY>=nEntry)?nEntry-z:GRANULARITY;
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
        var inputMap = new byte[nEntry, nEntry];

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
                    inputMap[xCoord, zCoord] = bType;
                    zCoord++;
                }
            }

            if (_loader != null)
                // this is our loading screen status
                _loader.PercentDone = ((double)reader.BaseStream.Position / (double)reader.BaseStream.Length) * 100.0;
        }

        reader.Close();

        _map = inputMap;
    }


    private void LoadTrees()
    {

        // assign tree positions
        var currIdx = 0;

        foreach (var tree in _treePositions)
        {
            AssignCircularPatch(tree, TREE_RADIUS, FOREST);
            currIdx++;
            if (_loader != null)
                _loader.PercentDone = ((double)currIdx / (double)_treePositions.Count) * 100.0;
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
        foreach (Vector3 bridgePos in _bridgePositions)
        {

            // Bridge starts and ends at the two closest road nodes
            Vector3 start = Vector3.zero;
            float startDist = float.MaxValue;
            foreach (ERModularRoad road in _roads)
            {
                foreach (Vector3 roadVert in road.middleIndentVecs)
                {
                    float dist = (roadVert - bridgePos).magnitude;
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
                    float dist = (roadVert - bridgePos).magnitude;
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
                int indexX = MapIndex(position.x - MapCenter.x);
                int indexZ = MapIndex(position.z - MapCenter.z);
                if (indexX >= 0 && indexX < _map.Length && indexZ >= 0 && indexZ < _map.Length)
                    _map[indexX, indexZ] = value;
            }

            distLong += MAP_SPACING / 2;
        }
    }

    private void AssignCircularPatch(Vector3 position, float radius, byte value)
    {
        for (float x = -radius; x < radius; x += MAP_SPACING / 2) {
            for (float z = -radius; z < radius; z += MAP_SPACING / 2) {
                if (Mathf.Sqrt(x * x + z * z) < radius) {
                    int indexX = MapIndex(position.x + x - MapCenter.x);
                    int indexZ = MapIndex(position.z + z - MapCenter.z);
                    if (indexX >= 0 && indexX < _map.Length && indexZ >= 0 && indexZ < _map.Length)
                        _map[indexX, indexZ] = value;
                }
            }
        }
    }

    private Vector3 PositionOf(int x, int z)
    {
        Vector3 pos = MAP_SPACING * new Vector3(x - EXTENSION + 0.5f - _mapSize, 0f, z - EXTENSION + 0.5f - _mapSize);
        return pos + MapCenter;
    }

    private int MapIndex(float position)
    {
        int index = (int)(position / MAP_SPACING) + _mapSize + EXTENSION;
        return index;
    }

    public int GetTerrainType(Vector3 position)
    {
        return _map[MapIndex(position.x - MapCenter.x), MapIndex(position.z - MapCenter.z)];
    }

    public Terrain GetTerrainAtPos(Vector3 position)
    {
        int indexX = (int)((position.x - MapMin.x) / _terrainSpacingX);
        int indexZ = (int)((position.z - MapMin.z) / _terrainSpacingZ);
        if (indexX < 0 || indexX >= _terrains.GetLength(0))
            return null;
        if (indexZ < 0 || indexZ >= _terrains.GetLength(1))
            return null;
        return _terrains[indexX, indexZ];
    }

    public bool IsInMap(Vector3 position)
    {
        return GetTerrainAtPos(position) != null;
    }

    public float GetTerrainHeight(Vector3 position)
    {
        Terrain terrain = GetTerrainAtPos(position);
        return terrain == null ? WATER_HEIGHT : terrain.SampleHeight(position);
        //if (type == WATER) {
        //    return WATER_HEIGHT;
        //}else if (type == BRIDGE) {
        //    return BRIDGE_HEIGHT;
        //} else {
        //    return terrain.SampleHeight(position);
        //}
    }

}
