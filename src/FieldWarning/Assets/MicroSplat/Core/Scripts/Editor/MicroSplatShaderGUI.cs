//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MicroSplat;
using System.Linq;


public partial class MicroSplatShaderGUI : ShaderGUI
{
   public static readonly string MicroSplatVersion = "2.5";

   MicroSplatCompiler compiler = new MicroSplatCompiler();

   public MaterialProperty FindProp(string name, MaterialProperty[] props)
   {
      return FindProperty(name, props);
   }

   GUIContent CShaderName = new GUIContent("Name", "Menu path with name for the shader");
   #if UNITY_2018_1_OR_NEWER
   GUIContent CRenderLoop = new GUIContent("Render Loop", "In 2018.1+, Scriptable Render Loops are available. You can select which render loop the shader should be compiled for here");
   #endif

   bool needsCompile = false;
   int perTexIndex = 0;
   System.Text.StringBuilder builder = new System.Text.StringBuilder(1024);
   GUIContent[] renderLoopNames;

   bool DrawRenderLoopGUI(MicroSplatKeywords keywords, Material targetMat)
   {
#if UNITY_2018_1_OR_NEWER
      // init render loop name list
      if (renderLoopNames == null || renderLoopNames.Length != availableRenderLoops.Count)
      {
         var rln = new List<GUIContent>();
         for (int i = 0; i < availableRenderLoops.Count; ++i)
         {
            rln.Add(new GUIContent(availableRenderLoops[i].GetDisplayName()));
         }
         renderLoopNames = rln.ToArray();
      }

      if (renderLoopNames.Length == 1)
      {
         return false;
      }

      int curRenderLoopIndex = 0;
      for (int i = 0; i < keywords.keywords.Count; ++i)
      {
         string s = keywords.keywords[i];
         for (int j = 0; j < availableRenderLoops.Count; ++j)
         {
            if (s == availableRenderLoops[j].GetRenderLoopKeyword())
            {
               curRenderLoopIndex = j;
               compiler.renderLoop = availableRenderLoops[j];
               break;
            }
         }
      }

      int oldIdx = curRenderLoopIndex;
      curRenderLoopIndex = EditorGUILayout.Popup(CRenderLoop, curRenderLoopIndex, renderLoopNames);
      if (oldIdx != curRenderLoopIndex && curRenderLoopIndex >= 0 && curRenderLoopIndex < availableRenderLoops.Count)
      {
         if (compiler.renderLoop != null)
         {
            keywords.DisableKeyword(compiler.renderLoop.GetRenderLoopKeyword());
         }
         compiler.renderLoop = availableRenderLoops[curRenderLoopIndex];
         keywords.EnableKeyword(compiler.renderLoop.GetRenderLoopKeyword());
         return true;
      }
#endif

#if UNITY_2018_3_OR_NEWER
      if (targetMat != null && !targetMat.enableInstancing)
      {
         EditorUtility.SetDirty(targetMat);
         targetMat.enableInstancing = true;
      }
#endif
      return false;
   }

