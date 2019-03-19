//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   [CreateAssetMenu(menuName = "MicroSplat/Texture Array Config", order = 1)]
   [ExecuteInEditMode]
   public class TextureArrayConfig : ScriptableObject 
   {
      public enum AllTextureChannel
      {
         R = 0,
         G,
         B,
         A,
         Custom
      }


      public enum TextureChannel
      {
         R = 0,
         G,
         B,
         A
      }

      public enum Compression
      {
         AutomaticCompressed,
         ForceDXT,
         ForcePVR,
         ForceETC2,
         ForceASTC,
         ForceCrunch,
         Uncompressed
      }

      public enum TextureSize
      {
         k4096 = 4096,
         k2048 = 2048,
         k1024 = 1024,
         k512 = 512,
         k256 = 256,
         k128 = 128,
         k64 = 64,
         k32 = 32,
      }

      [System.Serializable]
      public class TextureArraySettings
      {
         public TextureArraySettings(TextureSize s, Compression c, FilterMode f, int a = 1)
         {
            textureSize = s;
            compression = c;
            filterMode = f;
            Aniso = a;
         }

         public TextureSize textureSize;
         public Compression compression;
         public FilterMode filterMode;
         [Range(0, 16)]
         public int Aniso = 1;
      }


      public enum PackingMode
      {
         Fastest,
         Quality
      }

      public enum SourceTextureSize
      {
         Unchanged,
         k32 = 32,
         k256 = 256,
      }

      // for the interface
      public enum TextureMode
      {
         Basic,
         PBR,
         #if __MICROSPLAT_ADVANCED_DETAIL__
         AdvancedDetails,
         #endif
      }

      public enum ClusterMode
      {
         None,
         TwoVariations,
         ThreeVariations
      }

      public bool IsAdvancedDetail()
      {
      #if __MICROSPLAT_ADVANCED_DETAIL__
         return textureMode == TextureMode.AdvancedDetails;
      #else
         return false;
      #endif
      }
         
      [HideInInspector]
      public bool antiTileArray = false;

      [HideInInspector]
      public bool emisMetalArray = false;

      [HideInInspector]
      public TextureMode textureMode = TextureMode.PBR;   

      [HideInInspector]
      public ClusterMode clusterMode = ClusterMode.None;

      [HideInInspector]
      public PackingMode packingMode = PackingMode.Fastest;

      [HideInInspector]
      public int hash;


      static List<TextureArrayConfig> sAllConfigs = new List<TextureArrayConfig>();
      void Awake()
      {
         sAllConfigs.Add(this);
      }

      void OnDestroy()
      {
         sAllConfigs.Remove(this);
      }

      #if UNITY_EDITOR
      public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
      {
         List<T> assets = new List<T>();
         string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).ToString().Replace("UnityEngine.", "")));
         for( int i = 0; i < guids.Length; i++ )
         {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath( guids[i] );
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>( assetPath );
            if( asset != null )
            {
               assets.Add(asset);
            }
         }
         return assets;
      }
      #endif

      public static TextureArrayConfig FindConfig(Texture2DArray diffuse)
      {
         #if UNITY_EDITOR
         if (sAllConfigs.Count == 0)
         {
            sAllConfigs = FindAssetsByType<TextureArrayConfig>();
         }
         #endif

         for (int i = 0; i < sAllConfigs.Count; ++i)
         {
            if (sAllConfigs[i].diffuseArray == diffuse)
            {
               return sAllConfigs[i];
            }
         }
         return null;
      }

      [HideInInspector]
      public Texture2DArray diffuseArray;
      [HideInInspector]
      public Texture2DArray normalSAOArray;
      [HideInInspector]
      public Texture2DArray smoothAOArray;

      [HideInInspector]
      public Texture2DArray diffuseArray2;
      [HideInInspector]
      public Texture2DArray normalSAOArray2;
      [HideInInspector]
      public Texture2DArray smoothAOArray2;

      [HideInInspector]
      public Texture2DArray diffuseArray3;
      [HideInInspector]
      public Texture2DArray normalSAOArray3;
      [HideInInspector]
      public Texture2DArray smoothAOArray3;

      [HideInInspector]
      public Texture2DArray emisArray;
      [HideInInspector]
      public Texture2DArray emisArray2;
      [HideInInspector]
      public Texture2DArray emisArray3;

      [System.Serializable]
      public class TextureArrayGroup
      {
         public TextureArraySettings diffuseSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear, 1);
         public TextureArraySettings normalSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear, 1);
         public TextureArraySettings smoothSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear, 1);
         public TextureArraySettings antiTileSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear, 1);
         public TextureArraySettings emissiveSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear, 1);
      }


      [System.Serializable]
      public class PlatformTextureOverride
      {
         #if UNITY_EDITOR
         public UnityEditor.BuildTarget platform = UnityEditor.BuildTarget.StandaloneWindows;
         #endif
         public TextureArrayGroup settings = new TextureArrayGroup(); 
      }


      // default settings, and overrides
      public TextureArrayGroup defaultTextureSettings = new TextureArrayGroup();
      public List<PlatformTextureOverride> platformOverrides = new List<PlatformTextureOverride>();

      public SourceTextureSize sourceTextureSize = SourceTextureSize.Unchanged;

      [HideInInspector]
      public AllTextureChannel allTextureChannelHeight = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelSmoothness = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelAO = AllTextureChannel.G;
      [HideInInspector]     
      public AllTextureChannel allTextureChannelAlpha = AllTextureChannel.A;

      [System.Serializable]
      public class TextureEntry
      {
         #if !UNITY_2017_3_OR_NEWER
         public ProceduralMaterial substance;
         #endif
         #if SUBSTANCE_PLUGIN_ENABLED
         public Substance.Game.Substance substance;
         #endif
         public Texture2D diffuse;
         public Texture2D height;
         public TextureChannel heightChannel = TextureChannel.G;
         public Texture2D normal;
         public Texture2D smoothness;
         public TextureChannel smoothnessChannel = TextureChannel.G;
         public bool isRoughness;
         public Texture2D ao;
         public TextureChannel aoChannel = TextureChannel.G;       
         public Texture2D alpha;
         public TextureChannel alphaChannel = TextureChannel.G;
         public bool normalizeAlpha;

         public Texture2D emis;
         public Texture2D metal;
         public TextureChannel metalChannel = TextureChannel.G;

         // anti tile
         public Texture2D noiseNormal;
         public Texture2D detailNoise;
         public TextureChannel detailChannel = TextureChannel.G;      
         public Texture2D distanceNoise;
         public TextureChannel distanceChannel = TextureChannel.G;      

         public void Reset()
         {
            #if !UNITY_2017_3_OR_NEWER || SUBSTANCE_PLUGIN_ENABLED
            substance = null;
            #endif
            diffuse = null;
            height = null;
            normal = null;
            smoothness = null;
            ao = null;
            isRoughness = false;
            alpha = null;
            detailNoise = null;
            distanceNoise = null;
            metal = null;
            emis = null;
            heightChannel = TextureChannel.G;
            smoothnessChannel = TextureChannel.G;
            aoChannel = TextureChannel.G;
            alphaChannel = TextureChannel.G;
            distanceChannel = TextureChannel.G;
            detailChannel = TextureChannel.G;
         }

         public bool HasTextures()
         {
            return (
               #if !UNITY_2017_3_OR_NEWER || SUBSTANCE_PLUGIN_ENABLED
               substance != null || 
               #endif
               diffuse != null || 
               height != null || 
               normal != null || 
               smoothness != null || 
               ao != null);
         }
      }

      [HideInInspector]
      public List<TextureEntry> sourceTextures = new List<TextureEntry>();
      [HideInInspector]
      public List<TextureEntry> sourceTextures2 = new List<TextureEntry>();
      [HideInInspector]
      public List<TextureEntry> sourceTextures3 = new List<TextureEntry>();
   }
}
