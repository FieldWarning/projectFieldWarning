// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using static Staggart.VegetationSpawner.SpawnerBase;
using Random = UnityEngine.Random;
using System.Diagnostics;
using static Staggart.VegetationSpawner.VegetationSpawnerEditor;
using Debug = UnityEngine.Debug;

namespace Staggart.VegetationSpawner
{
    [CustomEditor(typeof(VegetationSpawner))]
    public class VegetationSpawnerInspector : Editor
    {
        VegetationSpawner script;
        SerializedProperty seed;
        SerializedProperty terrains;
        SerializedProperty terrainSettings;
        SerializedProperty cellSize;
        SerializedProperty cellDivisions;
        SerializedProperty collisionLayerMask;
        SerializedProperty highPrecisionCollision;
        SerializedProperty tempColliders;

        SerializedProperty waterHeight;
        private SerializedProperty autoRespawnTrees;
        
        private Stopwatch sw;

        SerializedProperty treeProbabilty;

        private void OnEnable()
        {
            //settingEditor = new Editor();
            script = (VegetationSpawner)target;

            seed = serializedObject.FindProperty("seed");
            terrains = serializedObject.FindProperty("terrains");
            terrainSettings = serializedObject.FindProperty("terrainSettings");
            cellSize = serializedObject.FindProperty("cellSize");
            cellDivisions = serializedObject.FindProperty("cellDivisions");
            collisionLayerMask = serializedObject.FindProperty("collisionLayerMask");
            highPrecisionCollision = serializedObject.FindProperty("highPrecisionCollision");
            tempColliders = serializedObject.FindProperty("tempColliders");

            waterHeight = serializedObject.FindProperty("waterHeight");
            autoRespawnTrees = serializedObject.FindProperty("autoRespawnTrees");

            VegetationSpawner.VisualizeCells = VisualizeCellsPersistent;

            Undo.undoRedoPerformed += OnUndoTree;

        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoTree;
        }

        private const float thumbSize = 100f;
        private const float texThumbSize = 50f;
        private int selectedLayerID;
        private Vector2 treeScrollPos;
        private Vector2 grassScrollPos;
        private Editor settingEditor;
        private Vector2 texScrollview;
        private float previewSize;
        private Texture2D previewTex;

        private static int TabID
        {
            get { return SessionState.GetInt("VegetationSpawnerInspector_TAB", 0); }
            set { SessionState.SetInt("VegetationSpawnerInspector_TAB", value); }
        }

        private static int selectedGrassID
        {
            get { return SessionState.GetInt("VegetationSpawnerInspector_selectedGrassID", 0); }
            set { SessionState.SetInt("VegetationSpawnerInspector_selectedGrassID", value); }
        }

        private static int selectedTreeID
        {
            get { return SessionState.GetInt("VegetationSpawnerInspector_selectedTreeID", 0); }
            set { SessionState.SetInt("VegetationSpawnerInspector_selectedTreeID", value); }
        }

        private static bool VisualizeCellsPersistent
        {
            get { return SessionState.GetBool("VegetationSpawnerInspector_VisualizeCells", false); }
            set { SessionState.SetBool("VegetationSpawnerInspector_VisualizeCells", value); }
        }

        private static bool ShowLog
        {
            get { return SessionState.GetBool("VegetationSpawnerInspector_ShowLog", false); }
            set { SessionState.SetBool("VegetationSpawnerInspector_ShowLog", value); }
        }
        private Vector2 logScrollPos;

