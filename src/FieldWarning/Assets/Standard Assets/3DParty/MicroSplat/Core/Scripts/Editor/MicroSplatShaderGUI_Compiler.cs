//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MicroSplat;
using System.Collections.Generic;
using System.Linq;

public partial class MicroSplatShaderGUI : ShaderGUI
{
   public enum PassType
   {
      Surface = 0,
      Color,
      Meta,
      Depth,
      Shadow
   }

   // hacky, but prevents having to change the module api..
   public static PassType passType = PassType.Surface;

   static List<IRenderLoopAdapter> availableRenderLoops = new List<IRenderLoopAdapter>();


   [MenuItem("Assets/Create/Shader/MicroSplat Shader")]
   static void NewShader2()
   {
      NewShader();
   }

   [MenuItem("Assets/Create/MicroSplat/MicroSplat Shader")]
   public static Shader NewShader()
   {
      string path = "Assets";
      foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
      {
         path = AssetDatabase.GetAssetPath(obj);
         if (System.IO.File.Exists(path))
         {
            path = System.IO.Path.GetDirectoryName(path);
         }
         break;
      }
      path = path.Replace("\\", "/");
      path = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
      string name = path.Substring(path.LastIndexOf("/"));
      name = name.Substring(0, name.IndexOf("."));
      MicroSplatCompiler compiler = new MicroSplatCompiler();
      compiler.Init();
      string ret = compiler.Compile(new string[1] { "_MSRENDERLOOP_SURFACESHADER" }, name, name);
      System.IO.File.WriteAllText(path, ret);
      AssetDatabase.Refresh();
      return AssetDatabase.LoadAssetAtPath<Shader>(path);
   }

   public static Material NewShaderAndMaterial(string path, string name, string[] keywords = null)
   {
      string shaderPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
      string shaderBasePath = shaderPath.Replace(".shader", "_Base.shader");
      string matPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.mat");

      MicroSplatCompiler compiler = new MicroSplatCompiler();
      compiler.Init();

      if (keywords == null)
      {
         keywords = new string[0];
      }

      string baseName = "Hidden/MicroSplat/" + name + "_Base";

      string baseShader = compiler.Compile(keywords, baseName);
      string regularShader = compiler.Compile(keywords, name, baseName);

      System.IO.File.WriteAllText(shaderPath, regularShader);
      System.IO.File.WriteAllText(shaderBasePath, baseShader);

     
      if (keywords.Contains("_MESHOVERLAYSPLATS"))
      {
         string meshOverlayShader = compiler.Compile(keywords, name, null, true);
         System.IO.File.WriteAllText(shaderPath.Replace(".shader", "_MeshOverlay.shader"), meshOverlayShader);
      }

      AssetDatabase.Refresh();
      Shader s = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

      Material m = new Material(s);
      AssetDatabase.CreateAsset(m, matPath);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      var kwds = MicroSplatUtilities.FindOrCreateKeywords(m);
      kwds.keywords = new List<string>(keywords);
      EditorUtility.SetDirty(kwds);

      return AssetDatabase.LoadAssetAtPath<Material>(matPath);
   }

   public static Material NewShaderAndMaterial(Terrain t)
   {
      string path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
      return NewShaderAndMaterial(path, t.name);
   }

   public class MicroSplatCompiler
   {
      public List<FeatureDescriptor> extensions = new List<FeatureDescriptor>();

      public string GetShaderModel(string[] features)
      {
         string minModel = "3.5";
         for (int i = 0; i < extensions.Count; ++i)
         {
            if (extensions[i].RequiresShaderModel46())
            {
               minModel = "4.6";
            }
         }
         if (features.Contains("_FORCEMODEL46"))
         {
            minModel = "4.6";
         }
         if (features.Contains("_FORCEMODEL50"))
         {
            minModel = "5.0";
         }

         return minModel;
      }

