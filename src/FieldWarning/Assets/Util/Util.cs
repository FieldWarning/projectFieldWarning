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
        /// TODO for some reason this loads much blurrier images than the manual
        /// sprite import through unity. Until this is fixed we can't
        /// use it as our main method.
        /// </summary>
        /// <returns></returns>
        static public Sprite LoadSpriteFromFile(
                string filename, 
                float PixelsPerUnit = 100.0f,
                SpriteMeshType type = SpriteMeshType.Tight)
        {
            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
            Texture2D SpriteTexture = LoadTexture(filename);
            if (SpriteTexture == null)
            {
                Logger.LogConfig(LogLevel.WARNING, 
                    $"Could not load sprite {filename}, " +
                    $"file does not exist or is not an image.");
                return null;
            }

            Sprite NewSprite = Sprite.Create(
                    SpriteTexture, 
                    new Rect(0, 
                            0, 
                            SpriteTexture.width, 
                            SpriteTexture.height), 
                    new Vector2(0, 0),
                    PixelsPerUnit, 
                    0,
                    type,
                    new Vector4(0, 0, 0, 0));

            return NewSprite;
        }

        static private Texture2D LoadTexture(string FilePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);
                // Load the imagedata into the texture (size is set automatically)
                if (Tex2D.LoadImage(FileData))
                {
                    return Tex2D;  // If data = readable -> return texture
                }
            }
            return null;
        }
    }
}
