//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
   
   [CustomEditor(typeof(TextureArrayConfig))]
   public class TextureArrayConfigEditor : Editor
   {
      static GUIContent CPlatformOverrides = new GUIContent("Platform Compression Overrides", "Override the compression type on a per platform basis");
      static GUIContent CTextureMode = new GUIContent("Texturing Mode", "Do you have just diffuse and normal, or a fully PBR pipeline with height, smoothness, and ao textures?");
      static GUIContent CSourceTextureSize = new GUIContent("Source Texture Size", "Reduce source texture size to save memory in builds");
      static GUIContent CPackingMode = new GUIContent("Packing Mode", "Pack textures into two arrays (fastest) or three (best quality)");
#if __MICROSPLAT_TEXTURECLUSTERS__
      static GUIContent CClusterMode = new GUIContent("Cluster Mode", "Add extra slots for packing parallel arrays for texture clustering");
#endif
#if __MICROSPLAT_DETAILRESAMPLE__
      static GUIContent CAntiTileArray = new GUIContent("AntiTile Array", "Create an array for each texture to have it's own Noise Normal, Detail, and Distance noise texture");
#endif

      static GUIContent CEmisMetalArray = new GUIContent("Emissive/Metal array", "Create a texture array for emissive and metallic materials");

      static GUIContent CDiffuse = new GUIContent("Diffuse", "Diffuse or Albedo texture");
      static GUIContent CNormal = new GUIContent("Normal", "Normal map");
      static GUIContent CAO = new GUIContent("AO", "Ambient Occlusion map");
      static GUIContent CSmoothness = new GUIContent("Smoothness", "Smoothness map, or roughness map with invert on");
      static GUIContent CHeight = new GUIContent("Height", "Height Map");
      static GUIContent CAlpha = new GUIContent("Alpha", "Alpha Map");
      static GUIContent CNoiseNormal = new GUIContent("Noise Normal", "Normal to bend in over a larger area");
      static GUIContent CDetailNoise = new GUIContent("Detail", "Noise texture to blend in when close");
      static GUIContent CDistanceNoise = new GUIContent("Distance", "Noise texture to blend in when far away");

      void DrawHeader(TextureArrayConfig cfg)
      {
         if (cfg.textureMode != TextureArrayConfig.TextureMode.Basic)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("", GUILayout.Width(30));
            EditorGUILayout.LabelField("Channel", GUILayout.Width(64));
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(20));
            EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(64));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(CHeight, GUILayout.Width(64));
            cfg.allTextureChannelHeight = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelHeight, GUILayout.Width(64));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(CSmoothness, GUILayout.Width(64));
            cfg.allTextureChannelSmoothness = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelSmoothness, GUILayout.Width(64));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (cfg.IsAdvancedDetail())
            {
               EditorGUILayout.LabelField(CAlpha, GUILayout.Width(64));
               cfg.allTextureChannelAlpha = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAlpha, GUILayout.Width(64));
            }
            else
            {         
               EditorGUILayout.LabelField(CAO, GUILayout.Width(64));
               cfg.allTextureChannelAO = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAO, GUILayout.Width(64));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));
         }
      }

      void DrawAntiTileEntry(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry e, int i)
      {
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();EditorGUILayout.Space();
         EditorGUILayout.BeginVertical();

         EditorGUILayout.LabelField(CNoiseNormal, GUILayout.Width(92));
         e.noiseNormal = (Texture2D)EditorGUILayout.ObjectField(e.noiseNormal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(CDetailNoise, GUILayout.Width(92));
         e.detailNoise = (Texture2D)EditorGUILayout.ObjectField(e.detailNoise, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         e.detailChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.detailChannel, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(CDistanceNoise, GUILayout.Width(92));
         e.distanceNoise = (Texture2D)EditorGUILayout.ObjectField(e.distanceNoise, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         e.distanceChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.distanceChannel, GUILayout.Width(64));
         EditorGUILayout.EndVertical();


         EditorGUILayout.EndHorizontal();

         if (e.noiseNormal == null)
         {
            int index = (int)Mathf.Repeat(i, 3);
            e.noiseNormal = MicroSplatUtilities.GetAutoTexture("microsplat_def_detail_normal_0" + (index+1).ToString());
         }

      }

      void SwapEntry(TextureArrayConfig cfg, int src, int targ)
      {
         if (src >= 0 && targ >= 0 && src < cfg.sourceTextures.Count && targ < cfg.sourceTextures.Count)
         {
            {
               var s = cfg.sourceTextures[src];
               cfg.sourceTextures[src] = cfg.sourceTextures[targ];
               cfg.sourceTextures[targ] = s;
            }
            if (cfg.sourceTextures2.Count == cfg.sourceTextures.Count)
            {
               var s = cfg.sourceTextures2[src];
               cfg.sourceTextures2[src] = cfg.sourceTextures2[targ];
               cfg.sourceTextures2[targ] = s;
            }
            if (cfg.sourceTextures3.Count == cfg.sourceTextures.Count)
            {
               var s = cfg.sourceTextures3[src];
               cfg.sourceTextures3[src] = cfg.sourceTextures3[targ];
               cfg.sourceTextures3[targ] = s;
            }

         }
         
      }

      bool DrawTextureEntry(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry e, int i, bool controls = true)
      {
         bool ret = false;
         if (controls)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));

               
            if (e.HasTextures())
            {
               
               EditorGUILayout.LabelField(e.diffuse != null ? e.diffuse.name : "empty");
               ret = GUILayout.Button("Clear Entry");
            }
            else
            {
               EditorGUILayout.HelpBox("Removing an entry completely can cause texture choices to change on existing terrains. You can leave it blank to preserve the texture order and MicroSplat will put a dummy texture into the array.", MessageType.Warning);
               ret = (GUILayout.Button("Delete Entry"));
            }

            if (GUILayout.Button("Up", GUILayout.Width(40)))
            {
               SwapEntry(cfg, i, i - 1);
            }
            if (GUILayout.Button("Down", GUILayout.Width(40)))
            {
               SwapEntry(cfg, i, i + 1);
            }
            EditorGUILayout.EndHorizontal();
         }

         EditorGUILayout.BeginHorizontal();

         if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR)
         {
#if !UNITY_2017_3_OR_NEWER
            EditorGUILayout.BeginVertical();
            if (controls)
            {
               EditorGUILayout.LabelField(new GUIContent("Substance"), GUILayout.Width(64));
            }
            e.substance = (ProceduralMaterial)EditorGUILayout.ObjectField(e.substance, typeof(ProceduralMaterial), false, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.EndVertical();
#endif
#if SUBSTANCE_PLUGIN_ENABLED
            EditorGUILayout.BeginVertical();
            if (controls)
            {
               EditorGUILayout.LabelField(new GUIContent("Substance"), GUILayout.Width(64));
            }
            e.substance = (Substance.Game.Substance)EditorGUILayout.ObjectField(e.substance, typeof(Substance.Game.Substance), false, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.EndVertical();
#endif
         }

         EditorGUILayout.BeginVertical();
         if (controls)
         {
            EditorGUILayout.LabelField(CDiffuse, GUILayout.Width(64));
         }
         e.diffuse = (Texture2D)EditorGUILayout.ObjectField(e.diffuse, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         if (controls)
         {
            EditorGUILayout.LabelField(CNormal, GUILayout.Width(64));
         }
         e.normal = (Texture2D)EditorGUILayout.ObjectField(e.normal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();

         if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR || cfg.IsAdvancedDetail())
         {
            EditorGUILayout.BeginVertical();
            if (controls)
            {
               EditorGUILayout.LabelField(CHeight, GUILayout.Width(64));
            }
            e.height = (Texture2D)EditorGUILayout.ObjectField(e.height, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            if (cfg.allTextureChannelHeight == TextureArrayConfig.AllTextureChannel.Custom)
            {
               e.heightChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.heightChannel, GUILayout.Width(64));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (controls)
            {
               EditorGUILayout.LabelField(CSmoothness, GUILayout.Width(64));
            }
            e.smoothness = (Texture2D)EditorGUILayout.ObjectField(e.smoothness, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
            if (cfg.allTextureChannelSmoothness == TextureArrayConfig.AllTextureChannel.Custom)
            {
               e.smoothnessChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.smoothnessChannel, GUILayout.Width(64));
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Invert", GUILayout.Width(44));
            e.isRoughness = EditorGUILayout.Toggle(e.isRoughness, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (cfg.IsAdvancedDetail())
            {
               if (controls)
               {
                  EditorGUILayout.LabelField(CAlpha, GUILayout.Width(64));
               }
               e.alpha = (Texture2D)EditorGUILayout.ObjectField(e.alpha, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
               if (cfg.allTextureChannelAlpha == TextureArrayConfig.AllTextureChannel.Custom)
               {
                  e.alphaChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.alphaChannel, GUILayout.Width(64));
               }
               EditorGUILayout.BeginHorizontal();
               EditorGUILayout.LabelField("Normalize", GUILayout.Width(64));
               e.normalizeAlpha = EditorGUILayout.Toggle(e.normalizeAlpha, GUILayout.Width(44));
               EditorGUILayout.EndHorizontal();               
            }
            else
            {
               if (controls)
               {
                  EditorGUILayout.LabelField(CAO, GUILayout.Width(64));
               }
               e.ao = (Texture2D)EditorGUILayout.ObjectField(e.ao, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
               if (cfg.allTextureChannelAO == TextureArrayConfig.AllTextureChannel.Custom)
               {
                  e.aoChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.aoChannel, GUILayout.Width(64));
               }
            }
            EditorGUILayout.EndVertical();

            if (!cfg.IsAdvancedDetail() && cfg.emisMetalArray)
            {
               EditorGUILayout.BeginVertical();
               if (controls)
               {
                  EditorGUILayout.LabelField(new GUIContent("Emissive"), GUILayout.Width(64));
               }
               e.emis = (Texture2D)EditorGUILayout.ObjectField(e.emis, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
               EditorGUILayout.EndVertical();

               EditorGUILayout.BeginVertical();
               if (controls)
               {
                  EditorGUILayout.LabelField(new GUIContent("Metal"), GUILayout.Width(64));
               }
               e.metal = (Texture2D)EditorGUILayout.ObjectField(e.metal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
               e.metalChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.metalChannel, GUILayout.Width(64));

               EditorGUILayout.EndVertical();
            }
         }
         EditorGUILayout.EndHorizontal();

         return ret;
      }

      public static bool GetFromTerrain(TextureArrayConfig cfg, Terrain t)
      {
         if (t != null && cfg.sourceTextures.Count == 0 && t.terrainData != null)
         {
#if UNITY_2018_3_OR_NEWER
            int count = t.terrainData.terrainLayers.Length;
            for (int i = 0; i < count; ++i)
            {
               // Metalic, AO, Height, Smooth
               var proto = t.terrainData.terrainLayers[i];
               var e = new TextureArrayConfig.TextureEntry();
               e.diffuse = proto.diffuseTexture;
               e.normal = proto.normalMapTexture;
               e.metal = proto.maskMapTexture;
               e.metalChannel = TextureArrayConfig.TextureChannel.R;
               e.height = proto.maskMapTexture;
               e.heightChannel = TextureArrayConfig.TextureChannel.B;
               e.smoothness = proto.maskMapTexture;
               e.smoothnessChannel = TextureArrayConfig.TextureChannel.A;
               e.ao = proto.maskMapTexture;
               e.aoChannel = TextureArrayConfig.TextureChannel.G;
               cfg.sourceTextures.Add(e);
            }
            return true;
#else
            int count = t.terrainData.splatPrototypes.Length;
            for (int i = 0; i < count; ++i)
            {
               var proto = t.terrainData.splatPrototypes[i];
               var e = new TextureArrayConfig.TextureEntry();
               e.diffuse = proto.texture;
               e.normal = proto.normalMap;
               cfg.sourceTextures.Add(e);
            }
            return true;
#endif
         }
         return false;
      }

      static void GetFromTerrain(TextureArrayConfig cfg)
      {
         Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
         for (int x = 0; x < terrains.Length; ++x)
         {
            var t = terrains[x];
            if (GetFromTerrain(cfg, t))
               return;
         }
      }

      public static TextureArrayConfig CreateConfig(string path)
      {
         string configPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplatConfig.asset");
         TextureArrayConfig cfg = TextureArrayConfig.CreateInstance<TextureArrayConfig>();

         AssetDatabase.CreateAsset(cfg, configPath);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
         cfg = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(configPath);
         CompileConfig(cfg);
         return cfg;
      }

      public static TextureArrayConfig CreateConfig(Terrain t)
      {
         string path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
         string configPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplatConfig.asset");
         TextureArrayConfig cfg = TextureArrayConfig.CreateInstance<TextureArrayConfig>();
         GetFromTerrain(cfg, t);

         AssetDatabase.CreateAsset(cfg, configPath);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
         cfg = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(configPath);
         CompileConfig(cfg);
         return cfg;

      }


      void Remove(TextureArrayConfig cfg, int i)
      {
         cfg.sourceTextures.RemoveAt(i);
         cfg.sourceTextures2.RemoveAt(i);
         cfg.sourceTextures3.RemoveAt(i);
      }

      void Reset(TextureArrayConfig cfg, int i)
      {
         cfg.sourceTextures[i].Reset();
         cfg.sourceTextures2[i].Reset();
         cfg.sourceTextures3[i].Reset();
      }

      static void MatchArrayLength(TextureArrayConfig cfg)
      {
         int srcCount = cfg.sourceTextures.Count;
         bool change = false;
         while (cfg.sourceTextures2.Count < srcCount)
         {
            var entry = new TextureArrayConfig.TextureEntry();
            entry.aoChannel = cfg.sourceTextures[0].aoChannel;
            entry.heightChannel = cfg.sourceTextures[0].heightChannel;
            entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
            cfg.sourceTextures2.Add(entry);
            change = true;
         }

         while (cfg.sourceTextures3.Count < srcCount)
         {
            var entry = new TextureArrayConfig.TextureEntry();
            entry.aoChannel = cfg.sourceTextures[0].aoChannel;
            entry.heightChannel = cfg.sourceTextures[0].heightChannel;
            entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
            cfg.sourceTextures3.Add(entry);
            change = true;
         }

         while (cfg.sourceTextures2.Count > srcCount)
         {
            cfg.sourceTextures2.RemoveAt(cfg.sourceTextures2.Count - 1);
            change = true;
         }
         while (cfg.sourceTextures3.Count > srcCount)
         {
            cfg.sourceTextures3.RemoveAt(cfg.sourceTextures3.Count - 1);
            change = true;
         }
         if (change)
         {
            EditorUtility.SetDirty(cfg);
         }
      }

      void DrawOverrideGUI(TextureArrayConfig cfg)
      {
         var prop = serializedObject.FindProperty("platformOverrides");
         EditorGUILayout.PropertyField(prop, CPlatformOverrides, true);
      }

      public override void OnInspectorGUI()
      {
         var cfg = target as TextureArrayConfig;
         serializedObject.Update();
         MatchArrayLength(cfg);
         EditorGUI.BeginChangeCheck();
         cfg.textureMode = (TextureArrayConfig.TextureMode)EditorGUILayout.EnumPopup(CTextureMode, cfg.textureMode);
         cfg.packingMode = (TextureArrayConfig.PackingMode)EditorGUILayout.EnumPopup(CPackingMode, cfg.packingMode);
         cfg.sourceTextureSize = (TextureArrayConfig.SourceTextureSize)EditorGUILayout.EnumPopup(CSourceTextureSize, cfg.sourceTextureSize);


         if (cfg.IsAdvancedDetail())
         {
            cfg.clusterMode = TextureArrayConfig.ClusterMode.None;
         }
         else
         {
            #if __MICROSPLAT_DETAILRESAMPLE__
            cfg.antiTileArray = EditorGUILayout.Toggle(CAntiTileArray, cfg.antiTileArray);
            #endif

            if (cfg.textureMode == TextureArrayConfig.TextureMode.PBR)
            {
               cfg.emisMetalArray = EditorGUILayout.Toggle(CEmisMetalArray, cfg.emisMetalArray);
            }

            #if __MICROSPLAT_TEXTURECLUSTERS__
            if (!cfg.IsAdvancedDetail())
            {
               cfg.clusterMode = (TextureArrayConfig.ClusterMode)EditorGUILayout.EnumPopup(CClusterMode, cfg.clusterMode);
            }
            #endif
         }

         var root = serializedObject.FindProperty("defaultTextureSettings");

         EditorGUILayout.PropertyField(root);
         if (root.isExpanded)
         {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(root.FindPropertyRelative("diffuseSettings"), true);
            EditorGUILayout.PropertyField(root.FindPropertyRelative("normalSettings"), true);

            if (cfg.textureMode != TextureArrayConfig.TextureMode.Basic)
            {
               if (cfg.packingMode == TextureArrayConfig.PackingMode.Quality)
               {
                  EditorGUILayout.PropertyField(root.FindPropertyRelative("smoothSettings"), true);
               }
               if (cfg.antiTileArray)
               {
                  EditorGUILayout.PropertyField(root.FindPropertyRelative("antiTileSettings"), true);
               }
               if (cfg.emisMetalArray)
               {
                  EditorGUILayout.PropertyField(root.FindPropertyRelative("emissiveSettings"), true);
               }
            }
            else 
            {
               EditorGUILayout.HelpBox("Select PBR mode to provide custom height, smoothness, and ao textures to greatly increase quality!", MessageType.Info);
            }

            EditorGUI.indentLevel--;
         }

         DrawOverrideGUI(cfg);


         if (MicroSplatUtilities.DrawRollup("Textures", true))
         {
            EditorGUILayout.HelpBox("Don't have a normal map? Any missing textures will be generated automatically from the best available source texture", MessageType.Info);
            bool disableClusters = cfg.IsAdvancedDetail();
            DrawHeader(cfg);
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {
               using (new GUILayout.VerticalScope(GUI.skin.box))
               {
                  bool remove = (DrawTextureEntry(cfg, cfg.sourceTextures[i], i));


                  if (cfg.clusterMode != TextureArrayConfig.ClusterMode.None && !disableClusters)
                  {
                     DrawTextureEntry(cfg, cfg.sourceTextures2[i], i, false);
                  }
                  if (cfg.clusterMode == TextureArrayConfig.ClusterMode.ThreeVariations && !disableClusters)
                  {
                     DrawTextureEntry(cfg, cfg.sourceTextures3[i], i, false);
                  }
                  if (remove)
                  {
                     var e = cfg.sourceTextures[i];
                     if (!e.HasTextures())
                     {
                        Remove(cfg, i);
                        i--;
                     }
                     else
                     {
                        Reset(cfg, i);
                     }
                  }

                  if (cfg.antiTileArray)
                  {
                     DrawAntiTileEntry(cfg, cfg.sourceTextures[i], i);
                  }

                  GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));
               }
            }
            if (GUILayout.Button("Add Textures"))
            {
               if (cfg.sourceTextures.Count > 0)
               {
                  var entry = new TextureArrayConfig.TextureEntry();
                  entry.aoChannel = cfg.sourceTextures[0].aoChannel;
                  entry.heightChannel = cfg.sourceTextures[0].heightChannel;
                  entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;
                  entry.alphaChannel = cfg.sourceTextures[0].alphaChannel;
                  cfg.sourceTextures.Add(entry);
               }
               else
               {
                  var entry = new TextureArrayConfig.TextureEntry();
                  entry.aoChannel = TextureArrayConfig.TextureChannel.G;
                  entry.heightChannel = TextureArrayConfig.TextureChannel.G;
                  entry.smoothnessChannel = TextureArrayConfig.TextureChannel.G;
                  entry.alphaChannel = TextureArrayConfig.TextureChannel.G;
                  cfg.sourceTextures.Add(entry);
               }
            }
         }
         if (GUILayout.Button("Grab From Scene Terrain"))
         {
            cfg.sourceTextures.Clear();
            GetFromTerrain(cfg);
         }
         if (GUILayout.Button("Update"))
         {
            staticConfig = cfg;
            EditorApplication.delayCall += DelayedCompileConfig;
         }
         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(cfg);
         }
         serializedObject.ApplyModifiedProperties();
      }

      static bool IsLinear(TextureImporter ti)
      {
         return ti.sRGBTexture == false;
      }

      static Texture2D ResizeTexture(Texture2D source, int width, int height, bool linear)
      {
         RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
         rt.DiscardContents();
         GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear) && !linear;
         Graphics.Blit(source, rt);
         GL.sRGBWrite = false;
         RenderTexture.active = rt;
         Texture2D ret = new Texture2D(width, height, TextureFormat.ARGB32, true, linear);
         ret.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
         ret.Apply(true);
         RenderTexture.active = null;
         rt.Release();
         DestroyImmediate(rt);
         return ret;
      }

      public static TextureFormat GetTextureFormat(TextureArrayConfig cfg, TextureArrayConfig.Compression cmp)
      {
         if (cmp == TextureArrayConfig.Compression.ForceETC2)
         {
            return TextureFormat.ETC2_RGBA8;
         }
         else if (cmp == TextureArrayConfig.Compression.ForcePVR)
         {
            return TextureFormat.PVRTC_RGBA4;
         }
         else if (cmp == TextureArrayConfig.Compression.ForceASTC)
         {
            return TextureFormat.ASTC_RGBA_4x4;
         }
         else if (cmp == TextureArrayConfig.Compression.ForceDXT)
         {
            return TextureFormat.DXT5;
         }
         else if (cmp == TextureArrayConfig.Compression.ForceCrunch)
         {
            return TextureFormat.DXT5Crunched;
         }
         else if (cmp == TextureArrayConfig.Compression.Uncompressed)
         {
            return TextureFormat.ARGB32;
         }
         var platform = EditorUserBuildSettings.activeBuildTarget;
         if (platform == BuildTarget.Android)
         {
            return TextureFormat.ETC2_RGBA8;
         }
         else if (platform == BuildTarget.iOS)
         {
            return TextureFormat.PVRTC_RGBA4;
         }
         else
         {
            return TextureFormat.DXT5;
         }
      }

      static Texture2D RenderMissingTexture(Texture2D src, string shaderPath, int width, int height, int channel = -1)
      {
         Texture2D res = new Texture2D(width, height, TextureFormat.ARGB32, true, true);
         RenderTexture resRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         resRT.DiscardContents();
         Shader s = Shader.Find(shaderPath);
         if (s == null)
         {
            Debug.LogError("Could not find shader " + shaderPath);
            res.Apply();
            return res;
         }
         Material genMat = new Material(Shader.Find(shaderPath));
         if (channel >= 0)
         {
            genMat.SetInt("_Channel", channel);
         }

         GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
         Graphics.Blit(src, resRT, genMat);
         GL.sRGBWrite = false;

         RenderTexture.active = resRT;
         res.ReadPixels(new Rect(0, 0, width, height), 0, 0);
         res.Apply();
         RenderTexture.active = null;
         resRT.Release();
         DestroyImmediate(resRT);
         DestroyImmediate(genMat);
         return res;
      }

      static void MergeInChannel(Texture2D target, int targetChannel, 
         Texture2D merge, int mergeChannel, bool linear, bool invert = false)
      {
         Texture2D src = ResizeTexture(merge, target.width, target.height, linear);
         Color[] sc = src.GetPixels();
         Color[] tc = target.GetPixels();

         for (int i = 0; i < tc.Length; ++i)
         {
            Color s = sc[i];
            Color t = tc[i];
            t[targetChannel] = s[mergeChannel];
            tc[i] = t;
         }
         if (invert)
         {
            for (int i = 0; i < tc.Length; ++i)
            {
               Color t = tc[i];
               t[targetChannel] = 1.0f - t[targetChannel];
               tc[i] = t;
            }
         }

         target.SetPixels(tc);
         target.Apply();
         DestroyImmediate(src);
      }


      static Texture2D NormalizeAlphaMask(Texture2D src, int targetChannel)
      {
         Texture2D result = RenderMissingTexture(src, "Unlit/Texture", src.width, src.height);
         Color[] pixels = result.GetPixels();

         float min = 1f;
         float max = 0f;
         float offset = 1f / 255f;

         foreach (Color c in pixels)
         {
            float v = c[targetChannel];
            if (v > 0)
            {
               if (v < min) min = v;
               if (v > max) max = v;
            }
         }

         min -= offset;
         float diff = max - min;

         if (diff > 0)
         {
            for (int i = 0; i < pixels.Length; i++)
            {
               Color c = pixels[i];
               float v = c[targetChannel];

               if (v > 0)
               {
                  v -= min;
                  v /= diff;
               }
               c[targetChannel] = v;
               pixels[i] = new Color(v, v, v, v);
            }

            result.SetPixels(pixels);
            result.Apply();
         }
         return result;
      }


#if !UNITY_2017_3_OR_NEWER

      static Texture2D BakeSubstance(string path, ProceduralTexture pt, bool linear = true, bool isNormal = false, bool invert = false)
      {
         string texPath = path + pt.name + ".tga";
         TextureImporter ti = TextureImporter.GetAtPath(texPath) as TextureImporter;
         if (ti != null)
         {
            bool changed = false;
            if (ti.sRGBTexture == true && linear)
            {
               ti.sRGBTexture = false;
               changed = true;
            }
            else if (ti.sRGBTexture == false && !linear)
            {
               ti.sRGBTexture = true;
               changed = true;
            }
            if (isNormal && ti.textureType != TextureImporterType.NormalMap)
            {
               ti.textureType = TextureImporterType.NormalMap;
               changed = true;
            }
            if (changed)
            {
               ti.SaveAndReimport();
            }
         }
         var srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
         return srcTex;
      }


      static void PreprocessTextureEntries(List<TextureArrayConfig.TextureEntry> src, TextureArrayConfig cfg, bool diffuseIsLinear)
      {
         for (int i = 0; i < src.Count; ++i)
         {
            var e = src[i];
            // fill out substance data if it exists
            if (e.substance != null)
            {
               e.substance.isReadable = true;
               e.substance.RebuildTexturesImmediately();
               string srcPath = AssetDatabase.GetAssetPath(e.substance);

               e.substance.SetProceduralVector("$outputsize", new Vector4(11, 11, 0, 0)); // in mip map space, so 2048

               SubstanceImporter si = AssetImporter.GetAtPath(srcPath) as SubstanceImporter;

               si.SetMaterialScale(e.substance, new Vector2(2048, 2048));
               string path = AssetDatabase.GetAssetPath(cfg);
               path = path.Replace("\\", "/");
               path = path.Substring(0, path.LastIndexOf("/"));
               path += "/SubstanceExports/";
               System.IO.Directory.CreateDirectory(path);
               si.ExportBitmaps(e.substance, path, true);
               AssetDatabase.Refresh();

               Texture[] textures = e.substance.GetGeneratedTextures();
               for (int tidx = 0; tidx < textures.Length; tidx++)
               {
                  ProceduralTexture pt = e.substance.GetGeneratedTexture(textures[tidx].name);

                  if (pt.GetProceduralOutputType() == ProceduralOutputType.Diffuse)
                  {
                     e.diffuse = BakeSubstance(path, pt, diffuseIsLinear);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Height)
                  {
                     e.height = BakeSubstance(path, pt);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.AmbientOcclusion)
                  {
                     e.ao = BakeSubstance(path, pt);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Normal)
                  {
                     e.normal = BakeSubstance(path, pt, true, true);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Smoothness)
                  {
                     e.smoothness = BakeSubstance(path, pt);
                     e.isRoughness = false;
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Roughness)
                  {
                     e.smoothness = BakeSubstance(path, pt, true, false);
                     e.isRoughness = true;
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Emissive && cfg.emisMetalArray)
                  {
                     e.emis = BakeSubstance(path, pt, true, false);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Metallic && cfg.emisMetalArray)
                  {
                     e.metal = BakeSubstance(path, pt);
                  }
               }
            }

         }
      }

      static void PreprocessTextureEntries(TextureArrayConfig cfg)
      {
         bool diffuseIsLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

         PreprocessTextureEntries(cfg.sourceTextures, cfg, diffuseIsLinear);
         if (cfg.clusterMode != TextureArrayConfig.ClusterMode.None && !cfg.IsAdvancedDetail())
         {
            PreprocessTextureEntries(cfg.sourceTextures2, cfg, diffuseIsLinear);
         }
         if (cfg.clusterMode == TextureArrayConfig.ClusterMode.ThreeVariations && !cfg.IsAdvancedDetail())
         {
            PreprocessTextureEntries(cfg.sourceTextures3, cfg, diffuseIsLinear);
         }

      }
#endif

#if SUBSTANCE_PLUGIN_ENABLED

      static Texture2D BakeSubstance(string path, Substance.Game.Substance pt, bool linear = true, bool isNormal = false, bool invert = false)
      {
         string texPath = path + pt.name + ".tga";
         TextureImporter ti = TextureImporter.GetAtPath(texPath) as TextureImporter;
         if (ti != null)
         {
            bool changed = false;
            if (ti.sRGBTexture == true && linear)
            {
               ti.sRGBTexture = false;
               changed = true;
            }
            else if (ti.sRGBTexture == false && !linear)
            {
               ti.sRGBTexture = true;
               changed = true;
            }
            if (isNormal && ti.textureType != TextureImporterType.NormalMap)
            {
               ti.textureType = TextureImporterType.NormalMap;
               changed = true;
            }
            if (changed)
            {
               ti.SaveAndReimport();
            }
         }
         var srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
         return srcTex;
      }


      static void PreprocessTextureEntries(List<TextureArrayConfig.TextureEntry> src, TextureArrayConfig cfg, bool diffuseIsLinear)
      {
         for (int i = 0; i < src.Count; ++i)
         {
            var e = src[i];
            // fill out substance data if it exists
            if (e.substance != null)
            {
            
               string srcPath = AssetDatabase.GetAssetPath(e.substance);


               foreach (var g in e.substance.graphs)
               {
                  e.substance.graphs[0].SetTexturesResolution(new Vector2Int(2048, 2048));
                  e.substance.graphs[0].QueueForRender();
                  e.substance.graphs[0].RenderSync();
                  var texes = g.GetGeneratedTextures();
                  foreach (var t in texes)
                  {
                     if (t.name.Contains("- baseColor"))
                     {
                        e.diffuse = t;
                     }
                     if (t.name.Contains("- height"))
                     {
                        e.height = t;
                        e.heightChannel = TextureArrayConfig.TextureChannel.G;
                     }
                     if (t.name.Contains("- ambientOcclusion"))
                     {
                        e.ao = t;
                        e.aoChannel = TextureArrayConfig.TextureChannel.G;
                     }
                     if (t.name.Contains("- metallic"))
                     {
                        e.metal = t;
                        e.metalChannel = TextureArrayConfig.TextureChannel.G;
                        e.smoothness = t;
                        e.smoothnessChannel = TextureArrayConfig.TextureChannel.A;
                     }
                     if (t.name.Contains("- normal"))
                     {
                        e.normal = t;
                     }
                     if (t.name.Contains("- emissive"))
                     {
                        e.emis = t;
                     }
                  }
               }
            }

         }
      }

      static void PreprocessTextureEntries(TextureArrayConfig cfg)
      {
         bool diffuseIsLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

         PreprocessTextureEntries(cfg.sourceTextures, cfg, diffuseIsLinear);
         if (cfg.clusterMode != TextureArrayConfig.ClusterMode.None && !cfg.IsAdvancedDetail())
         {
            PreprocessTextureEntries(cfg.sourceTextures2, cfg, diffuseIsLinear);
         }
         if (cfg.clusterMode == TextureArrayConfig.ClusterMode.ThreeVariations && !cfg.IsAdvancedDetail())
         {
            PreprocessTextureEntries(cfg.sourceTextures3, cfg, diffuseIsLinear);
         }

      }
#endif

      static TextureArrayConfig staticConfig;
      void DelayedCompileConfig()
      {
         CompileConfig(staticConfig);
      }

      static string GetDiffPath(TextureArrayConfig cfg, string ext)
      {
         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         return path.Replace(".asset", "_diff" + ext + "_tarray.asset");
      }

      static string GetNormPath(TextureArrayConfig cfg, string ext)
      {
         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         if (cfg.packingMode == TextureArrayConfig.PackingMode.Fastest)
         {
            return path.Replace(".asset", "_normSAO" + ext + "_tarray.asset");
         }
         else
         {
            return path.Replace(".asset", "_normal" + ext + "_tarray.asset");
         }
      }

      static string GetSmoothAOPath(TextureArrayConfig cfg, string ext)
      {
         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         return path.Replace(".asset", "_smoothAO" + ext + "_tarray.asset");
      }

      static string GetAntiTilePath(TextureArrayConfig cfg, string ext)
      {
         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         return path.Replace(".asset", "_antiTile" + ext + "_tarray.asset");
      }

      static string GetEmisPath(TextureArrayConfig cfg, string ext)
      {
         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         return path.Replace(".asset", "_emis" + ext + "_tarray.asset");
      }

      static int SizeToMipCount(int size)
      {
         int mips = 11;
         if (size == 4096)
            mips = 13;
         else if (size == 2048)
            mips = 12;
         else if (size == 1024)
            mips = 11;
         else if (size == 512)
            mips = 10;
         else if (size == 256)
            mips = 9;
         else if (size == 128)
            mips = 8;
         else if (size == 64)
            mips = 7;
         else if (size == 32)
            mips = 6;
         return mips;
      }

      static void ShrinkSourceTexture(Texture2D tex, TextureArrayConfig.SourceTextureSize stz)
      {
         if (tex != null)
         {
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            TextureImporter ti = (TextureImporter)ai;
            if (ti != null && ti.maxTextureSize != (int)stz)
            {
               ti.maxTextureSize = (int)stz;
               ti.SaveAndReimport();
            }
         }
      }

      static void ShrinkSourceTextures(List<TextureArrayConfig.TextureEntry> textures, TextureArrayConfig.SourceTextureSize stz)
      {
         if (textures == null)
            return;
         if (stz == TextureArrayConfig.SourceTextureSize.Unchanged)
            return;
         foreach (var t in textures)
         {
            ShrinkSourceTexture(t.ao, stz);
            ShrinkSourceTexture(t.alpha, stz);
            ShrinkSourceTexture(t.diffuse, stz);
            ShrinkSourceTexture(t.distanceNoise, stz);
            ShrinkSourceTexture(t.normal, stz);
            ShrinkSourceTexture(t.noiseNormal, stz);
            ShrinkSourceTexture(t.detailNoise, stz);
            ShrinkSourceTexture(t.emis, stz);
            ShrinkSourceTexture(t.height, stz);
            ShrinkSourceTexture(t.metal, stz);
            ShrinkSourceTexture(t.smoothness, stz);
         }
      }
         

      static void RestoreSourceTexture(Texture2D tex)
      {
         if (tex != null)
         {
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            TextureImporter ti = ai as TextureImporter;
            if (ti != null && ti.maxTextureSize <= 256)
            {
               ti.maxTextureSize = 4096;
               ti.SaveAndReimport();
            }

         }
      }

      static void RestoreSourceTextures(List<TextureArrayConfig.TextureEntry> textures, TextureArrayConfig.SourceTextureSize stz)
      {
         if (textures == null)
            return;

         foreach (var t in textures)
         {
            RestoreSourceTexture(t.ao);
            RestoreSourceTexture(t.alpha);
            RestoreSourceTexture(t.diffuse);
            RestoreSourceTexture(t.distanceNoise);
            RestoreSourceTexture(t.normal);
            RestoreSourceTexture(t.noiseNormal);
            RestoreSourceTexture(t.detailNoise);
            RestoreSourceTexture(t.emis);
            RestoreSourceTexture(t.height);
            RestoreSourceTexture(t.metal);
            RestoreSourceTexture(t.smoothness);
         }
      }

      public static TextureArrayConfig.TextureArrayGroup GetSettingsGroup(TextureArrayConfig cfg, BuildTarget t)
      { 
         foreach (var g in cfg.platformOverrides)
         {
            if (g.platform == t)
            {
               return g.settings;
            }
         }
         return cfg.defaultTextureSettings;
      }

      static void CompileConfig(TextureArrayConfig cfg, 
         List<TextureArrayConfig.TextureEntry> src,
         string ext, 
         bool isCluster = false)
      {

         RestoreSourceTextures(src, cfg.sourceTextureSize);

         bool diffuseIsLinear = false;

         var settings = GetSettingsGroup(cfg, UnityEditor.EditorUserBuildSettings.activeBuildTarget);


         int diffuseWidth =   (int)settings.diffuseSettings.textureSize;
         int diffuseHeight =  (int)settings.diffuseSettings.textureSize;
         int normalWidth =    (int)settings.normalSettings.textureSize;
         int normalHeight =   (int)settings.normalSettings.textureSize;
         int smoothWidth =    (int)settings.smoothSettings.textureSize;
         int smoothHeight =   (int)settings.smoothSettings.textureSize;
         int antiTileWidth =  (int)settings.antiTileSettings.textureSize;
         int antiTileHeight = (int)settings.antiTileSettings.textureSize;
         int emisWidth =      (int)settings.emissiveSettings.textureSize;
         int emisHeight =     (int)settings.emissiveSettings.textureSize;

         int diffuseAnisoLevel = settings.diffuseSettings.Aniso;
         int normalAnisoLevel = settings.normalSettings.Aniso;
         int antiTileAnisoLevel = settings.antiTileSettings.Aniso;
         int emisAnisoLevel = settings.emissiveSettings.Aniso;

         FilterMode diffuseFilter = settings.diffuseSettings.filterMode;
         FilterMode normalFilter = settings.normalSettings.filterMode;
         FilterMode antiTileFilter = settings.antiTileSettings.filterMode;
         FilterMode emisFilter = settings.emissiveSettings.filterMode;

         int diffuseMipCount = SizeToMipCount(diffuseWidth);
         int normalMipCount = SizeToMipCount(normalWidth);
         int smoothAOMipCount = SizeToMipCount((int)settings.smoothSettings.textureSize);
         int antiTileMipCount = SizeToMipCount(antiTileWidth);
         int emisMipCount = SizeToMipCount(emisWidth);

         int texCount = src.Count;
         if (texCount < 1)
            texCount = 1;

         // diffuse
         Texture2DArray diffuseArray = new Texture2DArray(diffuseWidth, diffuseHeight, texCount,
          GetTextureFormat(cfg, settings.diffuseSettings.compression),
            true, diffuseIsLinear);

         diffuseArray.wrapMode = TextureWrapMode.Repeat;
         diffuseArray.filterMode = diffuseFilter;
         diffuseArray.anisoLevel = diffuseAnisoLevel;


         // normal
         var nmlcomp = GetTextureFormat(cfg, settings.normalSettings.compression);
         Texture2DArray normalSAOArray = new Texture2DArray(normalWidth, normalHeight, texCount, nmlcomp, true, true);

         normalSAOArray.wrapMode = TextureWrapMode.Repeat;
         normalSAOArray.filterMode = normalFilter;
         normalSAOArray.anisoLevel = normalAnisoLevel;

         Texture2DArray smoothAOArray = null;
         if (cfg.packingMode == TextureArrayConfig.PackingMode.Quality)
         {
            smoothAOArray = new Texture2DArray((int)settings.smoothSettings.textureSize, (int)settings.smoothSettings.textureSize, texCount,
               GetTextureFormat(cfg, settings.smoothSettings.compression),
               true, true);
         }

         // antitile
         Texture2DArray antiTileArray = null;
         if (!isCluster && cfg.antiTileArray)
         {
            antiTileArray = new Texture2DArray(antiTileWidth, antiTileHeight, texCount,
               GetTextureFormat(cfg, settings.antiTileSettings.compression),
               true, true);

            antiTileArray.wrapMode = TextureWrapMode.Repeat;
            antiTileArray.filterMode = antiTileFilter;
            antiTileArray.anisoLevel = antiTileAnisoLevel;
         }

         // emis/metal
         Texture2DArray emisArray = null;
         if (cfg.emisMetalArray && !cfg.IsAdvancedDetail())
         {
            emisArray = new Texture2DArray(emisWidth, emisHeight, texCount,
               GetTextureFormat(cfg, settings.emissiveSettings.compression),
               true, diffuseIsLinear);

            emisArray.wrapMode = TextureWrapMode.Repeat;
            emisArray.filterMode = emisFilter;
            emisArray.anisoLevel = emisAnisoLevel;
         }

         for (int i = 0; i < src.Count; ++i)
         {
            try
            {
               EditorUtility.DisplayProgressBar("Packing textures...", "", (float)i / (float)src.Count);

               // first, generate any missing data. We generate a full NSAO map from diffuse or height map
               // if no height map is provided, we then generate it from the resulting or supplied normal. 
               var e = src[i];
               Texture2D diffuse = e.diffuse;
               if (diffuse == null)
               {
                  diffuse = Texture2D.whiteTexture;
               }

               // resulting maps
               Texture2D diffuseHeightTex = ResizeTexture(diffuse, diffuseWidth, diffuseHeight, diffuseIsLinear);
               Texture2D normalSAOTex = null;
               Texture2D smoothAOTex = null;
               Texture2D antiTileTex = null;
               Texture2D emisTex = null;

               int heightChannel = (int)e.heightChannel;
               int aoChannel = (int)e.aoChannel;
               int smoothChannel = (int)e.smoothnessChannel;
               int alphaChannel = (int)e.alphaChannel;
               int detailChannel = (int)e.detailChannel;
               int distanceChannel = (int)e.distanceChannel;
               int metalChannel = (int)e.metalChannel;

               if (cfg.allTextureChannelHeight != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  heightChannel = (int)cfg.allTextureChannelHeight;
               }
               if (cfg.allTextureChannelAO != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  aoChannel = (int)cfg.allTextureChannelAO;
               }
               if (cfg.allTextureChannelSmoothness != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  smoothChannel = (int)cfg.allTextureChannelSmoothness;
               }
               if (cfg.allTextureChannelAlpha != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  alphaChannel = (int)cfg.allTextureChannelAlpha;
               }

               if (e.normal == null)
               {
                  if (e.height == null)
                  {
                     normalSAOTex = RenderMissingTexture(diffuse, "Hidden/MicroSplat/NormalSAOFromDiffuse", normalWidth, normalHeight);
                  }
                  else
                  {
                     normalSAOTex = RenderMissingTexture(e.height, "Hidden/MicroSplat/NormalSAOFromHeight", normalWidth, normalHeight, heightChannel);
                  }
               }
               else
               {
                  // copy, but go ahead and generate other channels in case they aren't provided later.
                  normalSAOTex = RenderMissingTexture(e.normal, "Hidden/MicroSplat/NormalSAOFromNormal", normalWidth, normalHeight);
               }

               if (!isCluster && cfg.antiTileArray)
               {
                  antiTileTex = RenderMissingTexture(e.noiseNormal, "Hidden/MicroSplat/NormalSAOFromNormal", antiTileWidth, antiTileHeight);
               }

               bool destroyHeight = false;
               Texture2D height = e.height;
               if (height == null)
               {
                  destroyHeight = true;
                  height = RenderMissingTexture(normalSAOTex, "Hidden/MicroSplat/HeightFromNormal", diffuseHeight, diffuseWidth);
               }

               MergeInChannel(diffuseHeightTex, (int)TextureArrayConfig.TextureChannel.A, height, heightChannel, diffuseIsLinear);


               if (cfg.emisMetalArray && !cfg.IsAdvancedDetail())
               {
                  emisTex = ResizeTexture(e.emis != null ? e.emis : Texture2D.blackTexture, emisWidth, emisHeight, diffuseIsLinear);
                  Texture2D metal = ResizeTexture(e.metal != null ? e.metal : Texture2D.blackTexture, emisWidth, emisHeight, true);
                  MergeInChannel(emisTex, 3, metal, e.metal != null ? metalChannel : 0, true, false);
                  DestroyImmediate(metal);
               }

               if (cfg.IsAdvancedDetail())
               {
                  if (e.alpha != null)
                  {
                     var alpha = e.normalizeAlpha? NormalizeAlphaMask(e.alpha, (int)TextureArrayConfig.TextureChannel.B) : e.alpha;
                     MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.B, alpha, alphaChannel, true);
                  }
                  else
                  {
                     MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.B, Texture2D.whiteTexture, 0, true);
                  }
               }
               else if (e.ao != null)
               {
                  MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.B, e.ao, aoChannel, true);
               }

               if (e.smoothness != null)
               {
                  MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.R, e.smoothness, smoothChannel, true, e.isRoughness);
               }

               if (cfg.packingMode == TextureArrayConfig.PackingMode.Quality)
               {
                  // clear non-normal data to help compression quality
                  Color[] cols = normalSAOTex.GetPixels();
                  for (int x = 0; x < cols.Length; ++x)
                  {
                     Color c = cols[x];
                     c.r = 0;
                     c.b = 0;
                     cols[x] = c;
                  }
                  normalSAOTex.SetPixels(cols);


                  // generate missing maps for smoothness
                  if (e.normal == null)
                  {
                     if (e.height == null)
                     {
                        smoothAOTex = RenderMissingTexture(diffuse, "Hidden/MicroSplat/NormalSAOFromDiffuse", smoothWidth, smoothHeight);
                     }
                     else
                     {
                        smoothAOTex = RenderMissingTexture(e.height, "Hidden/MicroSplat/NormalSAOFromHeight", smoothWidth, smoothHeight, heightChannel);
                     }
                  }
                  else
                  {
                     // copy, but go ahead and generate other channels in case they aren't provided later.
                     smoothAOTex = RenderMissingTexture(e.normal, "Hidden/MicroSplat/NormalSAOFromNormal", smoothWidth, smoothHeight);
                  }

                  // now clear normal data, and swizzle channels into G/A

                  // clear non-normal data to help compression quality
                  cols = smoothAOTex.GetPixels();
                  for (int x = 0; x < cols.Length; ++x)
                  {
                     Color c = cols[x];
                     c.g = c.r;
                     c.a = c.b;
                     c.r = 0;
                     c.b = 0;
                     cols[x] = c;
                  }
                  smoothAOTex.SetPixels(cols);

                  // merge in data if provided
                  if (e.ao != null)
                  {
                     MergeInChannel(smoothAOTex, (int)TextureArrayConfig.TextureChannel.A, e.ao, aoChannel, true);
                  }

                  if (e.smoothness != null)
                  {
                     MergeInChannel(smoothAOTex, (int)TextureArrayConfig.TextureChannel.G, e.smoothness, smoothChannel, true, e.isRoughness);
                  }

               }


               if (!isCluster && cfg.antiTileArray && antiTileTex != null)
               {
                  Texture2D detail = e.detailNoise;
                  Texture2D distance = e.distanceNoise;
                  bool destroyDetail = false;
                  bool destroyDistance = false;
                  if (detail == null)
                  {
                     detail = new Texture2D(1, 1, TextureFormat.RGB24, true, true);
                     detail.SetPixel(0, 0, Color.grey);
                     detail.Apply();
                     destroyDetail = true;
                     detailChannel = (int)TextureArrayConfig.TextureChannel.G;
                  }
                  if (distance == null)
                  {
                     distance = new Texture2D(1, 1, TextureFormat.RGB24, true, true);
                     distance.SetPixel(0, 0, Color.grey);
                     distance.Apply();
                     destroyDistance = true;
                     distanceChannel = (int)TextureArrayConfig.TextureChannel.G;
                  }
                  MergeInChannel(antiTileTex, (int)TextureArrayConfig.TextureChannel.R, detail, detailChannel, true, false);
                  MergeInChannel(antiTileTex, (int)TextureArrayConfig.TextureChannel.B, distance, distanceChannel, true, false);

                  if (destroyDetail)
                  {
                     GameObject.DestroyImmediate(detail);
                  }
                  if (destroyDistance)
                  {
                     GameObject.DestroyImmediate(distance);
                  }
               }

#if UNITY_2018_3_OR_NEWER
               int tq = (int)UnityEditor.TextureCompressionQuality.Normal;
#else
               var tq = UnityEngine.TextureCompressionQuality.Normal;
#endif

               if (settings.normalSettings.compression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(normalSAOTex, GetTextureFormat(cfg, settings.normalSettings.compression), tq);
               }

               if (settings.diffuseSettings.compression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(diffuseHeightTex, GetTextureFormat(cfg, settings.diffuseSettings.compression), tq);
               }

               if (smoothAOTex != null && cfg.packingMode == TextureArrayConfig.PackingMode.Quality)
               {
                  EditorUtility.CompressTexture(smoothAOTex, GetTextureFormat(cfg, settings.smoothSettings.compression), tq);
               }

               if (antiTileTex != null && settings.antiTileSettings.compression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(antiTileTex, GetTextureFormat(cfg, settings.antiTileSettings.compression), tq);
               }

               if (cfg.emisMetalArray && !cfg.IsAdvancedDetail() && emisTex != null && settings.emissiveSettings.compression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(emisTex, GetTextureFormat(cfg, settings.emissiveSettings.compression), tq);
               }

               normalSAOTex.Apply();
               diffuseHeightTex.Apply();

               if (smoothAOTex != null)
               {
                  smoothAOTex.Apply();
               }

               if (antiTileTex != null)
               {
                  antiTileTex.Apply();
               }

               if (emisTex != null)
               {
                  emisTex.Apply();
               }

               for (int mip = 0; mip < diffuseMipCount; ++mip)
               {
                  Graphics.CopyTexture(diffuseHeightTex, 0, mip, diffuseArray, i, mip);
               }
               for (int mip = 0; mip < normalMipCount; ++mip)
               {
                  Graphics.CopyTexture(normalSAOTex, 0, mip, normalSAOArray, i, mip);
               }
               if (smoothAOArray != null)
               {
                  for (int mip = 0; mip < smoothAOMipCount; ++mip)
                  {
                     Graphics.CopyTexture(smoothAOTex, 0, mip, smoothAOArray, i, mip);
                  }
               }
               if (antiTileTex != null)
               {
                  for (int mip = 0; mip < antiTileMipCount; ++mip)
                  {
                     Graphics.CopyTexture(antiTileTex, 0, mip, antiTileArray, i, mip);
                  }
               }
               if (emisTex != null)
               {
                  for (int mip = 0; mip < emisMipCount; ++mip)
                  {
                     Graphics.CopyTexture(emisTex, 0, mip, emisArray, i, mip);
                  }
               }
               DestroyImmediate(diffuseHeightTex);
               DestroyImmediate(normalSAOTex);

               if (smoothAOTex != null)
               {
                  DestroyImmediate(smoothAOTex);
               }
               if (antiTileTex != null)
               {
                  DestroyImmediate(antiTileTex);
               }
               if (emisTex)
               {
                  DestroyImmediate(emisTex);
               }

               if (destroyHeight)
               {
                  DestroyImmediate(height);
               }


            }
            finally
            {
               EditorUtility.ClearProgressBar();
            }

         }
         EditorUtility.ClearProgressBar();

         diffuseArray.Apply(false, true);
         normalSAOArray.Apply(false, true);
         if (antiTileArray != null)
         {
            antiTileArray.Apply(false, true);
         }
         if (emisArray != null)
         {
            emisArray.Apply(false, true);
         }
         if (smoothAOArray != null)
         {
            smoothAOArray.Apply(false, true);
         }

         string diffPath = GetDiffPath(cfg, ext);
         string normSAOPath = GetNormPath(cfg, ext);
         string antiTilePath = GetAntiTilePath(cfg, ext);
         string emisPath = GetEmisPath(cfg, ext);
         string smoothAOPath = GetSmoothAOPath(cfg, ext);

         {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(diffPath);
            if (existing != null)
            {
               EditorUtility.CopySerialized(diffuseArray, existing);
            }
            else
            {
               AssetDatabase.CreateAsset(diffuseArray, diffPath);
            }
         }

         {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(normSAOPath);
            if (existing != null)
            {
               EditorUtility.CopySerialized(normalSAOArray, existing);
            }
            else
            {
               AssetDatabase.CreateAsset(normalSAOArray, normSAOPath);
            }
         }

         if (cfg.packingMode != TextureArrayConfig.PackingMode.Fastest && smoothAOArray != null)
         {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(smoothAOPath);
            if (existing != null)
            {
               EditorUtility.CopySerialized(smoothAOArray, existing);
            }
            else
            {
               AssetDatabase.CreateAsset(smoothAOArray, smoothAOPath);
            }
         }

         if (cfg.antiTileArray && antiTileArray != null)
         {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(antiTilePath);
            if (existing != null)
            {
               EditorUtility.CopySerialized(antiTileArray, existing);
            }
            else
            {
               AssetDatabase.CreateAsset(antiTileArray, antiTilePath);
            }
         }

         if (cfg.emisMetalArray && emisArray != null)
         {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(emisPath);
            if (existing != null)
            {
               EditorUtility.CopySerialized(emisArray, existing);
            }
            else
            {
               AssetDatabase.CreateAsset(emisArray, emisPath);
            }
         }

         EditorUtility.SetDirty(cfg);
         AssetDatabase.Refresh();
         AssetDatabase.SaveAssets();

         MicroSplatUtilities.ClearPreviewCache();
         MicroSplatTerrain.SyncAll();

         ShrinkSourceTextures(src, cfg.sourceTextureSize);

      }

      public static void CompileConfig(TextureArrayConfig cfg)
      {
         MatchArrayLength(cfg);
#if !UNITY_2017_3_OR_NEWER
         PreprocessTextureEntries(cfg);
#endif

#if SUBSTANCE_PLUGIN_ENABLED
         PreprocessTextureEntries(cfg);
#endif

         CompileConfig(cfg, cfg.sourceTextures, "", false);
         if (cfg.clusterMode != TextureArrayConfig.ClusterMode.None)
         {
            CompileConfig(cfg, cfg.sourceTextures2, "_C2", true);
         }
         if (cfg.clusterMode == TextureArrayConfig.ClusterMode.ThreeVariations)
         {
            CompileConfig(cfg, cfg.sourceTextures3, "_C3", true);
         }


         cfg.diffuseArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(GetDiffPath(cfg, ""));
         cfg.normalSAOArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(GetNormPath(cfg, ""));
         if (cfg.packingMode == TextureArrayConfig.PackingMode.Quality)
         {
            cfg.smoothAOArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(GetSmoothAOPath(cfg, ""));
         }

         EditorUtility.SetDirty(cfg);
         AssetDatabase.Refresh();
         AssetDatabase.SaveAssets();

         MicroSplatUtilities.ClearPreviewCache();
         MicroSplatTerrain.SyncAll();
      }

   }
}
