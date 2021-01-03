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

using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PFW.UI.MainMenu
{
    public class LoadSceneOnClick : MonoBehaviour
    {
        private bool _isRunning = false;
        [SerializeField]
        private Image _progressBar = null;
        [SerializeField]
        private TMP_Dropdown _mapSelect = null;

        struct Scene
        {
            public Scene(string alias, string path)
            {
                Alias = alias;
                Path = path;
            }

            public string Alias;  //< Player-friendly scene name
            public string Path;   //< Path as seen in the build settings
        }

        private readonly Scene[] MAPS =
        {
            new Scene("Full Feature 2", "Scene/full-feature-scene-2/full-feature-scene2"),
            new Scene("Rush Valley", "Scene/Maps/1v1 Map/1v1Map")
        };

        private void Awake()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (Scene s in MAPS)
            {
                options.Add(new TMP_Dropdown.OptionData(s.Alias));
            }
            _mapSelect.ClearOptions();
            _mapSelect.AddOptions(options);
        }

        public void LoadMapAsyncWrapper()
        {
            string sceneName = MAPS[_mapSelect.value].Path;
            if (!_isRunning)
            {
                Logger.LogLoading(LogLevel.INFO, $"Trying to load {sceneName}.");
                _isRunning = true;
                StartCoroutine(LoadMapAsync(sceneName));
            }
        }

        public IEnumerator LoadMapAsync(string sceneName)
        {
            yield return null;

            AsyncOperation asyncMapLoading = SceneManager.LoadSceneAsync(sceneName);
            _progressBar.enabled = true;

            while (!asyncMapLoading.isDone)
            {
                // Unity only uses values between 0 and 0.9 for tracking progress.
                float progress = Mathf.Clamp01(asyncMapLoading.progress / 0.9f);
                _progressBar.fillAmount = progress;

                yield return null;
            }
        }
    }
}
