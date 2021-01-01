// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Staggart.VegetationSpawner
{
    [ExecuteInEditMode]
    [AddComponentMenu("Vegetation Spawner")]
    public class VegetationSpawner : SpawnerBase
    {
        public const string Version = "1.0.3";
        
        //[SerializeField]
        public Dictionary<Terrain, Cell[,]> terrainCells = new Dictionary<Terrain, Cell[,]>();
        //This is used for the height range slider
        public float maxTerrainHeight = 1000f;
        
        public int cellSize = 64;
        public int cellDivisions = 4;
        [System.NonSerialized]
        public static bool VisualizeCells = false;
        public static bool VisualizeWaterlevel = false;
        [Tooltip("When enabled, raycasting is also performed on the corners of each cell. This is slower to calculate, but will yield higher precision around collider edges")]
        public bool highPrecisionCollision = true;
        public LayerMask collisionLayerMask = -1;

        [Tooltip("Assign any collider objects that should temporarily be enabled when collision cache is rebuilt")]
        public Collider[] tempColliders;

        public float waterHeight;
        [Tooltip("Tree item will automatically respawn after changing a parameter in the inspector")]
        public bool autoRespawnTrees = true;
        
        public delegate void OnTreeRespawn(TreePrefab prefab);
        public static event OnTreeRespawn onTreeRespawn;
        
        public delegate void OnGrassRespawn(GrassPrefab prefab);
        public static event OnGrassRespawn onGrassRespawn;
        
        public void Respawn()
        {
            if (terrains == null) return;

            SpawnAllGrass();
            SpawnAllTrees();

            foreach (Terrain terrain in terrains)
            {
                if (!terrain) continue;

                terrain.Flush();

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
                UnityEditor.EditorUtility.SetDirty(terrain);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
        }

        #region Collision cells
        public void RebuildCollisionCacheIfNeeded()
        {
            if (terrainCells.Count == 0) RebuildCollisionCache();
        }

        public void RebuildCollisionCache()
        {
            if (tempColliders != null)
            {
                for (int i = 0; i < tempColliders.Length; i++)
                {
                    if (tempColliders[i] == null) continue;
                    tempColliders[i].gameObject.SetActive(true);
                }
            }

            RaycastHit hit;

            terrainCells.Clear();

            foreach (Terrain terrain in terrains)
            {
                if(terrain.gameObject.activeInHierarchy == false) continue;
                
                int xCount = Mathf.CeilToInt(terrain.terrainData.size.x / cellSize);
                int zCount = Mathf.CeilToInt(terrain.terrainData.size.z / cellSize);

                Cell[,] cellGrid = new Cell[xCount, zCount];

                for (int x = 0; x < xCount; x++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        Vector3 wPos = new Vector3(terrain.GetPosition().x + (x * cellSize) + (cellSize * 0.5f), 0f, terrain.GetPosition().z + (z * cellSize) + (cellSize * 0.5f));

                        Vector2 normalizeTerrainPos = terrain.GetNormalizedPosition(wPos);

                        terrain.SampleHeight(normalizeTerrainPos, out _, out wPos.y, out _);

                        Cell cell = Cell.New(wPos, cellSize);
                        cell.Subdivide(cellDivisions);

                        cellGrid[x, z] = cell;

                        for (int sX = 0; sX < cellDivisions; sX++)
                        {
                            for (int sZ = 0; sZ < cellDivisions; sZ++)
                            {
                                //Sample corners of cell
                                if (highPrecisionCollision)
                                {
                                    Bounds b = cell.subCells[sX, sZ].bounds;

                                    Vector3[] corners = new Vector3[]
                                    {
                                        //BL corner
                                        new Vector3(b.min.x, b.center.y, b.min.z),
                                        //TL corner
                                        new Vector3(b.min.x, b.center.y, b.min.z + b.size.z),
                                        //BR corner
                                        new Vector3(b.max.x, b.center.y, b.min.z),
                                        //TR corner
                                        new Vector3(b.max.x, b.center.y, b.max.z),
                                    };

                                    int hitCount = corners.Length;
                                    for (int i = 0; i < corners.Length; i++)
                                    {
                                        if (Physics.Raycast(corners[i] + (Vector3.up * 100f), -Vector3.up, out hit, 150f, collisionLayerMask))
                                        {
                                            //Require to check for type, since its possible to hit a neighboring terrains
                                            if (hit.collider.GetType() == typeof(TerrainCollider))
                                            {
                                                hitCount--;
                                            }
                                        }
                                        else
                                        {
                                            hitCount--;
                                        }
                                    }

                                    //Remove cell when all rays missed
                                    if (hitCount == 0) cell.subCells[sX, sZ] = null;
                                }
                                //Sample center of cell
                                else
                                {
                                    //Remove cell if hitting terrain
                                    if (Physics.Raycast(cell.subCells[sX, sZ].bounds.center + (Vector3.up * 50f), -Vector3.up, out hit, 100f, collisionLayerMask))
                                    {
                                        if (hit.collider.gameObject == terrain.gameObject)
                                        {
                                            cell.subCells[sX, sZ] = null;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                terrainCells.Add(terrain, cellGrid);
            }

            if (tempColliders != null)
            {
                for (int i = 0; i < tempColliders.Length; i++)
                {
                    if (tempColliders[i] == null) continue;
                    tempColliders[i].gameObject.SetActive(false);
                }
            }
        }

        public bool InsideOccupiedCell(Terrain terrain, Vector3 worldPos, Vector2 normalizedPos)
        {
            if (terrainCells == null) return false;

            //No collision cells baked for terrain, user will probably notice
            if (terrainCells.ContainsKey(terrain) == false) return false;

            Cell[,] cells = terrainCells[terrain];

            Vector2Int cellIndex = Cell.PositionToCellIndex(terrain, normalizedPos, cellSize);
            Cell mainCell = cells[cellIndex.x, cellIndex.y];

            if (mainCell != null)
            {
                Cell subCell = mainCell.GetSubcell(worldPos, cellSize, cellDivisions);

                if (subCell != null)
                {
                    return true;
                }
                else
                {
                    //Cell doesn't exist
                    return false;
                }
            }
            else
            {
                Debug.LogErrorFormat("Position {0} falls outside of the cell grid", worldPos);
            }

            return false;
        }
        #endregion

        #region Trees
        private void SpawnAllTrees()
        {
            if (treeTypes == null) return;

            if (treeTypes.Count == 0) return;

            InitializeSeed();

            RefreshTreePrefabs();

            int index = 0;
            foreach (TreeType item in treeTypes)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar(" Vegetation Spawner", "Spawning trees...", (float)index / (float)grassPrefabs.Count);
#endif
                SpawnTree(item);

                index++;
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

        }

        public void RefreshTreePrefabs()
        {
            List<TreePrototype> treePrototypeCollection = new List<TreePrototype>();

            int index = 0;
            foreach (TreeType item in treeTypes)
            {
                foreach (TreePrefab p in item.prefabs)
                {
                    if (p.prefab == null) continue;

                    TreePrototype treePrototype = new TreePrototype();
                    treePrototype.prefab = p.prefab;
                    treePrototypeCollection.Add(treePrototype);
                    p.index = treePrototypeCollection.Count - 1;
                }

                index++;
            }
            foreach (Terrain terrain in terrains)
            {
                terrain.terrainData.treePrototypes = treePrototypeCollection.ToArray();

                //Ensures prototypes are persistent
                terrain.terrainData.RefreshPrototypes();
            }
        }

        public void SpawnTree(TreeType item)
        {
            if (item.collisionCheck) RebuildCollisionCacheIfNeeded();

            item.instanceCount = 0;
            RefreshTreePrefabs();

            float height, worldHeight, normalizedHeight;

            foreach (Terrain terrain in terrains)
            {
                List<TreeInstance> treeInstanceCollection = new List<TreeInstance>(terrain.terrainData.treeInstances);

                //Clear all existing instances first
                for (int i = 0; i < treeInstanceCollection.Count; i++)
                {
                    foreach (TreePrefab prefab in item.prefabs)
                    {
                        treeInstanceCollection.RemoveAll(x => x.prototypeIndex == prefab.index);
                    }
                }

                InitializeSeed(item.seed);
                item.spawnPoints = PoissonDisc.GetSpawnpoints(terrain, item.distance, item.seed + seed);

                foreach (Vector3 pos in item.spawnPoints)
                {
                    //InitializeSeed(item.seed + index);

                    //Relative position as 0-1 value
                    Vector2 normalizedPos = terrain.GetNormalizedPosition(pos);

                    if (item.collisionCheck)
                    {
                        //Check for collision
                        if (InsideOccupiedCell(terrain, pos, normalizedPos))
                        {
                            continue;
                        }
                    }

                    InitializeSeed(item.seed + (int)pos.x + (int)pos.z);
                    //Skip if failing global probability check
                    if (((Random.value * 100f) <= item.probability) == false)
                    {
                        continue;
                    }

                    TreePrefab prefab = SpawnerBase.GetProbableTree(item);

                    //Failed probability checks entirely
                    if (prefab == null) continue;

                    terrain.SampleHeight(normalizedPos, out height, out worldHeight, out normalizedHeight);

                    if (item.rejectUnderwater && worldHeight < waterHeight) continue;

                    //Check height
                    if (worldHeight < item.heightRange.x || worldHeight > item.heightRange.y)
                    {
                        continue;
                    }

                    if (item.slopeRange.x > 0 || item.slopeRange.y < 90f)
                    {
                        float slope = terrain.GetSlope(normalizedPos, false);

                        //Reject if slope check fails
                        if (!(slope >= (item.slopeRange.x + 0.001f) && slope <= (item.slopeRange.y)))
                        {
                            continue;
                        }
                    }

                    if (item.curvatureRange.x > 0 || item.curvatureRange.y < 1f)
                    {
                        float curvature = terrain.SampleConvexity(normalizedPos);
                        //0=concave, 0.5=flat, 1=convex
                        curvature = TerrainSampler.ConvexityToCurvature(curvature);
                        if (curvature < item.curvatureRange.x || curvature > item.curvatureRange.y)
                        {
                            continue;
                        }
                    }

                    //Reject based on layer masks
                    Vector2Int texelIndex = terrain.SplatmapTexelIndex(normalizedPos);

                    float spawnChance = 0f;
                    if (item.layerMasks.Count == 0) spawnChance = 100f;
                    foreach (TerrainLayerMask layer in item.layerMasks)
                    {
                        Texture2D splat = terrain.terrainData.GetAlphamapTexture(GetSplatmapID(layer.layerID));

                        Color color = splat.GetPixel(texelIndex.x, texelIndex.y);

                        int channel = layer.layerID % 4;
                        float value = SampleChannel(color, channel);

                        if (value > 0)
                        {
                            value = Mathf.Clamp01(value - layer.threshold);
                        }
                        value *= 100f;

                        spawnChance += value;

                    }
                    InitializeSeed((int)pos.x * (int)pos.z);
                    if ((Random.value <= spawnChance) == false)
                    {
                        continue;
                    }

                    //Passed all conditions, add instance
                    TreeInstance treeInstance = new TreeInstance();
                    treeInstance.prototypeIndex = prefab.index;

                    treeInstance.position = new Vector3(normalizedPos.x, normalizedHeight, normalizedPos.y);
                    treeInstance.rotation = Random.Range(0f, 359f) * Mathf.Deg2Rad;

                    float scale = Random.Range(item.scaleRange.x, item.scaleRange.y);
                    treeInstance.heightScale = scale;
                    treeInstance.widthScale = scale;

                    treeInstance.color = Color.white;
                    treeInstance.lightmapColor = Color.white;
                    
                    treeInstanceCollection.Add(treeInstance);

                    item.instanceCount++;
                }
                
               

                item.spawnPoints.Clear();

#if UNITY_2019_1_OR_NEWER
                terrain.terrainData.SetTreeInstances(treeInstanceCollection.ToArray(), false);
#else
                terrain.terrainData.treeInstances = treeInstanceCollection.ToArray();
#endif
                
            }
            
            for (int i = 0; i < item.prefabs.Count; i++)
            {
                onTreeRespawn?.Invoke(item.prefabs[i]);
            }

        }

        private TreePrototype GetTreePrototype(TreePrefab item, Terrain terrain)
        {
            return terrain.terrainData.treePrototypes[item.index];
        }

        public void UpdateTreeItem(TreeType item)
        {
            foreach (Terrain terrain in terrains)
            {
                foreach (TreePrefab p in item.prefabs)
                {
                    //Not yet added
                    if (p.index >= terrain.terrainData.treePrototypes.Length) continue;

                    if (p.prefab == null) continue;

                    if (terrain.terrainData.treePrototypes[p.index] == null) continue;

                    //Note only works when creating these copies :/
                    TreePrototype[] treePrototypes = terrain.terrainData.treePrototypes;

                    TreePrototype t = new TreePrototype();

                    t.prefab = p.prefab;

                    treePrototypes[p.index] = t;
                    terrain.terrainData.treePrototypes = treePrototypes;
                }

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(terrain);
                UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
#endif
            }
        }
#endregion

        #region Grass
        public void RefreshGrassPrototypes()
        {
            foreach (Terrain terrain in terrains)
            {
                List<DetailPrototype> grassPrototypeCollection = new List<DetailPrototype>();
                int index = 0;
                foreach (GrassPrefab item in grassPrefabs)
                {
                    item.index = index;

                    DetailPrototype detailPrototype = new DetailPrototype();

                    UpdateGrassItem(item, detailPrototype);

                    grassPrototypeCollection.Add(detailPrototype);

                    index++;
                }
                if (grassPrototypeCollection.Count > 0) terrain.terrainData.detailPrototypes = grassPrototypeCollection.ToArray();
            }
        }

        private void SpawnAllGrass()
        {
            RefreshGrassPrototypes();

            InitializeSeed();
            
            int index = 0;
            foreach (GrassPrefab item in grassPrefabs)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar(" Vegetation Spawner", "Spawning grass...", (float)index / (float)grassPrefabs.Count);
#endif
                SpawnGrass(item);

                index++;
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        public void UpdateProperties(GrassPrefab item)
        {
            foreach (Terrain terrain in terrains)
            {
                //Note only works when creating these copies :/
                DetailPrototype[] detailPrototypes = terrain.terrainData.detailPrototypes;
                DetailPrototype detailPrototype = GetGrassPrototype(item, terrain);

                UpdateGrassItem(item, detailPrototype);

                detailPrototypes[item.index] = detailPrototype;
                terrain.terrainData.detailPrototypes = detailPrototypes;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(terrain);
                UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
#endif
            }

        }

        public void SpawnGrass(GrassPrefab item)
        {
            if (item.collisionCheck) RebuildCollisionCacheIfNeeded();

            item.instanceCount = 0;

            foreach (Terrain terrain in terrains)
            {
                //2 texels per unit
                //int density = Mathf.NextPowerOfTwo(Mathf.RoundToInt(terrain.terrainData.size.x * 2f));
                //int[,] map = new int[density, density];
                //terrain.terrainData.SetDetailResolution(density, terrain.terrainData.detailResolutionPerPatch);

#if UNITY_EDITOR
                //UnityEditor.EditorUtility.DisplayProgressBar("Vegetation Spawner", "Spawning grass...", 1f);
#endif
                int[,] map = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, item.index);

                for (int x = 0; x < terrain.terrainData.detailWidth; x++)
                {
                    for (int y = 0; y < terrain.terrainData.detailHeight; y++)
                    {

                        InitializeSeed(x * y + item.seed);

                        //Default
                        int instanceCount = 1;
                        map[x, y] = 0;

                        //XZ world position
                        Vector3 wPos = terrain.DetailToWorld(y, x);
                        
                        Vector2 normalizedPos = terrain.GetNormalizedPosition(wPos);

                        if (item.collisionCheck)
                        {
                            //Check for collision
                            if (InsideOccupiedCell(terrain, wPos, normalizedPos))
                            {
                                continue;
                            }
                            /* 1 second slower
                            RaycastHit hit;
                            if (Physics.Raycast(wPos + (Vector3.up * 50f), -Vector3.up, out hit, 100f, -1))
                            {
                                if (hit.collider.gameObject != terrain.gameObject)
                                {
                                    continue;
                                }
                            }
                            */
                        }

                        //Skip if failing probability check
                        if (((Random.value * 100f) <= item.probability) == false)
                        {
                            instanceCount = 0;
                            continue;
                        }

                        terrain.SampleHeight(normalizedPos, out _, out wPos.y, out _);

                        if (item.rejectUnderwater && wPos.y < waterHeight)
                        {
                            instanceCount = 0;
                            continue;
                        }
                        //Check height
                        if (wPos.y < item.heightRange.x || wPos.y > item.heightRange.y)
                        {
                            instanceCount = 0;
                            continue;
                        }

                        if (item.slopeRange.x > 0 || item.slopeRange.y < 90)
                        {
                            float slope = terrain.GetSlope(normalizedPos);
                            //Reject if slope check fails
                            if (slope < item.slopeRange.x || slope > item.slopeRange.y)
                            {
                                instanceCount = 0;
                                continue;
                            }
                        }

                        if (item.curvatureRange.x > 0 || item.curvatureRange.y < 1f)
                        {
                            float curvature = terrain.SampleConvexity(normalizedPos);
                            //0=concave, 0.5=flat, 1=convex
                            curvature = TerrainSampler.ConvexityToCurvature(curvature);
                            if (curvature < item.curvatureRange.x || curvature > item.curvatureRange.y)
                            {
                                instanceCount = 0;
                                continue;
                            }
                        }

                        //Reject based on layer masks
                        float spawnChance = 0f;
                        Vector2Int texelIndex = terrain.SplatmapTexelIndex(normalizedPos);

                        if (item.layerMasks.Count == 0) spawnChance = 100f;
                        foreach (TerrainLayerMask layer in item.layerMasks)
                        {
                            Texture2D splat = terrain.terrainData.GetAlphamapTexture(GetSplatmapID(layer.layerID));

                            Color color = splat.GetPixel(texelIndex.x, texelIndex.y);

                            int channel = layer.layerID % 4;
                            float value = SampleChannel(color, channel);

                            if (value > 0)
                            {
                                value = Mathf.Clamp01(value - layer.threshold);
                            }
                            value *= 100f;

                            spawnChance += value;

                        }
                        InitializeSeed(x * y + item.seed);
                        if ((Random.value <= spawnChance) == false)
                        {
                            instanceCount = 0;
                        }

                        //if (instanceCount == 1) DebugPoints.Add(wPos, true, sampler.slope);
                        item.instanceCount += instanceCount;
                        //Passed all conditions, spawn one instance here
                        map[x, y] = instanceCount;
                    }
                }

                
                terrain.terrainData.SetDetailLayer(0, 0, item.index, map);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
            }
            
            onGrassRespawn?.Invoke(item);
        }

        private DetailPrototype GetGrassPrototype(GrassPrefab item, Terrain terrain)
        {
            return terrain.terrainData.detailPrototypes[item.index];
        }

        private void UpdateGrassItem(GrassPrefab item, DetailPrototype d)
        {
            d.healthyColor = item.mainColor;
            d.dryColor = item.secondairyColor;

            d.minHeight = item.minMaxHeight.x;
            d.maxHeight = item.minMaxHeight.y;

            d.minWidth = item.minMaxWidth.x;
            d.maxWidth = item.minMaxWidth.y;

            d.noiseSpread = item.noiseSize;
            d.prototype = item.prefab;
            d.prototypeTexture = item.billboard;

            if (item.type == GrassType.Mesh && item.prefab)
            {
                d.renderMode = DetailRenderMode.Grass; //Actually mesh
                d.usePrototypeMesh = true;
                d.prototype = item.prefab;
                d.prototypeTexture = null;

            }
            if (item.type == GrassType.Billboard && item.billboard)
            {
                d.renderMode = DetailRenderMode.GrassBillboard;
                d.usePrototypeMesh = false;
                d.prototypeTexture = item.billboard;
                d.prototype = null;
            }
        }
#endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (VisualizeCells)
            {
                if (terrainCells == null) return;

                foreach (KeyValuePair<Terrain, Cell[,]> item in terrainCells)
                {
                    foreach (Cell cell in item.Value)
                    {
                        if ((UnityEditor.SceneView.lastActiveSceneView.camera.transform.position - cell.bounds.center).magnitude > 150f) continue;

                        foreach (Cell subCell in cell.subCells)
                        {
                            if (subCell == null) continue;
                            Gizmos.color = new Color(1f, 0.05f, 0.05f, 1f);
                            Gizmos.DrawWireCube(new Vector3(subCell.bounds.center.x, subCell.bounds.center.y, subCell.bounds.center.z),
                                new Vector3(subCell.bounds.size.x, subCell.bounds.size.y, subCell.bounds.size.z));
                        }

                        Gizmos.color = new Color(0.66f, 0.66f, 1f, 0.25f);
                        Gizmos.DrawWireCube(
                            new Vector3(cell.bounds.center.x, cell.bounds.center.y, cell.bounds.center.z),
                            new Vector3(cell.bounds.size.x, cell.bounds.size.y, cell.bounds.size.z)
                            );
                    }
                }
            }

            if (VisualizeWaterlevel)
            {
                Gizmos.color = new Color(0f, 0.8f, 1f, 0.75f);
                
                Gizmos.DrawCube(new Vector3(UnityEditor.SceneView.lastActiveSceneView.camera.transform.position.x, waterHeight, UnityEditor.SceneView.lastActiveSceneView.camera.transform.position.z), new Vector3(250f, 0f, 250f) );
            }
        }
#endif
    }
}