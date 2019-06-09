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
using UnityStandardAssets.Water;
using EasyRoads3Dv3;
using System.IO;
using System;
using System.Collections.Generic;

public class TerrainMap : Loading
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

    public const string height_map_path = "Assets/Terrain/SampleHeight.txt";

    private byte[,] map;
    private int mapSize;
    private Terrain terrain;
    public readonly float waterHeight;

    private List<Vector3> trees = new List<Vector3>();
    GameObject[] bridges;
    ERModularRoad[] roads;
    

    public TerrainMap(Terrain terrain) : base("Terrain")
    {
        this.terrain = terrain;
        WaterBasic water = (WaterBasic)GameObject.FindObjectOfType(typeof(WaterBasic));
        waterHeight = water.transform.position.y;

        mapSize = (int)(Mathf.Max(this.terrain.terrainData.size.x, this.terrain.terrainData.size.z) / 2f / MAP_SPACING);

        //TODO create some debug UI to dump the map when needed
        if (!File.Exists(height_map_path))
        {
            WriteHeightMap(height_map_path);
        }

        roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        bridges = GameObject.FindGameObjectsWithTag("Bridge");

        // get the trees
        foreach (TreeInstance tree in terrain.terrainData.treeInstances)
        {
            Vector3 treePosition = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;
            trees.Add(treePosition);
        }

        AddWorker(LoadHeightMap, "Loading height map");
        AddWorker(LoadTrees, "Setting tree positions");
        AddWorker(LoadRoads, "Connecting roads");
        AddWorker(LoadBridges, "Loading bridges");

    }

    public void WriteHeightMap(string path)
    {
        
        int nEntry = 2 * mapSize + 2 * EXTENSION;
        BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        for (int x = 0; x < nEntry; x++)
        {
            var first = true;
            float temp = 0;
            float last = 0;
            int lastcnt = 0;

            for (int z = 0; z < nEntry; z++)
            {


                temp = terrain.SampleHeight(PositionOf(x, z));

                //map[x, z] = (byte)(terrain.SampleHeight(PositionOf(x, z)) > waterHeight ? PLAIN : WATER);

                if (last == temp || first)
                {

                    lastcnt++;
                    first = false;

                }
                else
                {
                    writer.Write(temp);
                    writer.Write(lastcnt);


                    lastcnt = 0;
                    first = true;

                }

                last = temp;


            }
            // if there are multiple of same type on last iteration.. dump to file
            if (!first)
            {
                writer.Write(temp);
                writer.Write(lastcnt);

            }

            writer.Write((int)'\n');

        }

        writer.Close();
    }

    public void ReadHeightMap(string path)
    {
        //TODO : not much error checking is done in thsi function

        int nEntry = 2 * mapSize + 2 * EXTENSION;
        var file = File.ReadAllBytes(path);

        //var last_notify_msec = 0;
        int x = 0;
        int z = 0;
        MemoryStream ms = new MemoryStream(file);
        BinaryReader reader = new BinaryReader(ms);

        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {

            // <type><-><amount><,>[\n]
            byte[] type = reader.ReadBytes(4);

            if (BitConverter.ToInt32(type, 0) == (int)0x0a)
            {
                z = 0;
                x++;
            }
            else
            {

                // since we already read a byte but we need 4 bytes
                int len = reader.ReadInt32();

                var bType = (byte)(BitConverter.ToSingle(type, 0) > waterHeight ? PLAIN : WATER);

                var zEnd = z + len;
  
                // populate the rest of the same type
                while (z < zEnd)
                {
                    map[x, z] = bType;
                    z++;
                }
                
            }

            // this is our loading screen status
            percent_done = ((double)reader.BaseStream.Position / (double)reader.BaseStream.Length) * 100.0;
        }


        reader.Close();
    }


    public void LoadTrees()
    {
        // assign tree positions
        var currIdx = 0;

        foreach (var tree in trees)
        {
            AssignCircularPatch(tree, TREE_RADIUS, FOREST);
            currIdx++;
            percent_done = ((double)currIdx / (double)trees.Count) * 100.0;
        }

        
    }

    public void LoadRoads()
    {
        var currRoadIdx = 0;
        foreach (ERModularRoad road in roads)
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
                percent_done =  ((currRoadVertIdx / road.middleIndentVecs.Count) * 100) * (currRoadIdx / roads.Length);
            }

            currRoadIdx++;
        }
    }

    public void LoadBridges()
    {
        for (int i = 0; i < bridges.Length; i++)
        {
            GameObject bridge = bridges[i];

            // Bridge starts and ends at the two closest road nodes
            Vector3 start = Vector3.zero;
            float startDist = float.MaxValue;
            foreach (ERModularRoad road in roads)
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
            foreach (ERModularRoad road in roads)
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

    public void LoadHeightMap()
    {
        int nEntry = 2 * mapSize + 2 * EXTENSION;
        map = new byte[nEntry, nEntry];

        //Write some text to the test.txt file
        //StreamWriter writer = new StreamWriter(path, false);
        //StreamReader reader = new StreamReader(path, true);

        // assign heights
        ReadHeightMap(height_map_path);
        
        

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
                if (indexX >= 0 && indexX < map.Length && indexZ >= 0 && indexZ < map.Length)
                    map[MapIndex(position.x), MapIndex(position.z)] = value;
            }

            distLong += MAP_SPACING / 2;
        }
    }

    private void AssignCircularPatch(Vector3 position, float radius, byte value)
    {
        for (float x = -radius; x < radius; x += MAP_SPACING / 2) {
            for (float z = -radius; z < radius; z += MAP_SPACING / 2) {
                if (Mathf.Sqrt(x*x + z*z) < radius)
                    map[MapIndex(position.x + x), MapIndex(position.z + z)] = value;
            }
        }
    }

    private Vector3 PositionOf(int x, int z)
    {
        return MAP_SPACING * new Vector3(x - EXTENSION + 0.5f - mapSize, 0f, z - EXTENSION + 0.5f - mapSize);
    }

    private int MapIndex(float position)
    {
        int index = (int)(position / MAP_SPACING) + mapSize + EXTENSION;
        return index;
    }

    public int GetTerrainType(Vector3 position)
    {
        return map[MapIndex(position.x), MapIndex(position.z)];
    }

    public float GetTerrainHeight(Vector3 position, int type)
    {
        if (type == WATER) {
            return waterHeight;
        }else if (type == BRIDGE) {
            return BRIDGE_HEIGHT;
        } else {
            return terrain.SampleHeight(position);
        }
    }

}