        public override void OnInspectorGUI()
        {
            Rect versionRect = EditorGUILayout.GetControlRect();
            versionRect.y -= versionRect.height;
            versionRect.x = 240f;
            GUI.Label(versionRect, "Version " + VegetationSpawner.Version, EditorStyles.miniLabel);
            GUILayout.Space(-17f);
            
            serializedObject.Update();

            EditorGUILayout.Space();

            if (script.terrains.Count > 0)
            {
                TabID = GUILayout.Toolbar(TabID, new GUIContent[] {
                new GUIContent("Terrain", VegetationSpawnerEditor.TerrainIcon),
                new GUIContent("Trees", VegetationSpawnerEditor.TreeIcon),
                new GUIContent("Grass", VegetationSpawnerEditor.DetailIcon),
                new GUIContent("Settings", EditorGUIUtility.IconContent("d_Settings").image)
            }, GUILayout.Height(30f));

                EditorGUILayout.Space();

                
                EditorGUI.BeginChangeCheck();
                switch (TabID)
                {
                    case 0:
                        DrawTerrain();
                        break;
                    case 1:
                        DrawTrees();
                        break;
                    case 2:
                        DrawGrass();
                        break;
                    case 3:
                        DrawSettings();
                        break;
                }


                VegetationSpawner.VisualizeCells = TabID == 3;
                VegetationSpawner.VisualizeWaterlevel = TabID == 3;
                

                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.HelpBox("Assign terrains to spawn on", MessageType.Info);
                DrawTerrain();
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Vegetation already placed on the terrain will be cleared!", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                ShowLog = GUILayout.Toggle(ShowLog, "▼", "Button", GUILayout.MaxWidth(30f));
                EditorGUILayout.LabelField("Log", EditorStyles.boldLabel, GUILayout.MaxWidth(35f));
            }

            if (ShowLog)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.textArea, UnityEngine.GUILayout.MaxHeight(100f)))
                {
                    logScrollPos = EditorGUILayout.BeginScrollView(logScrollPos);

                    for (int i = VegetationSpawnerEditor.Log.items.Count - 1; i >= 0; i--)
                    {
                        EditorGUILayout.LabelField(VegetationSpawnerEditor.Log.items[i], EditorStyles.miniLabel);
                    }

                    logScrollPos.y += 10f;

                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.LabelField("- Staggart Creations -", EditorStyles.centeredGreyMiniLabel);

        }

        private void DrawTerrain()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(terrains, true);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add active terrains"))
                {
                    Terrain[] activeTerrains = Terrain.activeTerrains;

                    for (int i = 0; i < activeTerrains.Length; i++)
                    {
                        if (script.terrains.Contains(activeTerrains[i]) == false) script.terrains.Add(activeTerrains[i]);
                    }

                    for (int i = 0; i < activeTerrains.Length; i++)
                    {
                        script.maxTerrainHeight = Mathf.Max(script.maxTerrainHeight, script.terrains[i].terrainData.bounds.size.y);
                    }
                    
                    script.RebuildCollisionCache();
                }

