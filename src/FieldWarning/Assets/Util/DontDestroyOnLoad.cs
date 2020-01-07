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

using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFW.Loading
{
    /// <summary>
    /// A component that keeps the object active even if a new scene gets loaded.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
    {
        // used to keep track of duplicates
        private int _id = 0;

        // Start is called before the first frame update
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // checks to see if there are other objects that identically named but have different id's...
            // those are duplicates... remove them.
            var components = FindObjectsOfType<DontDestroyOnLoad>();
            foreach (var c in components)
            {
                if (c.gameObject.name == gameObject.name && _id != c._id)
                {
                    DestroyImmediate(this.gameObject);
                    return;
                }
            }
        }

        private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
        {
            _id++;
        }
    }
}
