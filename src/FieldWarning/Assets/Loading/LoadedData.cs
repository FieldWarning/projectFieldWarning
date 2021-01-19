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

using PFW.Model;
using PFW.Units.Component.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFW.Loading
{
    public class LoadedData : MonoBehaviour
    {
        public TerrainMap TerrainData;
        public PathfinderData PathFinderData;
        public static int SceneBuildId;

        //private Loading _loader;

        // Start is called before the first frame update
        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();

            TerrainData = new TerrainMap(terrains, SceneBuildId);

            PathFinderData = new PathfinderData(
                    TerrainData, 
                    GameSession.Singleton.Armory.UniqueMobilityTypes, 
                    SceneBuildId);
        }

        private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
        {
            // Loading is done so the data object should not persist across scenes anymore:
            Util.RevertDontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
