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
        // Tracks which object to keep (lower id -> object is newer)
        private int _id = 0;

        // When a duplicate is detected, do we erase the newer or older instance?
        [SerializeField]
        private bool _keepOlder = true;

        // Start is called before the first frame update
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
        {
            _id++;

            // checks to see if there are other objects that identically named 
            // but have different ids; those are duplicates -> remove them.
            DontDestroyOnLoad[] components = FindObjectsOfType<DontDestroyOnLoad>();
            foreach (DontDestroyOnLoad c in components)
            {
                // Only proceed if this really is a duplicate 
                // with the same name who is also not this exact object
                if (c.gameObject.name != gameObject.name || _id == c._id)
                {
                    continue;
                }

                if (_keepOlder)
                {
                    if (_id < c._id)
                    {
                        Logger.LogLoading(LogLevel.DEBUG, "Destroying duplicate. Id: " + _id);
                        SceneManager.sceneLoaded -= OnSceneLoaded;
                        DestroyImmediate(this.gameObject);

                        // This script exists to support a trampoline where we go
                        // from scene A, to loading scene, to scene A again.
                        // If we find a duplicate, it means we're at the end of the process
                        // so this object no longer needs to persist with scene changes.
                        Util.RevertDontDestroyOnLoad(c.gameObject);
                        SceneManager.sceneLoaded -= c.OnSceneLoaded;
                        Destroy(c);
                        return;
                    }
                }
                else
                {
                    if (_id > c._id)
                    {
                        Logger.LogLoading(LogLevel.DEBUG, "Destroying duplicate. Id: " + _id);
                        SceneManager.sceneLoaded -= OnSceneLoaded;
                        DestroyImmediate(this.gameObject);

                        // This script exists to support a trampoline where we go
                        // from scene A, to loading scene, to scene A again.
                        // If we find a duplicate, it means we're at the end of the process
                        // so this object no longer needs to persist with scene changes.
                        Util.RevertDontDestroyOnLoad(c.gameObject);
                        SceneManager.sceneLoaded -= c.OnSceneLoaded;
                        Destroy(c);
                        return;
                    }
                }
            }
        }
    }
}