                if (GUILayout.Button("Clear"))
                {
                    script.terrains.Clear();
                }
            }

            if (script.terrains.Count == 0) return;

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            terrainSettings.isExpanded = true;

            EditorGUILayout.PropertyField(terrainSettings, true);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();
                script.CopySettingsToTerrains();
            }
        }

        private void DrawTrees()
        {
            EditorGUILayout.LabelField("Species", EditorStyles.boldLabel);

            //Tree item view
            treeScrollPos = EditorGUILayout.BeginScrollView(treeScrollPos, EditorStyles.textArea, GUILayout.Height(thumbSize + 10f));
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < script.treeTypes.Count; i++)
                {
                    if (script.treeTypes[i] == null) continue;

                    Texture2D thumb = EditorGUIUtility.IconContent("d_BuildSettings.Broadcom").image as Texture2D;

                    if (script.treeTypes[i].prefabs.Count > 0)
                    {
                        if (script.treeTypes[i].prefabs[0] != null)
                        {
                            if (script.treeTypes[i].prefabs[0].prefab) thumb = AssetPreview.GetAssetPreview(script.treeTypes[i].prefabs[0].prefab);
                        }
                    }

                    if (GUILayout.Button(new GUIContent("", thumb), (selectedTreeID == i) ? VegetationSpawnerEditor.PreviewTexSelected : VegetationSpawnerEditor.PreviewTex, GUILayout.MinHeight(thumbSize), GUILayout.MaxWidth(thumbSize), GUILayout.MaxHeight(thumbSize)))
                    {
                        selectedTreeID = i;
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            Undo.RecordObject(script, "Modified tree species");

            serializedObject.Update();
            using (var treeChange = new EditorGUI.ChangeCheckScope())
            {
                //Tree type view options
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Add", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image, "Add new item")))
                    {
                        VegetationSpawner.TreeType tree = TreeType.New();

                        script.treeTypes.Add(tree);
                        selectedTreeID = script.treeTypes.Count - 1;
                    }
                    if (script.treeTypes.Count > 0)
                    {
                        if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Remove")))
                        {
                            script.treeTypes.RemoveAt(selectedTreeID);
                            selectedTreeID = script.treeTypes.Count - 1;

                            if (selectedTreeID < 0) selectedTreeID = 0;

                            script.RefreshTreePrefabs();
                        }
                    }
                }

                //Settings for selected


                if (script.treeTypes.Count > 0)
                {
                    VegetationSpawner.TreeType tree = script.treeTypes[selectedTreeID];

                    EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
                    if (tree.prefabs.Count == 0) EditorGUILayout.HelpBox("Add a tree prefab first", MessageType.Info);

                    for (int p = 0; p < tree.prefabs.Count; p++)
                    {
                        TreePrefab item = tree.prefabs[p];

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUI.BeginChangeCheck();
                                item.prefab = EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(GameObject), true) as GameObject;

                                if (item.prefab)
                                {
                                    if (EditorUtility.IsPersistent(item.prefab) == false) EditorGUILayout.HelpBox("Prefab cannot be a scene instance", MessageType.Error);
                                }
                                item.probability = EditorGUILayout.Slider("Spawn chance %", item.probability, 0f, 100f);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    script.UpdateTreeItem(tree);
                                    if(autoRespawnTrees.boolValue) script.SpawnTree(tree);
                                    EditorUtility.SetDirty(target);
                                }
                            }

                            if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Remove")))
                            {
                                tree.prefabs.RemoveAt(p);

                                script.RefreshTreePrefabs();
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(new GUIContent("Add", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image, "Add new item")))
                        {
                            TreePrefab p = new TreePrefab();
                            p.probability = 100f;

                            tree.prefabs.Add(p);

                            script.RefreshTreePrefabs();
                        }
                    }

                    if (tree.prefabs.Count > 0)
                    {
                        if (tree.prefabs[0].prefab != null)
                        {
                            EditorGUILayout.LabelField("Spawn rules", EditorStyles.boldLabel);

                            EditorGUI.BeginChangeCheck();

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                tree.seed = EditorGUILayout.IntField("Seed", tree.seed, GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 50f));
                                if (GUILayout.Button("Randomize", GUILayout.MaxWidth(100f)))
                                {
                                    tree.seed = Random.Range(0, 99999);
                                }
                            }
                            tree.probability = EditorGUILayout.Slider("Global spawn chance %", tree.probability, 0f, 100f);
                            tree.distance = EditorGUILayout.Slider("Distance between", tree.distance, 0.5f, 50f);
                            VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Scale", "Scale is randomly selected from this range"), ref tree.scaleRange, 0f, 2f);
                            tree.sinkAmount = EditorGUILayout.Slider(new GUIContent("Sink amount", "Lowers the Y position of the tree"), tree.sinkAmount, 0f, 1f);

                            EditorGUILayout.Space();
                            tree.collisionCheck = EditorGUILayout.Toggle("Collision check", tree.collisionCheck);

                            tree.rejectUnderwater = EditorGUILayout.Toggle(new GUIContent("Remove underwater", "The water height level can be set in the settings tab"), tree.rejectUnderwater);
                            VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Height range", "Min/max height this item can spawn at"), ref tree.heightRange, 0f, script.maxTerrainHeight);
                            VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Slope range", "Min/max slope (0-90 degrees) this item can spawn at"), ref tree.slopeRange, 0f, 90f);
                            VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Curvature range", "0=Concave (bowl), 0.5=flat, 1=convex (edge)"), ref tree.curvatureRange, 0f, 1f);
                           
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!autoRespawnTrees.boolValue) return;
                                
                                Stopwatch sw = new Stopwatch();
                                sw.Restart();
                                 script.SpawnTree(tree);
                                sw.Stop();

                                Log.Add("Respawning tree: " + sw.Elapsed.Milliseconds + "ms...");

                                EditorUtility.SetDirty(target);
                            }

                            LayerMaskSettings(tree.layerMasks);

                            EditorGUILayout.LabelField("Instances: " + tree.instanceCount.ToString("##,#"), EditorStyles.miniLabel);

                            if(autoRespawnTrees.boolValue == false) EditorGUILayout.HelpBox("Auto respawning is disabled for trees in the settings tab", MessageType.Warning);

                            if (GUILayout.Button(new GUIContent(" Respawn", EditorGUIUtility.IconContent("d_Refresh").image), GUILayout.Height(30f)))
                            {
                                Stopwatch sw = new Stopwatch();
                                sw.Restart();
                                script.SpawnTree(tree);
                                sw.Stop();

                                Log.Add("Respawning tree: " + sw.Elapsed.Milliseconds + "ms...");
                            }
                            
                        }
                    }
                }

                if (treeChange.changed)
                {
                    EditorUtility.SetDirty(target);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawGrass()
        {
            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

            grassScrollPos = EditorGUILayout.BeginScrollView(grassScrollPos, EditorStyles.textArea, GUILayout.Height(thumbSize + 10f));
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < script.grassPrefabs.Count; i++)
                {
                    Texture2D thumb = AssetPreview.GetAssetPreview(script.grassPrefabs[i].prefab);
                    if (script.grassPrefabs[i].type == SpawnerBase.GrassType.Billboard) thumb = script.grassPrefabs[i].billboard;
                    if (thumb == null) thumb = EditorGUIUtility.IconContent("d_BuildSettings.Broadcom").image as Texture2D;

                    if (GUILayout.Button(new GUIContent("", thumb), (selectedGrassID == i) ? VegetationSpawnerEditor.PreviewTexSelected : VegetationSpawnerEditor.PreviewTex, GUILayout.MinHeight(thumbSize), GUILayout.MaxWidth(thumbSize), GUILayout.MaxHeight(thumbSize)))
                    {
                        selectedGrassID = i;
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            //Edge case: Clamp in case there's a switch to scene with less items
            selectedGrassID = Mathf.Min(selectedGrassID, script.grassPrefabs.Count - 1);
            
            VegetationSpawner.GrassPrefab grass = script.grassPrefabs.Count > 0 ? script.grassPrefabs[selectedGrassID] : null;

            using (new EditorGUILayout.HorizontalScope())
            {
                if(grass != null) EditorGUILayout.LabelField("Instances: " + grass.instanceCount.ToString("##,#"), EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("Add", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image, "Add new item")))
                {
                    VegetationSpawner.GrassPrefab newGrass = new SpawnerBase.GrassPrefab();
                    script.grassPrefabs.Add(newGrass);

                    newGrass.seed = Random.Range(0, 9999);
                    selectedGrassID = script.grassPrefabs.Count - 1;
                    newGrass.index = script.grassPrefabs.Count;

                    script.RefreshGrassPrototypes();
                }

                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Duplicate item")))
                {
                    GrassPrefab newGrass = Duplicate(script.grassPrefabs[selectedGrassID]);

                    script.grassPrefabs.Add(newGrass);
                    selectedGrassID = script.grassPrefabs.Count - 1;
                    newGrass.index = script.grassPrefabs.Count;

                    script.RefreshGrassPrototypes();
                    
                    //Spawn it now so changes are visible
                    script.SpawnGrass(script.grassPrefabs[selectedGrassID]);
                }

                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Remove")))
                {
                    script.grassPrefabs.RemoveAt(selectedGrassID);

                    selectedGrassID = script.grassPrefabs.Count - 1;

                    script.RefreshGrassPrototypes();
                }
            }

            if (grass != null)
            {
                Undo.RecordObject(script, "Modified grass item");

                serializedObject.Update();
                using (var grassChange = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

                    grass.type = (SpawnerBase.GrassType)EditorGUILayout.Popup("Render type", (int)grass.type, new string[] { "Mesh", "Billboard" });

                    if (grass.type == SpawnerBase.GrassType.Mesh) grass.prefab = EditorGUILayout.ObjectField("Prefab", grass.prefab, typeof(GameObject), true) as GameObject;
                    if (grass.type == SpawnerBase.GrassType.Billboard) grass.billboard = EditorGUILayout.ObjectField("Billboard", grass.billboard, typeof(Texture2D), true) as Texture2D;

                    grass.mainColor = EditorGUILayout.ColorField("Main", grass.mainColor);
                    grass.linkColors = EditorGUILayout.Toggle(new GUIContent("Link", "Set the main and secondary color with one value"), grass.linkColors);

                    if (grass.linkColors)
                    {
                        grass.secondairyColor = grass.mainColor;
                    }
                    else
                    {
                        grass.secondairyColor = EditorGUILayout.ColorField("Secondary", grass.secondairyColor);
                    }
                    VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Width", "Min/max width of the mesh"), ref grass.minMaxWidth, 0f, 2f);
                    VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Length", "Min/max length of the mesh"), ref grass.minMaxHeight, 0f, 2f);

                    if (grassChange.changed)
                    {
                        EditorUtility.SetDirty(target);
                        script.UpdateProperties(grass);
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Spawning rules", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        grass.seed = EditorGUILayout.IntField("Seed", grass.seed, GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 50f));
                        if (GUILayout.Button("Randomize", GUILayout.MaxWidth(100f)))
                        {
                            grass.seed = Random.Range(0, 99999);
                        }
                    }

                    grass.probability = EditorGUILayout.Slider("Spawn chance %", grass.probability, 0f, 100f);
                    
                    EditorGUILayout.Space();
                    
                    grass.collisionCheck = EditorGUILayout.Toggle(new GUIContent("Collision check", "Take into account the collision cache to avoid spawning inside colliders (see Settings tab)"), grass.collisionCheck);
                    grass.rejectUnderwater = EditorGUILayout.Toggle(new GUIContent("Remove underwater", "The water height level can be set in the settings tab"), grass.rejectUnderwater);
                    VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Height range", "Min/max height this item can spawn at"), ref grass.heightRange, 0f, 1000f);
                    VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Slope range", "Min/max slope (0-90 degrees) this item can spawn at"), ref grass.slopeRange, 0f, 90f);
                    VegetationSpawnerEditor.DrawRangeSlider(new GUIContent("Curvature range", "0=Concave (bowl), 0.5=flat, 1=convex (edge)"), ref grass.curvatureRange, 0f, 1f);

                    EditorGUILayout.Space();

                    LayerMaskSettings(grass.layerMasks);

                    if (grassChange.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent(" Respawn", EditorGUIUtility.IconContent("d_Refresh").image), GUILayout.Height(30f)))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Restart();

                    script.SpawnGrass(script.grassPrefabs[selectedGrassID]);

                    sw.Stop();

                    Log.Add("Respawning grass: " + sw.Elapsed.Seconds + " seconds...");
                }
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Spawning", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                seed.intValue = EditorGUILayout.IntField("Seed", seed.intValue, GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 50f));
                if (GUILayout.Button("Randomize", GUILayout.MaxWidth(100f)))
                {
                    script.RandomizeSeed();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();
            }


            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(" ");
                
                if (GUILayout.Button(new GUIContent(" Respawn everything",
                    EditorGUIUtility.IconContent("d_Refresh").image)))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Restart();
                    script.Respawn();

                    sw.Stop();

                    Log.Add("Complete respawn: " + sw.Elapsed.Seconds + " seconds...");
                }
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(autoRespawnTrees);
            EditorGUILayout.PropertyField(waterHeight);

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Collision cache", EditorStyles.boldLabel);

            script.RebuildCollisionCacheIfNeeded();
            
            VisualizeCellsPersistent = VegetationSpawner.VisualizeCells;

            EditorGUILayout.PropertyField(cellSize);
            EditorGUILayout.PropertyField(cellDivisions);
            EditorGUILayout.PropertyField(highPrecisionCollision);
            EditorGUILayout.PropertyField(collisionLayerMask);
            EditorGUILayout.PropertyField(tempColliders, true);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Rebuild cache"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Restart();
                    script.RebuildCollisionCache();
                    sw.Stop();

                    Log.Add("Rebuilding collision cache: " + sw.Elapsed.Milliseconds + "ms...");
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();
            }
        }

        private Texture2D GetLayerMainTex(int index)
        {
            if (script.terrains.Count == 0) return EditorGUIUtility.IconContent("d_BuildSettings.Broadcom").image as Texture2D;

            TerrainLayer layer = script.terrains[0].terrainData.terrainLayers[index];

            if (!layer) return EditorGUIUtility.IconContent("d_BuildSettings.Broadcom").image as Texture2D;

            return layer.diffuseTexture ? layer.diffuseTexture : Texture2D.whiteTexture;
        }

        private Vector2 terrainLayerScrollPos;
        private void LayerMaskSettings(List<TerrainLayerMask> masks)
        {
            EditorGUILayout.LabelField("Layer masks", EditorStyles.boldLabel);

            if (masks.Count > 0)
            {
                terrainLayerScrollPos = EditorGUILayout.BeginScrollView(terrainLayerScrollPos, EditorStyles.textArea, GUILayout.Height(texThumbSize + 10f));

                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int i = 0; i < masks.Count; i++)
                    {
                        if (i < masks.Count && masks[i] != null)
                        {
                            //Select preview texture
                            if (masks[i] != null) previewTex = GetLayerMainTex(masks[i].layerID);

                            if (GUILayout.Button(new GUIContent("", previewTex), (selectedLayerID == masks[i].layerID) ? VegetationSpawnerEditor.PreviewTexSelected : VegetationSpawnerEditor.PreviewTex,
                            GUILayout.MaxWidth(texThumbSize), GUILayout.MaxHeight(texThumbSize)))
                            {
                                selectedLayerID = i;
                            }
                        }
                    }
                }
                
                EditorGUILayout.EndScrollView();

                TerrainLayerMask selected = masks[selectedLayerID];

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Add", VegetationSpawnerEditor.PlusIcon, "Add new item")))
                    {
                        LayerDropDown(masks);
                    }
                    

                    if (GUILayout.Button(new GUIContent("", VegetationSpawnerEditor.TrashIcon, "Remove")))
                    {
                        masks.Remove(selected);
                        selectedLayerID = masks.Count - 1;
                    }
                }

                if (selected != null)
                {
                    EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

                    selected.threshold = EditorGUILayout.Slider(new GUIContent("Minimum strength", "The minimum strength the material must have underneath the item, before it will spawn"), selected.threshold, 0f, 1f);
                }

            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Add", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image, "Add new item")))
                    {
                        LayerDropDown(masks);
                    }
                }
            }
        }

        private void LayerDropDown(List<TerrainLayerMask> masks)
        {
            if (script.terrains.Count == 0) Debug.LogError("No terrains assigned");

            contextMasks = masks;

            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < script.terrains[0].terrainData.terrainLayers.Length; i++)
            {
                if(script.terrains[0].terrainData.terrainLayers[i] == null) continue;
                
                //Check if layer already added
                if (masks.Find(x => x.layerID == i) == null)
                    menu.AddItem(new GUIContent(script.terrains[0].terrainData.terrainLayers[i].name), false, AddTerrainLayerMask, i);
            }
            menu.ShowAsContext();
        }

        private List<TerrainLayerMask> contextMasks;

        private void AddTerrainLayerMask(object id)
        {
            TerrainLayerMask m = new TerrainLayerMask();
            m.layerID = (int)id;

            contextMasks.Add(m);
            selectedLayerID = contextMasks.Count - 1;
        }

        private GrassPrefab Duplicate(GrassPrefab source)
        {
            GrassPrefab dest = new GrassPrefab();

            dest.type = source.type;
            dest.prefab = source.prefab;
            dest.billboard = source.billboard;

            dest.mainColor = source.mainColor;
            dest.secondairyColor = source.secondairyColor;
            dest.linkColors = source.linkColors;

            dest.minMaxHeight = source.minMaxHeight;
            dest.minMaxWidth = source.minMaxWidth;

            //Base
            dest.seed = source.seed;
            dest.probability = source.probability;
            dest.heightRange = source.heightRange;
            dest.slopeRange = source.slopeRange;
            dest.curvatureRange = source.curvatureRange;

            dest.layerMasks = new List<TerrainLayerMask>(source.layerMasks);
            for (int i = 0; i < dest.layerMasks.Count; i++)
            {
                dest.layerMasks[i] = new TerrainLayerMask(source.layerMasks[i].layerID, source.layerMasks[i].threshold);
            }
            
            dest.collisionCheck = source.collisionCheck;
            dest.rejectUnderwater = source.rejectUnderwater;

            return dest;
        }

        private void OnUndoTree()
        {
            //Terrain
            if (TabID == 0)
            {
                script.CopySettingsToTerrains();
            }
            //Tree
            if (TabID == 1)
            {
                if(autoRespawnTrees.boolValue) script.SpawnTree(script.treeTypes[selectedTreeID]);
            }
            //Grass
            if (TabID == 2)
            {
                script.UpdateProperties(script.grassPrefabs[selectedGrassID]);
            }
        }
    }
}