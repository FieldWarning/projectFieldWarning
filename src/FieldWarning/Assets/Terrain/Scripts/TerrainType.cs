using UnityEngine;
using UnityEditor;
using UnityStandardAssets.Water;
using EasyRoads3Dv3;

public class TerrainMap
{
    public const int PLAIN = 0;
    public const int ROAD = 1;
    public const int WATER = 2;
    public const int FOREST = 3;

    private const float MAP_SPACING = 2f * TerrainConstants.MAP_SCALE;
    private const float ROAD_WIDTH_MULT = 0.8f;
    private const int EXTENSION = 100;

    private byte[,] map;
    private int mapSize;

    public TerrainMap(Terrain terrain)
    {
        mapSize = (int)(Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z) / 2f / MAP_SPACING);
        int nEntry = 2*mapSize + 2*EXTENSION;
        map = new byte[nEntry,nEntry];

        WaterBasic water = (WaterBasic)GameObject.FindObjectOfType(typeof(WaterBasic));
        float waterHeight = water.transform.position.y;
        for (int x = 0; x < nEntry; x++) {
            for (int z = 0; z < nEntry; z++)
                map[x, z] = (byte)(terrain.SampleHeight(PositionOf(x, z)) > waterHeight ? PLAIN : WATER);
        }
        
        ERModularRoad[] roads = (ERModularRoad[])GameObject.FindObjectsOfType(typeof(ERModularRoad));
        foreach (ERModularRoad road in roads) {

            // Loop over linear road stretches
            Vector3 previousVert = Vector3.zero;
            foreach (Vector3 roadVert in road.middleIndentVecs) {
                if (previousVert != Vector3.zero) {
                    float stretch = (roadVert - previousVert).magnitude;
                    Vector3 directionLong = (roadVert - previousVert).normalized;
                    Vector3 directionWide = new Vector3(-directionLong.z, 0f, directionLong.x);

                    // Step along length of road
                    float distLong = 0f;
                    while (distLong < stretch) {
                        Vector3 positionLong = previousVert + distLong * directionLong;

                        // Step along width of road
                        int nPointWide = (int)(ROAD_WIDTH_MULT*road.roadWidth/(MAP_SPACING/2));
                        for (int iWidth = -nPointWide; iWidth <= nPointWide; iWidth++) {
                            Vector3 position = positionLong + iWidth * (MAP_SPACING / 2) * directionWide;
                            int indexX = MapIndex(position.x);
                            int indexZ = MapIndex(position.z);
                            if (indexX >= 0 && indexX < nEntry && indexZ >= 0 && indexZ < nEntry)
                                map[MapIndex(position.x), MapIndex(position.z)] = ROAD;
                        }

                        distLong += MAP_SPACING / 2;
                    }
                }
                previousVert = roadVert;
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
        return index;// Mathf.Clamp(index, 0, map.Length-1);
    }

    public int GetTerrainType(Vector3 position)
    {
        return map[MapIndex(position.x), MapIndex(position.z)];
    }
}
