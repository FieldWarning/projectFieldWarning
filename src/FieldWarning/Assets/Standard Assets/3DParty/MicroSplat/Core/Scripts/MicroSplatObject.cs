//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroSplatObject : MonoBehaviour 
{
   [HideInInspector]
   public Material templateMaterial;
   [System.NonSerialized]
   [HideInInspector]
   public Material matInstance;

   [HideInInspector]
   public Texture2D terrainDesc;

   [HideInInspector]
   public Material blendMat;

   [HideInInspector]
   public Material blendMatInstance;

   [HideInInspector]
   public MicroSplatKeywords keywordSO;

   [HideInInspector]
   public Texture2D perPixelNormal;

   [HideInInspector]
   public Texture2D tintMapOverride;
   [HideInInspector]
   public Texture2D globalNormalOverride;
   [HideInInspector]
   public Texture2D geoTextureOverride;
   [HideInInspector]
   public Texture2D streamTexture;
   [HideInInspector]
   public Texture2D vsGrassMap;
   [HideInInspector]
   public Texture2D vsShadowMap;
   [HideInInspector]
   public Texture2D advDetailControl;
   [HideInInspector]
   public Texture2D clipMap;

   [HideInInspector]
   public Texture2D customControl0;
   [HideInInspector]
   public Texture2D customControl1;
   [HideInInspector]
   public Texture2D customControl2;
   [HideInInspector]
   public Texture2D customControl3;
   [HideInInspector]
   public Texture2D customControl4;
   [HideInInspector]
   public Texture2D customControl5;
   [HideInInspector]
   public Texture2D customControl6;
   [HideInInspector]
   public Texture2D customControl7;

#if __MICROSPLAT_PROCTEX__
   [HideInInspector]
   public MicroSplatProceduralTextureConfig procTexCfg;
   [HideInInspector]
   public Texture2D procBiomeMask;
#endif



   [HideInInspector]
   public MicroSplatPropData propData;

   protected void ApplyMaps(Material m)
   {
      if (m.HasProperty("_GeoTex") && geoTextureOverride != null)
      {
         m.SetTexture("_GeoTex", geoTextureOverride);
      }
      if (m.HasProperty("_GeoCurve") && propData != null)
      {
         m.SetTexture("_GeoCurve", propData.GetGeoCurve());
      }
      if (m.HasProperty("_AlphaHoleTexture") && clipMap != null)
      {
         m.SetTexture("_AlphaHoleTexture", clipMap);
      }
      if (m.HasProperty("_PerPixelNormal"))
      {
         m.SetTexture("_PerPixelNormal", perPixelNormal);
      }

      if (m.HasProperty("_GlobalTintTex") && tintMapOverride != null)
      {
         m.SetTexture("_GlobalTintTex", tintMapOverride);
      }
      if (m.HasProperty("_GlobalNormalTex") && globalNormalOverride != null)
      {
         m.SetTexture("_GlobalNormalTex", globalNormalOverride);
      }
      if (m.HasProperty("_VSGrassMap") && vsGrassMap != null)
      {
         m.SetTexture("_VSGrassMap", vsGrassMap);
      }
      if (m.HasProperty("_VSShadowMap") && vsShadowMap != null)
      {
         m.SetTexture("_VSShadowMap", vsShadowMap);
      }
      if (m.HasProperty("_StreamControl"))
      {
         m.SetTexture("_StreamControl", streamTexture);
      }
      if (m.HasProperty("_AdvDetailControl"))
      {
         m.SetTexture("_AdvDetailControl", advDetailControl);
      }

      if (propData != null)
      {
         m.SetTexture("_PerTexProps", propData.GetTexture());
      }

      #if __MICROSPLAT_PROCTEX__
      if (procTexCfg != null)
      {
         m.SetTexture("_ProcTexCurves", procTexCfg.GetCurveTexture());
         m.SetTexture("_ProcTexParams", procTexCfg.GetParamTexture());
         m.SetInt("_PCLayerCount", procTexCfg.layers.Count);
         if (procBiomeMask != null && m.HasProperty("_ProcTexBiomeMask"))
         {
            m.SetTexture("_ProcTexBiomeMask", procBiomeMask);
         }
      }
      #endif
}

protected void ApplyControlTextures(Texture2D[] controls, Material m)
   {
      m.SetTexture("_Control0", controls.Length > 0 ? controls[0] : Texture2D.blackTexture);
      m.SetTexture("_Control1", controls.Length > 1 ? controls[1] : Texture2D.blackTexture);
      m.SetTexture("_Control2", controls.Length > 2 ? controls[2] : Texture2D.blackTexture);
      m.SetTexture("_Control3", controls.Length > 3 ? controls[3] : Texture2D.blackTexture);
      m.SetTexture("_Control4", controls.Length > 4 ? controls[4] : Texture2D.blackTexture);
      m.SetTexture("_Control5", controls.Length > 5 ? controls[5] : Texture2D.blackTexture);
      m.SetTexture("_Control6", controls.Length > 6 ? controls[6] : Texture2D.blackTexture);
      m.SetTexture("_Control7", controls.Length > 7 ? controls[7] : Texture2D.blackTexture);

   }

   protected void SyncBlendMat(Vector3 size)
   {
      if (blendMatInstance != null && matInstance != null)
      {
         blendMatInstance.CopyPropertiesFromMaterial(matInstance);
         Vector4 bnds = new Vector4();
         bnds.z = size.x;
         bnds.w = size.z;
         bnds.x = transform.position.x;
         bnds.y = transform.position.z;
         blendMatInstance.SetVector("_TerrainBounds", bnds);
         blendMatInstance.SetTexture("_TerrainDesc", terrainDesc);
      }
   }

   public virtual Bounds GetBounds() { return new Bounds();  }


   public Material GetBlendMatInstance()
   {
      if (blendMat != null && terrainDesc != null)
      {
         if (blendMatInstance == null)
         {
            blendMatInstance = new Material(blendMat);
            SyncBlendMat(GetBounds().size);
         }
         if (blendMatInstance.shader != blendMat.shader)
         {
            blendMatInstance.shader = blendMat.shader;
            SyncBlendMat(GetBounds().size);
         }
      }
      return blendMatInstance;
   }

   protected void ApplyBlendMap()
   {
      if (blendMat != null && terrainDesc != null)
      {
         if (blendMatInstance == null)
         {
            blendMatInstance = new Material(blendMat);
         }

         SyncBlendMat(GetBounds().size); 
      }  
   }

   public void RevisionFromMat()
   {
#if UNITY_EDITOR
      if (keywordSO == null && templateMaterial != null)
      {
         var path = UnityEditor.AssetDatabase.GetAssetPath(templateMaterial);
         path = path.Replace(".mat", "_keywords.asset");
         keywordSO = UnityEditor.AssetDatabase.LoadAssetAtPath<MicroSplatKeywords>(path);
         if (keywordSO == null)
         {
            keywordSO = ScriptableObject.CreateInstance<MicroSplatKeywords>();
            keywordSO.keywords = new List<string>(templateMaterial.shaderKeywords);
            UnityEditor.AssetDatabase.CreateAsset(keywordSO, path);
            UnityEditor.AssetDatabase.SaveAssets();
            templateMaterial.shaderKeywords = null;
         }
         UnityEditor.EditorUtility.SetDirty(this);
      }
#endif
   }
}
