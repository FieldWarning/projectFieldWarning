/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */

using System.IO;
using UnityEngine;

namespace PFW
{
    public static class Util
    {
        public static bool GetTerrainClickLocation(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Find the first child by name, depth-first search.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform result = RecursiveFindChild(child, childName);
                    if (result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public static void Swap(ref int x, ref int y)
        {
            int tmp = x;
            x = y;
            y = tmp;
            return;
        }

        public static void Swap(ref Vector3 x, ref Vector3 y)
        {
            Vector3 tmp = x;
            x = y;
            y = tmp;
            return;
        }

        public static float RoundTowardZero(float f)
        {
            return f > 0 ?
                    (float)System.Math.Floor(f)
                    : (float)System.Math.Ceiling(f);
        }

        /// <summary>
        /// Loads a sprite that is not packaged into the game but rather sits
        /// somewhere on the file system.
        /// 
        /// There are still some differences in how sprites appear when loaded by
        /// this code as opposed to using the inspector, would be nice to find out why.
        /// </summary>
        /// <returns></returns>
        static public Sprite LoadSpriteFromFile(
                string filename, 
                float pixelsPerUnit = 100.0f,
                SpriteMeshType type = SpriteMeshType.FullRect)
        {
            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
            Texture2D spriteTexture = LoadTexture(filename);
            if (spriteTexture == null)
            {
                Logger.LogConfig(LogLevel.WARNING, 
                    $"Could not load sprite {filename}, " +
                    $"file does not exist or is not an image.");
                return null;
            }

            Sprite newSprite = Sprite.Create(
                    spriteTexture, 
                    new Rect(0, 
                            0,
                            spriteTexture.width,
                            spriteTexture.height), 
                    new Vector2(0, 0),
                    pixelsPerUnit, 
                    0,
                    type,
                    new Vector4(0, 0, 0, 0));

            return newSprite;
        }

        static private Texture2D LoadTexture(string FilePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D tex2D;
            byte[] fileData;

            if (File.Exists(FilePath))
            {
                fileData = File.ReadAllBytes(FilePath);
                tex2D = new Texture2D(0, 0)
                {
                    filterMode = FilterMode.Trilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                // Load the imagedata into the texture (size is set automatically)
                if (tex2D.LoadImage(fileData))
                {
                    return tex2D;  // If data = readable -> return texture
                }
            }
            return null;
        }
    }
}