      public void Init()
      {
         if (extensions.Count == 0)
         {
            string[] paths = AssetDatabase.FindAssets("microsplat_ t:TextAsset");
            for (int i = 0; i < paths.Length; ++i)
            {
               paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
            }


            // init extensions
            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var possible = (from System.Type type in types
                            where type.IsSubclassOf(typeof(FeatureDescriptor))
                            select type).ToArray();

            for (int i = 0; i < possible.Length; ++i)
            {
               var typ = possible[i];
               FeatureDescriptor ext = System.Activator.CreateInstance(typ) as FeatureDescriptor;
               ext.InitCompiler(paths);
               extensions.Add(ext);
            }
            extensions.Sort(delegate (FeatureDescriptor p1, FeatureDescriptor p2)
            {
               if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
               {
                  return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
               }
               return p1.GetType().Name.CompareTo(p2.GetType().Name);
            });


            var adapters = (from System.Type type in types
                            where (type.GetInterfaces().Contains(typeof(IRenderLoopAdapter)))
                            select type).ToArray();

            availableRenderLoops.Clear();
            for (int i = 0; i < adapters.Length; ++i)
            {
               var typ = adapters[i];
               IRenderLoopAdapter adapter = System.Activator.CreateInstance(typ) as IRenderLoopAdapter;
               adapter.Init(paths);
               availableRenderLoops.Add(adapter);
            }

         }
      }



      void WriteFeatures(string[] features, StringBuilder sb)
      {
         sb.AppendLine();
         for (int i = 0; i < features.Length; ++i)
         {
            sb.AppendLine("      #define " + features[i] + " 1");
         }

         sb.AppendLine();
      }

      void WriteExtensions(string[] features, StringBuilder sb)
      {
         // sort for compile order
         extensions.Sort(delegate (FeatureDescriptor p1, FeatureDescriptor p2)
         {
            if (p1.CompileSortOrder() != p2.CompileSortOrder())
               return (p1.CompileSortOrder() < p2.CompileSortOrder()) ? -1 : 1;
            return p1.GetType().Name.CompareTo(p2.GetType().Name);
         });

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
            {
               extensions[i].WriteFunctions(sb);
            }
         }

         // sort by name, then display order..
         extensions.Sort(delegate (FeatureDescriptor p1, FeatureDescriptor p2)
         {
            if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
            {
               return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
            }
            return p1.GetType().Name.CompareTo(p2.GetType().Name);
         });

      }


