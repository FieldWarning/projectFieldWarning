// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Staggart.VegetationSpawner
{
    [AddComponentMenu("")] //Hide
    public class SpawnerBase : MonoBehaviour
    {
        private void OnValidate()
        {
            if (terrainSettings == null) terrainSettings = new TerrainSettings();
        }
        public int seed = 0;

        public List<Terrain> terrains = new List<Terrain>();

        [Serializable]
        public class VegetationPrefab
        {
            public int seed;
            [Range(0f, 100f)]
            public float probability;
            public bool collisionCheck;
            public bool rejectUnderwater;
            public Vector2 heightRange = new Vector2(0f, 1000f);
            public Vector2 slopeRange = new Vector2(0f, 60f);
            public Vector2 curvatureRange = new Vector2(0f, 1f);

            public List<TerrainLayerMask> layerMasks = new List<TerrainLayerMask>();
            public int instanceCount;
        }

        [Serializable]
        public class TreeType : VegetationPrefab
        {
            [NonSerialized]
            public List<Vector3> spawnPoints = new List<Vector3>();

            [SerializeField]
            public List<TreePrefab> prefabs = new List<TreePrefab>();

            [Range(1f, 25f)]
            public float distance = 5f;
           
            public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
            public float sinkAmount = 0f;

            public static TreeType New()
            {
                TreeType t = new TreeType();

                //Constructor for inherent variables
                t.probability = 10f;

                //Add an initial prefab on construction
                TreePrefab p = new TreePrefab();
                t.prefabs.Add(p);

                return t;
            }

        }
        [SerializeField]
        public List<TreeType> treeTypes = new List<TreeType>();

        [Serializable]
        public class TreePrefab
        {
            //Index of tree prototype in TerrainData
            public int index;
            [Range(0f, 100f)]
            public float probability = 100f;
            public GameObject prefab;
        }

        public enum GrassType
        {
            Mesh,
            Billboard
        }

        [Serializable]
        public class GrassPrefab : VegetationPrefab
        {
            public int index;
            public GrassType type = GrassType.Billboard;
            public GameObject prefab;
            public Texture2D billboard;

            public Color mainColor = new Color(0.2692482f, 0.6603774f, 0f, 1f);
            public Color secondairyColor = new Color(0.2457143f, 0.6037736f, 0f, 1f);
            public bool linkColors;

            public Vector2 minMaxHeight;
            public Vector2 minMaxWidth;
            [Range(0.01f, 0.5f)]
            public float noiseSize;

            public GrassPrefab()
            {
                probability = 25f;
                heightRange = new Vector2(0f, 1000f);
                slopeRange = new Vector2(0f, 60f);
                minMaxHeight = new Vector2(0.5f, 1f);
                minMaxWidth = new Vector2(0.8f, 1.2f);
                noiseSize = 0.1f;
            }
        }
        public List<GrassPrefab> grassPrefabs = new List<GrassPrefab>();

        [Serializable]
        public class TerrainLayerMask
        {
            public int layerID;
            [Range(0f, 1f)]
            public float threshold = 0f;

            public TerrainLayerMask() {}

            //Cloning
            public TerrainLayerMask(int layerID, float threshold)
            {
                this.layerID = layerID;
                this.threshold = threshold;
            }
        }

        [Serializable]
        public class TerrainSettings
        {
            public bool drawInstanced = true;
            [Range(0f, 1000f)]
            public float detailDistance = 1000f;

            [Header("Trees")]
            public bool perservePrefabLayer = true;
            public bool treeLightProbes = false;
            [Range(0f, 5000f)]
            public float treeDistance = 1000f;
            [Range(5f, 2000f)]
            public float billboardStart = 300f;

            [Header("Grass wind")]
            [Range(0f, 1f)]
            public float windStrength = 0.5f;
            [Range(0f, 1f)]
            public float windSpeed = 1f;
            [Range(0.1f, 10f)]
            public float windFrequency = 2f;
            public Color wintTint = Color.white;
        }
        public TerrainSettings terrainSettings = new TerrainSettings();

        [ContextMenu("Randomize seed")]
        public void RandomizeSeed()
        {
            seed = Random.Range(0, 9999);
        }

        public void InitializeSeed(int start = 0)
        {
            Random.InitState(start + seed);
        }

        private static int recursionCounter;

        public static TreePrefab GetProbableTree(TreeType treeType)
        {
            recursionCounter = 0;

            return PickTreeRecursive(treeType);
        }

        //Chooses a prefab based on probability, recursively executed until succesful
        private static TreePrefab PickTreeRecursive(TreeType treeType)
        {
            if (treeType.prefabs.Count == 0) return null;

            TreePrefab p = treeType.prefabs[Random.Range(0, treeType.prefabs.Count)];

            //If prefabs have an extremely low probabilty, give up after 4 attempts
            if (recursionCounter >= 4) return null;

            if ((Random.value * 100f) <= p.probability)
            {
                //Debug.Log("<color=green>" + p.prefab.name + " passed probability check..</color>");
                return p;
            }

            //Debug.Log("<color=red>" + p.prefab.name + " failed probability check, trying another...</color>");

            recursionCounter++;

            //Note: It's possible for the next candidate to be the one that just failed
            return PickTreeRecursive(treeType);
        }

        public void CopySettingsToTerrains()
        {
            foreach (Terrain t in terrains)
            {
                t.drawInstanced = terrainSettings.drawInstanced;
                t.detailObjectDistance = terrainSettings.detailDistance;

                t.preserveTreePrototypeLayers = terrainSettings.perservePrefabLayer;
#if UNITY_EDITOR
                t.bakeLightProbesForTrees = terrainSettings.treeLightProbes;
#endif
                t.treeBillboardDistance = terrainSettings.billboardStart;
                t.treeDistance = terrainSettings.treeDistance;

                t.terrainData.wavingGrassAmount = terrainSettings.windStrength;
                t.terrainData.wavingGrassStrength = terrainSettings.windSpeed;
                t.terrainData.wavingGrassSpeed = terrainSettings.windFrequency;
                t.terrainData.wavingGrassTint = terrainSettings.wintTint;
            }
        }

        //Returns the splatmap index for a given terrain layer
        public static int GetSplatmapID(int layerID)
        {
            if (layerID > 3) return 1;
            if (layerID > 7) return 2;
            if (layerID > 11) return 3;

            return 0;
        }

        public static float SampleChannel(Color color, int channel)
        {
            float value = 0;

            switch (channel)
            {
                case 0:
                    value = color.r;
                    break;
                case 1:
                    value = color.g;
                    break;
                case 2:
                    value = color.b;
                    break;
                case 3:
                    value = color.a;
                    break;
            }

            return value;
        }
    }
}