   string cachedTitle;
   public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
   {
      if (cachedTitle == null)
      {
         cachedTitle = "Shader Generator        v:" + MicroSplatVersion;
      }
      if (GUI.enabled == false)
      {
         EditorGUILayout.HelpBox("You must edit the template material, not the instance being used", MessageType.Info);
         return;
      }
      EditorGUI.BeginChangeCheck(); // sync materials
      Material targetMat = materialEditor.target as Material;
      Texture2DArray diff = targetMat.GetTexture("_Diffuse") as Texture2DArray;

      var keywordSO = MicroSplatUtilities.FindOrCreateKeywords(targetMat);

      compiler.Init();
      // must unpack everything before the generator draws- otherwise we get IMGUI errors
      for (int i = 0; i < compiler.extensions.Count; ++i)
      {
         var ext = compiler.extensions[i];
         ext.Unpack(keywordSO.keywords.ToArray());
      }
         
      string shaderName = targetMat.shader.name;
      DrawModules();

      EditorGUI.BeginChangeCheck(); // needs compile

      if (MicroSplatUtilities.DrawRollup(cachedTitle))
      {
         shaderName = EditorGUILayout.DelayedTextField(CShaderName, shaderName);

         if (DrawRenderLoopGUI(keywordSO, targetMat))
         {
            needsCompile = true;
         }

         for (int i = 0; i < compiler.extensions.Count; ++i)
         {
            var e = compiler.extensions[i];
            if (e.GetVersion() == MicroSplatVersion)
            {
               //using (new GUILayout.VerticalScope(GUI.skin.box))
               {
                  e.DrawFeatureGUI(keywordSO);
               }
            }
            else
            {
               EditorGUILayout.HelpBox("Extension : " + e + " is version " + e.GetVersion() + " and MicroSplat is version " + MicroSplatVersion + ", please update", MessageType.Error);
            }
         }

         for (int i = 0; i < availableRenderLoops.Count; ++i)
         {
            var rl = availableRenderLoops[i];
            if (rl.GetVersion() != MicroSplatVersion)
            {
               EditorGUILayout.HelpBox("Render Loop : " + rl.GetDisplayName() + " is version " + rl.GetVersion() + " and MicroSplat is version " + MicroSplatVersion + ", please update", MessageType.Error);
            }
         }
      }
      needsCompile = needsCompile || EditorGUI.EndChangeCheck();

      int featureCount = keywordSO.keywords.Count;
      // note, ideally we wouldn't draw the GUI for the rest of stuff if we need to compile.
      // But we can't really do that without causing IMGUI to split warnings about
      // mismatched GUILayout blocks
      if (!needsCompile)
      {
         for (int i = 0; i < compiler.extensions.Count; ++i)
         {
            var ext = compiler.extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
            {
               ext.DrawShaderGUI(this, keywordSO, targetMat, materialEditor, props);
            }
            else
            {
               EditorGUILayout.HelpBox("Extension : " + ext + " is version " + ext.GetVersion() + " and MicroSplat is version " + MicroSplatVersion + ", please update so that all modules are using the same version.", MessageType.Error);
            }

         }


         if (diff != null && MicroSplatUtilities.DrawRollup("Per Texture Properties"))
         {
            var propTex = FindOrCreatePropTex(targetMat);
            perTexIndex = MicroSplatUtilities.DrawTextureSelector(perTexIndex, diff);
            for (int i = 0; i < compiler.extensions.Count; ++i)
            {
               var ext = compiler.extensions[i];
               if (ext.GetVersion() == MicroSplatVersion)
               {
                  ext.DrawPerTextureGUI(perTexIndex, keywordSO, targetMat, propTex);
               }
            }
         }
      }

      if (!needsCompile)
      {
         if (featureCount != keywordSO.keywords.Count)
         {
            needsCompile = true;
         }
      }
         
         
      int arraySampleCount = 0;
      int textureSampleCount = 0;
      int maxSamples = 0;
      int tessSamples = 0;
      int depTexReadLevel = 0;
      builder.Length = 0;
      for (int i = 0; i < compiler.extensions.Count; ++i)
      {
         var ext = compiler.extensions[i];
         if (ext.GetVersion() == MicroSplatVersion)
         {
            ext.ComputeSampleCounts(keywordSO.keywords.ToArray(), ref arraySampleCount, ref textureSampleCount, ref maxSamples, ref tessSamples, ref depTexReadLevel);
         }
      }
      if (MicroSplatUtilities.DrawRollup("Debug"))
      {
         string shaderModel = compiler.GetShaderModel(keywordSO.keywords.ToArray());
         builder.Append("Shader Model : ");
         builder.AppendLine(shaderModel);
         if (maxSamples != arraySampleCount)
         {
            builder.Append("Texture Array Samples : ");
            builder.AppendLine(arraySampleCount.ToString());

            builder.Append("Regular Samples : ");
            builder.AppendLine(textureSampleCount.ToString());
         }
         else
         {
            builder.Append("Texture Array Samples : ");
            builder.AppendLine(arraySampleCount.ToString());
            builder.Append("Regular Samples : ");
            builder.AppendLine(textureSampleCount.ToString());
         }
         if (tessSamples > 0)
         {
            builder.Append("Tessellation Samples : ");
            builder.AppendLine(tessSamples.ToString());
         }
         if (depTexReadLevel > 0)
         {
            builder.Append(depTexReadLevel.ToString());
            builder.AppendLine(" areas with dependent texture reads");
         }

         EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);
      }
         
