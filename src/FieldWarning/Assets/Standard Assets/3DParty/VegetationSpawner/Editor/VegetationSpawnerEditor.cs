// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Staggart.VegetationSpawner
{
    public class VegetationSpawnerEditor
    {

        public static void DrawRangeSlider(GUIContent label, ref Vector2 input, float min, float max)
        {
            float minBrightness = input.x;
            float maxBrightness = input.y;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

                minBrightness = EditorGUILayout.FloatField(minBrightness, GUILayout.Width(40f));
                EditorGUILayout.MinMaxSlider(ref minBrightness, ref maxBrightness, min, max);
                maxBrightness = EditorGUILayout.FloatField(maxBrightness, GUILayout.Width(40f));
            }

            input.x = minBrightness;
            input.y = maxBrightness;

        }

        private static GUIStyle _PreviewTex;
        public static GUIStyle PreviewTex
        {
            get
            {
                if (_PreviewTex == null)
                {
                    _PreviewTex = new GUIStyle(EditorStyles.label)
                    {
                        clipping = TextClipping.Clip,
                        alignment = TextAnchor.MiddleCenter,
                        imagePosition = ImagePosition.ImageAbove
                    };
                }
                return _PreviewTex;
            }
        }

        private static GUIStyle _PreviewTexSelected;
        public static GUIStyle PreviewTexSelected
        {
            get
            {
                if (_PreviewTexSelected == null)
                {
                    _PreviewTexSelected = new GUIStyle(EditorStyles.objectFieldThumb)
                    {
                        clipping = TextClipping.Clip,
                        alignment = TextAnchor.MiddleCenter,
                        imagePosition = ImagePosition.ImageAbove
                    };
                }
                return _PreviewTexSelected;
            }
        }

        private static Texture _TerrainIcon;
        public static Texture TerrainIcon
        {
            get
            {
                if (_TerrainIcon == null)
                {
#if UNITY_2019_3_OR_NEWER
                    _TerrainIcon = EditorGUIUtility.IconContent("d_Terrain Icon").image;
#else
                    _TerrainIcon = EditorGUIUtility.IconContent("Terrain Icon").image;
#endif
                }
                return _TerrainIcon;
            }
        }

        private static Texture _TreeIcon;
        public static Texture TreeIcon
        {
            get
            {
                if (_TreeIcon == null)
                {
                    _TreeIcon = EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolTrees").image;
                }
                return _TreeIcon;
            }
        }

        private static Texture _DetailIcon;
        public static Texture DetailIcon
        {
            get
            {
                if (_DetailIcon == null)
                {
                    _DetailIcon = EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolPlants").image;
                }
                return _DetailIcon;
            }
        }

        private static Texture _PlusIcon;
        public static Texture PlusIcon
        {
            get
            {
                if (_PlusIcon == null)
                {
                    _PlusIcon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image;
                }
                return _PlusIcon;
            }
        }

        private static Texture _TrashIcon;
        public static Texture TrashIcon
        {
            get
            {
                if (_TrashIcon == null)
                {
                    _TrashIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash").image;
                }
                return _TrashIcon;
            }
        }

        public class Log
        {
            private static int MaxItems = 9;

            public static List<string> items = new List<string>();

            public static void Add(string text)
            {
                if (items.Count >= MaxItems) items.RemoveAt(items.Count - 1);

                string hourString = ((DateTime.Now.Hour <= 9) ? "0" : "") + DateTime.Now.Hour;
                string minuteString = ((DateTime.Now.Minute <= 9) ? "0" : "") + DateTime.Now.Minute;
                string secString = ((DateTime.Now.Second <= 9) ? "0" : "") + DateTime.Now.Second;
                string timeString = "[" + hourString + ":" + minuteString + ":" + secString + "] ";

                items.Insert(0, timeString + text);
            }
        }

    }
}