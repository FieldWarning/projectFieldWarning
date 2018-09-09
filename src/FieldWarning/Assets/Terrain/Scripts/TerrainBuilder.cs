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
using System;

public class TerrainBuilder : MonoBehaviour
{
    static int fractalSize = 100;
    int treeCount = 1;
    static int[,] map;
    static Terrain terrain;
    public static Vector3 size;
    static float[,,] alphaMap;

    // Use this for initialization
    void Start()
    {
        terrain = GetComponent<Terrain>();
        size = terrain.terrainData.size;
        var forestOptions = new FractalOptions(2, 5);
        var townOptions = new FractalOptions(2, 5);

        InitializeAlphamap();
        BuildFields();
        BuildRoads();
        BuildTerrainTextures(forestOptions);
        BuildTownTextures(townOptions);
        BuildTrees();
        BuildHedges();
        terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
        terrain.Flush();

        TerrainData.initialize(alphaMap);
        RoadNetwork.BuildNetwork();
    }

    private static void InitializeAlphamap()
    {
        alphaMap = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
        for (int i = 0; i < terrain.terrainData.alphamapWidth; i++) {
            for (int j = 0; j < terrain.terrainData.alphamapHeight; j++) {
                for (int l = 0; l < terrain.terrainData.alphamapLayers; l++) {
                    if (l == 1) alphaMap[i, j, l] = 1;
                    else alphaMap[i, j, 2] = 0;
                }

            }
        }
    }

