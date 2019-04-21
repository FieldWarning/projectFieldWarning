//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;


public partial class MicroSplatTerrainEditor : Editor
{

   void ImportExportGUI()
   {
      if (MicroSplatUtilities.DrawRollup("Splat Import/Export", false))
      {
         EditorGUI.BeginChangeCheck();
         serializedObject.Update();
         SerializedProperty prop = serializedObject.FindProperty("importSplatMaps");
         EditorGUILayout.PropertyField(prop, true);
         if (EditorGUI.EndChangeCheck())
         {
            serializedObject.ApplyModifiedProperties();
         }

         if (GUILayout.Button("Import"))
         {
            ImportSplatMaps();
         }
         if (GUILayout.Button("Export"))
         {
            ExportSplatMaps();
         }
      }
      
   }


   void ImportSplatMaps()
   {
      MicroSplatTerrain mst = target as MicroSplatTerrain;
      var terrain = mst.terrain;
      if (terrain == null)
      {
         return;
      }
      var tdata = terrain.terrainData;
      if (tdata == null)
      {
         return;
      }
      List<Texture2D> importSplatMaps = mst.importSplatMaps;

      // sanatize data
      for (int i = 0; i < importSplatMaps.Count; ++i)
      {
         if (importSplatMaps[i] == null)
         {
            importSplatMaps.RemoveAt(i);
            i--;
         }
            
      }
      int mapCount = importSplatMaps.Count;
      if (mapCount > 8)
      {
         mapCount = 8;
      }
      if (mapCount == 0)
         return;

      int w = tdata.alphamapWidth;
      int h = tdata.alphamapHeight;


      RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      Texture2D buffer = new Texture2D(w, h, TextureFormat.ARGB32, false, true);

      float[,,] data = new float[w, h, tdata.alphamapLayers];
      for (int i = 0; i < mapCount; ++i)
      {
         try
         {
            EditorUtility.DisplayProgressBar("Importing Splat Maps", "Map : " + i, (float)i / mapCount);

            // scale texture to whatever size our alpha maps are set to
            Graphics.Blit(importSplatMaps[i], rt);
            RenderTexture.active = rt;
            buffer.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            buffer.Apply();
            for (int x = 0; x < w; ++x)
            {
               for (int y = 0; y < h; ++y)
               {
                  Color c = buffer.GetPixel(x, y);
                  data[x, y, i * 4] = c.r;
                  if (i*4+1 < tdata.alphamapLayers)
                     data[x, y, i * 4 + 1] = c.g;
                  if (i * 4 + 2 < tdata.alphamapLayers)
                     data[x, y, i * 4 + 2] = c.b;
                  if (i * 4 + 3 < tdata.alphamapLayers)
                     data[x, y, i * 4 + 3] = c.a;
               }
            }
         }
         catch 
         { 
            Debug.LogError("Error in importing terrain");
            EditorUtility.ClearProgressBar();
            RenderTexture.active = null;
            DestroyImmediate(rt);
            DestroyImmediate(buffer);
            return;
         }
         finally
         {
            RenderTexture.active = null;
            EditorUtility.ClearProgressBar();
         }
      }

      DestroyImmediate(rt);
      DestroyImmediate(buffer);
      tdata.SetAlphamaps(0, 0, data);
   }

   void ExportSplatMaps()
   {
      var path = EditorUtility.SaveFolderPanel("Save textures to directory", "", "");
      if (string.IsNullOrEmpty(path))
         return;

      path = path.Replace("\\", "/");
      if (!path.EndsWith("/"))
         path += "/";


      MicroSplatTerrain mst = target as MicroSplatTerrain;
      var terrain = mst.terrain;
      if (terrain == null)
      {
         return;
      }
      var tdata = terrain.terrainData;
      if (tdata == null)
      {
         return;
      }


      var data = tdata.GetAlphamaps(0, 0, tdata.alphamapWidth, tdata.alphamapHeight);
      int textureCount = tdata.alphamapLayers / 4 + 1;
      for (int i = 0; i < textureCount; ++i)
      {
         Texture2D tex = new Texture2D(tdata.alphamapWidth, tdata.alphamapHeight, TextureFormat.ARGB32, false, true);
         for (int x = 0; x < tdata.alphamapWidth; ++x)
         {
            for (int y = 0; y < tdata.alphamapHeight; ++y)
            {
               Color c;
               c.r = data[x, y, i * 4];
               c.g = tdata.alphamapLayers > i * 4 + 1 ? data[x, y, i * 4 + 1] : 0;
               c.b = tdata.alphamapLayers > i * 4 + 2 ? data[x, y, i * 4 + 2] : 0;
               c.a = tdata.alphamapLayers > i * 4 + 3 ? data[x, y, i * 4 + 3] : 0;
               tex.SetPixel(x, y, c);
            }
         }
         tex.Apply();
         var bytes = tex.EncodeToPNG();
         System.IO.File.WriteAllBytes(path + "SplatControl" + i + ".png", bytes);
         DestroyImmediate(tex);
      }
   }

}