      void WriteProperties(string[] features, StringBuilder sb, bool blendable)
      {
         sb.AppendLine("   Properties {");

         bool max4 = features.Contains("_MAX4TEXTURES");
         bool max8 = features.Contains("_MAX8TEXTURES");
         bool max12 = features.Contains("_MAX12TEXTURES");
         bool max20 = features.Contains("_MAX20TEXTURES");
         bool max24 = features.Contains("_MAX24TEXTURES");
         bool max28 = features.Contains("_MAX28TEXTURES");
         bool max32 = features.Contains("_MAX32TEXTURES");

         // always have this for UVs
         sb.AppendLine("      [HideInInspector] _Control0 (\"Control0\", 2D) = \"red\" {}");


         bool custom = features.Contains<string>("_CUSTOMSPLATTEXTURES");
         string controlName = "_Control";
         if (custom)
         {
            controlName = "_CustomControl";
         }


         if (custom)
         {
            sb.AppendLine("      [HideInInspector] _CustomControl0 (\"Control0\", 2D) = \"red\" {}");
         }

         if (!max4)
         {

            sb.AppendLine("      [HideInInspector] " + controlName + "1 (\"Control1\", 2D) = \"black\" {}");
         }
         if (!max4 && !max8)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "2 (\"Control2\", 2D) = \"black\" {}");
         }
         if (!max4 && !max8 && !max12)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "3 (\"Control3\", 2D) = \"black\" {}");
         }
         if (max20 || max24 || max28 || max32)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "4 (\"Control4\", 2D) = \"black\" {}");
         }
         if (max24 || max28 || max32)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "5 (\"Control5\", 2D) = \"black\" {}");
         }
         if (max28 || max32)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "6 (\"Control6\", 2D) = \"black\" {}");
         }
         if (max32)
         {
            sb.AppendLine("      [HideInInspector] " + controlName + "7 (\"Control7\", 2D) = \"black\" {}");
         }

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
            {
               ext.WriteProperties(features, sb);
            }
            sb.AppendLine("");
         }
         sb.AppendLine("   }");
      }

      public static bool HasDebugFeature(string[] features)
      {
         return features.Contains("_DEBUG_OUTPUT_ALBEDO") ||
            features.Contains("_DEBUG_OUTPUT_NORMAL") ||
            features.Contains("_DEBUG_OUTPUT_HEIGHT") ||
            features.Contains("_DEBUG_OUTPUT_METAL") ||
            features.Contains("_DEBUG_OUTPUT_SMOOTHNESS") ||
            features.Contains("_DEBUG_OUTPUT_AO") ||
            features.Contains("_DEBUG_OUTPUT_EMISSION");

      }

      public IRenderLoopAdapter renderLoop = null;
      static StringBuilder sBuilder = new StringBuilder(256000);
      public string Compile(string[] features, string name, string baseName = null, bool blendable = false)
      {
         Init();

         // get default render loop if it doesn't exist
         if (renderLoop == null)
         {
            for (int i = 0; i < availableRenderLoops.Count; ++i)
            {
               if (availableRenderLoops[i].GetType() == typeof(SurfaceShaderRenderLoopAdapter))
               {
                  renderLoop = availableRenderLoops[i];
               }
            }
         }

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            ext.Unpack(features);
         }
         sBuilder.Length = 0;
         var sb = sBuilder;
         sb.AppendLine("//////////////////////////////////////////////////////");
         sb.AppendLine("// MicroSplat");
         sb.AppendLine("// Copyright (c) Jason Booth");
         sb.AppendLine("//");
         sb.AppendLine("// Auto-generated shader code, don't hand edit!");
         sb.AppendLine("//   Compiled with MicroSplat " + MicroSplatVersion);
         sb.AppendLine("//   Unity : " + Application.unityVersion);
         sb.AppendLine("//   Platform : " + Application.platform);
         if (renderLoop != null)
         {
            sb.AppendLine("//   RenderLoop : " + renderLoop.GetDisplayName());
         }
         sb.AppendLine("//////////////////////////////////////////////////////");
         sb.AppendLine();

         if (!blendable && baseName == null)
         {
            sb.Append("Shader \"Hidden/MicroSplat/");
         }
         else
         {
            sb.Append("Shader \"MicroSplat/");
         }
         while (name.Contains("/"))
         {
            name = name.Substring(name.IndexOf("/") + 1);
         }
         sb.Append(name);
         if (blendable)
         {
            if (features.Contains("_MESHOVERLAYSPLATS"))
            {
               sb.Append("_MeshOverlay");
            }
            else
            {
               sb.Append("_BlendWithTerrain");
            }
         }
         sb.AppendLine("\" {");


         // props
         WriteProperties(features, sb, blendable);
         renderLoop.WriteShaderHeader(features, sb, this, blendable);


         for (int pass = 0; pass < renderLoop.GetNumPasses(); ++pass)
         {
            renderLoop.WritePassHeader(features, sb, this, pass, blendable);

            // don't remove
            sb.AppendLine();
            sb.AppendLine();

            WriteFeatures(features, sb);
            if (renderLoop == null)
            {
               sb.AppendLine("      #define _MSRENDERLOOP_SURFACESHADER 1");

            }
            else
            {
               sb.AppendLine("      #define " + renderLoop.GetRenderLoopKeyword() + " 1");
            }

            if (blendable)
            {
               if (features.Contains("_MESHOVERLAYSPLATS"))
               {
                  sb.AppendLine("      #define _MESHOVERLAYSPLATSSHADER 1");
               }
               else
               {
                  sb.AppendLine("      #define _TERRAINBLENDABLESHADER 1");
               }
            }

            renderLoop.WriteSharedCode(features, sb, this, pass, blendable);
            passType = renderLoop.GetPassType(pass);
            WriteExtensions(features, sb);

            renderLoop.WriteVertexFunction(features, sb, this, pass, blendable);

            renderLoop.WriteTerrainBody(features, sb, this, pass, blendable);

            renderLoop.WriteFragmentFunction(features, sb, this, pass, blendable);

         }


         renderLoop.WriteShaderFooter(features, sb, this, blendable, baseName);

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            ext.OnPostGeneration(sb, features, name, baseName, blendable);
         }

         sb.AppendLine("");
         renderLoop.PostProcessShader(features, sb, this, blendable);
         string output = sb.ToString();

         // fix newline mixing warnings..
         output = System.Text.RegularExpressions.Regex.Replace(output, "\r\n?|\n", System.Environment.NewLine);
         return output;
      }



      public void Compile(Material m, string shaderName = null)
      {
         int hash = 0;

         MicroSplatKeywords keywords = MicroSplatUtilities.FindOrCreateKeywords(m);

         for (int i = 0; i < keywords.keywords.Count; ++i)
         {
            hash += 31 + keywords.keywords[i].GetHashCode();
         }
         var path = AssetDatabase.GetAssetPath(m.shader);
         string nm = m.shader.name;
         if (!string.IsNullOrEmpty(shaderName))
         {
            nm = shaderName;
         }
         string baseName = "Hidden/" + nm + "_Base" + hash.ToString();

         string terrainShader = Compile(keywords.keywords.ToArray(), nm, baseName);
         if (renderLoop != null)
         {
            keywords.EnableKeyword(renderLoop.GetRenderLoopKeyword());
         }
         string blendShader = null;

         // strip extra feature from terrain blending to make it cheaper
         if (keywords.IsKeywordEnabled("_TERRAINBLENDING"))
         {
            List<string> blendKeywords = new List<string>(keywords.keywords);
            if (keywords.IsKeywordEnabled("_TBDISABLE_DETAILNOISE") && blendKeywords.Contains("_DETAILNOISE"))
            {
               blendKeywords.Remove("_DETAILNOISE");
            }
            if (keywords.IsKeywordEnabled("_TBDISABLE_DETAILNOISE") && blendKeywords.Contains("_ANTITILEARRAYDETAIL"))
            {
               blendKeywords.Remove("_ANTITILEARRAYDETAIL");
            }
            if (keywords.IsKeywordEnabled("_TBDISABLE_DISTANCENOISE") && blendKeywords.Contains("_DISTANCENOISE"))
            {
               blendKeywords.Remove("_DISTANCENOISE");
            }
            if (keywords.IsKeywordEnabled("_TBDISABLE_DISTANCENOISE") && blendKeywords.Contains("_ANTITILEARRAYDISTANCE"))
            {
               blendKeywords.Remove("_ANTITILEARRAYDISTANCE");
            }
            if (keywords.IsKeywordEnabled("_TBDISABLE_DISTANCERESAMPLE") && blendKeywords.Contains("_DISTANCERESAMPLE"))
            {
               blendKeywords.Remove("_DISTANCERESAMPLE");
            }

            blendShader = Compile(blendKeywords.ToArray(), nm, null, true);
         }



         string meshBlendShader = null;
         if (keywords.IsKeywordEnabled("_MESHOVERLAYSPLATS"))
         {
            List<string> blendKeywords = new List<string>(keywords.keywords);
            if (blendKeywords.Contains("_TESSDISTANCE"))
            {
               blendKeywords.Remove("_TESSDISTANCE");
            }
            meshBlendShader = Compile(blendKeywords.ToArray(), nm, null, true);
         }

         MicroSplatUtilities.Checkout(path);
         System.IO.File.WriteAllText(path, terrainShader);

         if (!keywords.IsKeywordEnabled("_MICROMESH"))
         {
            // generate fallback
            string[] oldKeywords = new string[keywords.keywords.Count];
            System.Array.Copy(keywords.keywords.ToArray(), oldKeywords, keywords.keywords.Count);
            keywords.DisableKeyword("_TESSDISTANCE");
            keywords.DisableKeyword("_PARALLAX");
            keywords.DisableKeyword("_DETAILNOISE");


            string fallback = Compile(keywords.keywords.ToArray(), baseName);
            keywords.keywords = new List<string>(oldKeywords);
            string fallbackPath = path.Replace(".shader", "_Base.shader");
            MicroSplatUtilities.Checkout(fallbackPath);
            System.IO.File.WriteAllText(fallbackPath, fallback);
         }


         string terrainBlendPath = path.Replace(".shader", "_TerrainObjectBlend.shader");
         string meshBlendPath = path.Replace(".shader", "_MeshOverlay.shader");

         if (blendShader != null)
         {
            MicroSplatUtilities.Checkout(terrainBlendPath);
            System.IO.File.WriteAllText(terrainBlendPath, blendShader);
         }
         if (meshBlendShader != null)
         {
            MicroSplatUtilities.Checkout(meshBlendPath);
            System.IO.File.WriteAllText(meshBlendPath, meshBlendShader);
         }

         EditorUtility.SetDirty(m);
         AssetDatabase.Refresh();
         MicroSplatTerrain.SyncAll();
      }
   }
}