      if (EditorGUI.EndChangeCheck() && !needsCompile)
      {
         MicroSplatTerrain.SyncAll();
#if __MICROSPLAT_MESH__
         MicroSplatMesh.SyncAll();
#endif
      }

      if (needsCompile)
      {
         needsCompile = false;
         keywordSO.keywords.Clear();
         for (int i = 0; i < compiler.extensions.Count; ++i)
         {
            compiler.extensions[i].Pack(keywordSO);
         }
         if (compiler.renderLoop != null)
         {
            keywordSO.EnableKeyword(compiler.renderLoop.GetRenderLoopKeyword());
         }
         
         // horrible workaround to GUI warning issues
         compileMat = targetMat;
         compileName = shaderName;
         targetCompiler = compiler;
         EditorApplication.delayCall += TriggerCompile;
      }
   }

   static Material compileMat;
   static string compileName;
   static MicroSplatCompiler targetCompiler;
   protected void TriggerCompile()
   {
      targetCompiler.Compile(compileMat, compileName);
   }


   class Module
   {
      public Module(string url, string img)
      {
         assetStore = url;
         texture = Resources.Load<Texture2D>(img);
      }
      public string assetStore;
      public Texture2D texture;
   }

   void InitModules()
   {
      if (modules.Count == 0)
      {
         //
#if !__MICROSPLAT_GLOBALTEXTURE__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96482?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_globaltexture"));
#endif
#if !__MICROSPLAT_SNOW__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96486?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_snow"));
#endif
#if !__MICROSPLAT_TESSELLATION__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96484?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_tessellation"));
#endif
#if !__MICROSPLAT_DETAILRESAMPLE__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96480?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_detailresample"));
#endif
#if !__MICROSPLAT_TERRAINBLEND__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97364?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_terrainblending"));
#endif
#if !__MICROSPLAT_STREAMS__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97993?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_streams"));
#endif
#if !__MICROSPLAT_ALPHAHOLE__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/97495?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_alphahole"));
#endif
#if !__MICROSPLAT_TRIPLANAR__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/96777?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_triplanaruvs"));
#endif
#if !__MICROSPLAT_TEXTURECLUSTERS__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/104223?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_textureclusters"));
#endif
#if !__MICROSPLAT_WINDGLITTER__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/105627?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_windglitter"));
#endif
#if !__MICROSPLAT_ADVANCED_DETAIL__
         modules.Add(new Module("https://www.assetstore.unity3d.com/#!/content/108321?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_module_advanceddetails"));
#endif
//#if !CASCADE
//         modules.Add(new Module("https://assetstore.unity.com/packages/tools/terrain/cascade-rivers-lakes-waterfalls-and-more-106072?aid=1011l37NJ&pubref=1011l37NJ", "microsplat_cascade"));
//#endif

         int n = modules.Count;
         if (n > 1)
         {
            System.Random rnd = new System.Random((int)(UnityEngine.Random.value * 1000)); 
            while (n > 1)
            {  
               n--;  
               int k = rnd.Next(n + 1);  
               var value = modules[k];  
               modules[k] = modules[n];  
               modules[n] = value;  
            } 
         }
      }
       

   }

   List<Module> modules = new List<Module>();

   Module openModule;
   void DrawModule(Module m)
   {
      if (GUILayout.Button(m.texture, GUI.skin.box, GUILayout.Width(128), GUILayout.Height(128)))
      {
         Application.OpenURL(m.assetStore);
      }
   }
   Vector2 moduleScroll;
   void DrawModules()
   {
      InitModules();
      if (modules.Count == 0)
      {
         return;
      }

      EditorGUILayout.LabelField("Want more features? Add them here..");

      moduleScroll = EditorGUILayout.BeginScrollView(moduleScroll, GUILayout.Height(156));
      GUILayout.BeginHorizontal();
      for (int i = 0; i < modules.Count; ++i)
      {
         DrawModule(modules[i]);
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.EndScrollView();

   }
}