    void BuildHedges()
    {
        float width = terrain.terrainData.alphamapWidth;
        float height = terrain.terrainData.alphamapHeight;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (UnityEngine.Random.value > alphaMap[i, j, TerrainType.Hedge]) continue;
                //Debug.Log("shrub");
                var x = (i + .8f * (UnityEngine.Random.value)) / width;
                var y = (j + .8f * (UnityEngine.Random.value)) / height;
                TreeInstance tree = new TreeInstance();
                tree.prototypeIndex = 1;
                tree.heightScale = 1;
                tree.widthScale = 1;
                tree.color = Color.white;
                tree.lightmapColor = Color.white;
                tree.rotation = 0;
                tree.position = new Vector3(y, 0, x);
                terrain.AddTreeInstance(tree);
            }
        }
    }

    void BuildFields()
    {
        var width = terrain.terrainData.alphamapWidth;
        var height = terrain.terrainData.alphamapHeight;
        int fields = 100;
        for (int i = 0; i < fields; i++) {

            var pos = new Vector3(UnityEngine.Random.Range(0, width), 0, UnityEngine.Random.Range(0, height));
            var corner = new Vector3(UnityEngine.Random.Range(10, 30), 0, UnityEngine.Random.Range(10, 30));
            var rotation = UnityEngine.Random.Range(0, 360);
            int type = UnityEngine.Random.Range(2, 4);
            BuildField(pos, corner, rotation, type);
        }
    }

    void BuildField(Vector3 pos, Vector3 topRightCorner, float rotation, int type)
    {
        var rot = Quaternion.AngleAxis(rotation, Vector3.up);
        var corners = new Vector3[4];
        corners[0] = rot * topRightCorner + pos;
        corners[1] = rot * new Vector3(topRightCorner.x, 0, -topRightCorner.z) + pos;
        corners[2] = rot * new Vector3(-topRightCorner.x, 0, -topRightCorner.z) + pos;
        corners[3] = rot * new Vector3(-topRightCorner.x, 0, topRightCorner.z) + pos;
        int minX = (int)corners[0].x;
        int minY = (int)corners[0].z;
        int maxX = (int)corners[0].x;
        int maxY = (int)corners[0].z;
        for (int i = 1; i < 4; i++) {
            if (corners[i].x < minX) {
                minX = (int)corners[i].x;
            }
            if (corners[i].x > maxX) {
                maxX = (int)corners[i].x;
            }
            if (corners[i].z < minY) {
                minY = (int)corners[i].z;
            }
            if (corners[i].z > maxY) {
                maxY = (int)corners[i].z;
            }
        }

        for (int x = minX - 4; x < maxX + 4; x++) {
            for (int y = minY - 4; y < maxY + 4; y++) {
                if (x < 0 || x >= terrain.terrainData.alphamapWidth || y < 0 || y >= terrain.terrainData.alphamapHeight) continue;
                var edge = EdgeFactor(x, y, corners);
                //if(edge!=0)Debug.Log(edge);
                if (Inside(x, y, corners)) {

                    for (int i = 0; i < alphaMap.GetLength(2); i++) {
                        alphaMap[x, y, i] = 0;
                    }
                    alphaMap[x, y, type] = 1 - edge;
                    alphaMap[x, y, TerrainType.Hedge] = edge;
                } else {
                    for (int i = 0; i < alphaMap.GetLength(2); i++) {
                        alphaMap[x, y, i] *= 1 - edge;
                    }
                    alphaMap[x, y, TerrainType.Hedge] += edge;// < alphaMap[x, y, 0]? alphaMap[x, y, 0]:edge;
                }
            }
        }
    }


    void BuildRoads()
    {
        int n = 5;
        float edgePadding = 0.1f;
        //float padding = 0.1f;
        var size = terrain.terrainData.size;
        //var dx = size.x * (1 - 2 * edgePadding) / (n);
        //var dy = size.z * (1 - 2 * edgePadding) / (n);
        //var x0 = size.x * edgePadding;
        //var y0 = size.z * edgePadding;
        Vector3[,] nodes = new Vector3[n, n];
        List<Road> roads = new List<Road>();
        //for (int i = 0; i < n; i++)
        //{
        //    for (int j = 0; j < n; j++)
        //    {
        //        var x = x0 + i * dx + dx * (padding + (1 - 2 * padding) * UnityEngine.Random.value);
        //        var y = y0 + j * dy + dy * (padding + (1 - 2 * padding) * UnityEngine.Random.value); ;
        //        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        //obj.transform.localScale = 10 * Vector3.one;
        //        nodes[i, j] = new Vector3(x, 0, y);
        //    }
        //}
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                if (i < n - 1) {
                    roads.Add(new Road(nodes[i, j], nodes[i + 1, j]));
                }
                if (j < n - 1) {
                    roads.Add(new Road(nodes[i, j], nodes[i, j + 1]));
                }
            }
        }
    }

    void BuildTownTextures(FractalOptions options)
    {
        int[,] fracMap = GetMapFromFractal(options, new float[] { 10, 1 });
        ApplyTextureFromFracMap(fracMap, TerrainType.Town);
    }

    void BuildTerrainTextures(FractalOptions options)
    {
        int[,] fracMap = GetMapFromFractal(options, new float[] { 5, 3 });
        map = fracMap;
        //float[, ,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, 2];
        // For each point on the alphamap...
        ApplyTextureFromFracMap(fracMap, 0);
    }

    private void BuildTrees()
    {
        float width = terrain.terrainData.alphamapWidth;
        float height = terrain.terrainData.alphamapHeight;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (UnityEngine.Random.value > .3f * alphaMap[i, j, 0]) continue;
                var x = (i + .8f * (UnityEngine.Random.value)) / width;
                var y = (j + .8f * (UnityEngine.Random.value)) / height;
                TreeInstance tree = new TreeInstance();
                tree.prototypeIndex = 0;
                tree.heightScale = 1;
                tree.widthScale = 1;
                tree.color = Color.white;
                tree.lightmapColor = Color.white;
                tree.rotation = 0;
                tree.position = new Vector3(y, 0, x);
                terrain.AddTreeInstance(tree);
            }
        }
    }

    private void ApplyTextureFromFracMap(int[,] fracMap, int texture)
    {
        Terrain t = terrain;
        t.terrainData.treeInstances = new TreeInstance[1000];
        for (var y = 0; y < t.terrainData.alphamapHeight; y++) {
            for (var x = 0; x < t.terrainData.alphamapWidth; x++) {
                // Get the normalized terrain coordinate that
                // corresponds to the the point.
                var dx = 1.0f / (t.terrainData.alphamapWidth - 1);
                var dy = 1.0f / (t.terrainData.alphamapHeight - 1);
                var normX = x * dx;
                var normY = y * dy;

                var X = normX * (fracMap.GetLength(0) - 2);
                var Y = normY * (fracMap.GetLength(1) - 2);
                int xMin = (int)X;
                int yMin = (int)Y;
                float xt = X - xMin;
                float yt = Y - yMin;
                float frac = fracMap[xMin, yMin] +
                    (fracMap[xMin + 1, yMin] - fracMap[xMin, yMin]) * xt +
                    (fracMap[xMin, yMin + 1] - fracMap[xMin, yMin]) * yt +
                    (fracMap[xMin + 1, yMin + 1] + fracMap[xMin, yMin] - (fracMap[xMin + 1, yMin] + fracMap[xMin, yMin + 1])) * xt * yt;

                frac = Mathf.Sqrt(frac);
                alphaMap[x, y, texture] = frac;
                for (int i = 0; i < 5; i++) {
                    if (i == texture) continue;
                    alphaMap[x, y, i] *= 1 - frac;

                }
            }
        }
    }
    //done

    private static int[,] GetMapFromFractal(FractalOptions options, float[] cut)
    {
        var fractalMap = Fractal.fractal(fractalSize, fractalSize, options);
        var map = new int[fractalSize, fractalSize];
        var cutoff = Fractal.getCutOffLevels(fractalMap, cut);
        for (var i = 0; i < fractalSize; i++) {
            for (var j = 0; j < fractalSize; j++) {
                /**/
                if (fractalMap[i, j] < cutoff[0]) {
                    map[i, j] = 0;
                } else {
                    map[i, j] = 1;
                }

            }
        }
        return map;
    }

    private float EdgeFactor(int x, int y, Vector3[] corners)
    {
        float minDistance = .1f;
        float maxDistance = 1f;
        var v = new Vector3(x, 0, y);
        var distance = Single.PositiveInfinity;
        for (int i = 0; i < 4; i++) {
            var p1 = corners[i];
            var p2 = corners[(i + 1) % 4];
            var q1 = p2 - p1;
            var q2 = v - p1;
            var proj = Vector3.Dot(q2, q1.normalized);
            var inRegion = proj > 0 && proj < q1.magnitude;

            var d = Mathf.Abs(Vector3.Cross(q2, q1.normalized).y);
            if (!inRegion) d = q2.magnitude;

            if (distance > d) {
                distance = d;
            }
        }

        if (distance < minDistance) {
            return 1;
        } else if (distance < maxDistance) {
            return (maxDistance - distance) / (maxDistance - minDistance);
        } else {
            return 0;
        }
    }

    private bool Inside(int x, int y, Vector3[] corners)
    {
        var v = new Vector3(x, 0, y);
        for (int i = 0; i < 4; i++) {
            var p1 = corners[i];
            var p2 = corners[(i + 1) % 4];
            var q1 = p2 - p1;
            var q2 = v - p1;
            if (Vector3.Cross(q2, q1).y > 0) {
                return false;
            }
        }
        return true;
    }

    void BuildTerrainVolume(Vector3 pos)
    {
        GameObject volumeObject = new GameObject();
        var collider = volumeObject.AddComponent<BoxCollider>();
        collider.transform.localScale = Vector3.one * terrain.terrainData.size.x / fractalSize;
        transform.position = pos;
    }
}

public class Road
{
    public Road(Vector3 p1, Vector3 p2)
    {
        var diff = (p2 - p1);
        var theta1 = 90 * UnityEngine.Random.value - 45;
        var p1p = Quaternion.Euler(0, theta1, 0) * diff;
        var theta2 = 90 * UnityEngine.Random.value - 45;
        var p2p = Quaternion.Euler(0, theta2, 0) * diff;

        //var go = GameObject.Instantiate(GameObject.Find("RoadBuilder"));
        //go.GetComponent<MeshBuilder>().buildRoadStretch(p1, p1p, p2, p2p);
    }
}
