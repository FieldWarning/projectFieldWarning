// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Staggart.VegetationSpawner
{
    /// <summary>
    /// Extension class to simplify sampling terrain data
    /// </summary>
    public static class TerrainSampler
    {
        /// <summary>
        /// Converts a world-space position to a normalized local-space XZ value (0-1 range)
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static Vector2 GetNormalizedPosition(this Terrain terrain, Vector3 worldPosition)
        {
            Vector3 localPos = terrain.transform.InverseTransformPoint(worldPosition);

            //Position relative to terrain as 0-1 value
            return new Vector2(
                localPos.x / terrain.terrainData.size.x,
                localPos.z / terrain.terrainData.size.z);
        }

        /// <summary>
        /// Sample various height forms at a given position
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <param name="height">Height value, relative to terrain local-space</param>
        /// <param name="worldHeight">Height value in world-space</param>
        /// <param name="normalizedHeight">0-1 height value, same as heightmap</param>
        public static void SampleHeight(this Terrain terrain, Vector2 position, out float height, out float worldHeight, out float normalizedHeight)
        {
            height = terrain.terrainData.GetHeight(
                Mathf.CeilToInt(position.x * terrain.terrainData.heightmapTexture.width),
                Mathf.CeilToInt(position.y * terrain.terrainData.heightmapTexture.height)
                );

            worldHeight = height + terrain.transform.position.y;
            //Normalized height value (0-1)
            normalizedHeight = height / terrain.terrainData.size.y;
        }

        /// <summary>
        /// Returns the slope at a given position in 0-90 degrees
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <param name="average">Average from neighboring samples</param>
        /// <returns></returns>
        public static float GetSlope(this Terrain terrain, Vector2 position, bool average = false)
        {
            if (!average)
            {
                return terrain.terrainData.GetSteepness(position.x, position.y);
            }

            return GetAverageSlope(terrain, position);
        }

        /// <summary>
        /// Returns the 2Darray indices of the detail map at a given position
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <returns></returns>
        public static Vector2Int DetailIndex(this Terrain terrain, Vector2 position)
        {
            return new Vector2Int(
                Mathf.CeilToInt(position.x * terrain.terrainData.detailResolution),
                Mathf.CeilToInt(position.y * terrain.terrainData.detailResolution)
                );
        }

        /// <summary>
        /// Returns the texel indices of the splatmap map at a given position
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <returns></returns>
        public static Vector2Int SplatmapTexelIndex(this Terrain terrain, Vector2 position)
        {
            return new Vector2Int(
               Mathf.CeilToInt(position.x * terrain.terrainData.alphamapWidth),
               Mathf.CeilToInt(position.y * terrain.terrainData.alphamapHeight)
               );
        }

        /// <summary>
        /// Returns a XZ world-space position based on detail indices. 
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>World position with X and Z component. Note: y-component is 0!</returns>
        public static Vector3 DetailToWorld(this Terrain terrain, int x, int y)
        {
            //XZ world position
            return new Vector3(
                terrain.GetPosition().x + (((float)x / (float)terrain.terrainData.detailWidth) * (terrain.terrainData.size.x)),
                0f,
                terrain.GetPosition().z + (((float)y / (float)terrain.terrainData.detailHeight) * (terrain.terrainData.size.z))
                );
        }

        /// <summary>
        /// Returns an averaged slope at the given position. Average is taken from 5 samples.
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <returns>Slope value in 0-90 degrees</returns>
        public static float GetAverageSlope(Terrain terrain, Vector2 position)
        {
            float texelSize = (1f / terrain.terrainData.heightmapTexture.width) * 2f;

            float slope = 0f;

            //Center
            slope += terrain.terrainData.GetSteepness(position.x, position.y);
            //Right
            slope += terrain.terrainData.GetSteepness(position.x + texelSize, position.y);
            //Left
            slope += terrain.terrainData.GetSteepness(position.x - texelSize, position.y);
            //Up
            slope += terrain.terrainData.GetSteepness(position.x, position.y + texelSize);
            //Down
            slope += terrain.terrainData.GetSteepness(position.x, position.y - texelSize);

            return slope / 5f;
        }

        /// <summary>
        /// Returns the curvature value at a given position. 
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position">Values should be normalized within 0-1 range. Use GetNormalizedPosition to convert a world-space position to this.</param>
        /// <param name="radius">Radius in heightmap texel-space</param>
        /// <returns>Curvature value, 0= flat, 1=convex. One minus value to get 0-1 concavity value</returns>
        public static float SampleConvexity(this Terrain terrain, Vector2 position, float radius = 3f)
        {
            float texelSize = (1f / terrain.terrainData.heightmapResolution) * radius;

            float posX = terrain.terrainData.GetInterpolatedNormal(position.x + texelSize, position.y).x;
            float negX = terrain.terrainData.GetInterpolatedNormal(position.x - texelSize, position.y).x;

            float x = (posX - negX) + 0.5f;

            float posY = terrain.terrainData.GetInterpolatedNormal(position.x, position.y + texelSize).z;
            float NegY = terrain.terrainData.GetInterpolatedNormal(position.x, position.y - texelSize).z;

            float y = (posY - NegY) + 0.5f;

            //Blend overlay
            return (y < 0.5f) ? 2.0f * x * y : 1.0f - 2.0f * (1.0f - x) * (1.0f - y);
        }

        /// <summary>
        /// Remaps a convexity value to a curvature range. 0=concave, 0.5=flat, 1=convex
        /// </summary>
        /// <param name="convexity"></param>
        /// <returns></returns>
        public static float ConvexityToCurvature(float convexity)
        {
            return (convexity - (1 - convexity)) * 0.5f + 0.5f;
        }

        /// <summary>
        /// Given a specific prefab, returns all the instances of a tree prefab. Positions will be converted to world-space
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static TreeInstance[] GetTreeInstances(this Terrain terrain, Object prefab)
        {
            var prototypeIndex = -1;
 
            //Get the index of the given prefab
            for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
            {
                if (terrain.terrainData.treePrototypes[i].prefab == prefab) prototypeIndex = i;
            }
 
            if (prototypeIndex >= 0)
            {
                //Get all instances matching the prefab index
                TreeInstance[] instances = terrain.terrainData.treeInstances.Where(x => x.prototypeIndex == prototypeIndex).ToArray();

                //Un-normalize positions so they're in world-space
                for (int i = 0; i < instances.Length; i++)
                {
                    instances[i].position = Vector3.Scale(instances[i].position, terrain.terrainData.size);
                    instances[i].position += terrain.GetPosition();
                }
                
                return instances;
            }
            else
            {
                Debug.LogError("Failed to return instances. Tree prefab not found in " + terrain.name);
                return null;
            }
        }
    }
}