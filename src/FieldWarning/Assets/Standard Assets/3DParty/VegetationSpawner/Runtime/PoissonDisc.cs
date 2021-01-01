// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Staggart.VegetationSpawner
{
    /// <summary>
    /// 2D (XZ) poisson disc sampling within bounds
    /// </summary>
    public sealed class PoissonDisc
    {
        //Max attempts
        private const int MAX_ATTEMPTS = 10;
        private const int dimensions = 2; //2D

        private static List<Vector2> samples = new List<Vector2>();
        private static List<Vector2> points = new List<Vector2>();
        private static List<Vector3> spawnPoints = new List<Vector3>();

        private static int[,] grid;
        private static float cellSize;
        private static float radius;
        private static Bounds bounds;

        public static List<Vector3> GetSpawnpoints(Terrain terrain, float radius, int seed)
        {
            PoissonDisc.radius = radius;
            PoissonDisc.bounds = terrain.terrainData.bounds;

            cellSize = radius / Mathf.Sqrt(dimensions);

            //Initialize terrain background grid
            int xCells = Mathf.CeilToInt(bounds.size.x / cellSize);
            int zCells = Mathf.CeilToInt(bounds.size.z / cellSize);
            grid = new int[xCells, zCells];

            samples.Clear();
            points.Clear();
            spawnPoints = new List<Vector3>();

            Random.InitState(seed);

            //Random starting point
            Vector2 randomPos = new Vector2(Random.value * bounds.size.x, Random.value * bounds.size.z);
            samples.Add(randomPos);

            while (samples.Count > 0)
            {
                int i = Random.Range(0, samples.Count);
                Vector2 sampleCenter = samples[i];

                bool valid = false;

                //Find next available position randomly outside radius^2
                for (int s = 0; s < MAX_ATTEMPTS; s++)
                {
                    Random.InitState(seed + s + i);

                    Vector2 sample = RandomPointOnAnnulus(sampleCenter);

                    if (ValidSample(sample))
                    {
                        Vector3 spawnPoint = CreateSpawnPoint(terrain, sample);

                        spawnPoints.Add(spawnPoint);

                        points.Add(sample);
                        samples.Add(sample);

                        Vector2Int gridPos = PositionToGridCoord(sample);
                        grid[gridPos.x, gridPos.y] = points.Count;

                        valid = true;
                        break;
                    }

                }
                if (!valid)
                {
                    samples.RemoveAt(i);
                }

            }

            return spawnPoints;
        }

        private static Vector2Int PositionToGridCoord(Vector2 pos)
        {
            return new Vector2Int((int)(pos.x / cellSize), (int)(pos.y / cellSize));
        }

        private static bool ValidSample(Vector2 sample)
        {
            //Reject any samples outside of the terrain bounds
            bool valid = InsideBounds(sample);

            if (valid)
            {
                Vector2Int gridPos = PositionToGridCoord(sample);

                int xmin = Mathf.Max(gridPos.x - 2, 0);
                int xmax = Mathf.Min(gridPos.x + 2, grid.GetLength(0) - 1);
                int ymin = Mathf.Max(gridPos.y - 2, 0);
                int ymax = Mathf.Min(gridPos.y + 2, grid.GetLength(1) - 1);

                //Check cells around current grid cell (3x3)
                for (int y = ymin; y <= ymax; y++)
                {
                    for (int x = xmin; x <= xmax; x++)
                    {
                        int i = grid[x, y] - 1;

                        if (i != -1)
                        {
                            if (OutsideRadius(sample, points[i])) return false;
                        }
                    }
                }
            }
            return valid;
        }

        private static bool InsideBounds(Vector2 pos)
        {
            return (pos.x >= 0 && pos.x < bounds.size.x && pos.y >= 0 && pos.y < bounds.size.z);
        }

        //Check if position falls within annulus
        private static bool OutsideRadius(Vector2 center, Vector2 position)
        {
            return ((center - position).sqrMagnitude < (radius * radius));
        }

        private static Vector2 RandomPointOnAnnulus(Vector2 center)
        {
            float angle = 2f * Mathf.PI * Random.value;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float dist = Random.Range(radius, radius * 2);

            return center + (dir * dist);
        }

        //Creates a world-space spawn point, relative to the terrain bounds
        private static Vector3 CreateSpawnPoint(Terrain t, Vector2 position)
        {
            return new Vector3((position.x + (t.GetPosition().x + bounds.center.x)) - bounds.extents.x, 0f, (position.y + (t.GetPosition().z + bounds.center.z)) - bounds.extents.z);
        }
    }
}