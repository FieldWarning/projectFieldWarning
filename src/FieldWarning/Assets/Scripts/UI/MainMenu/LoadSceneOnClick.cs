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

namespace PFW.UI.MainMenu
{
    public class LoadSceneOnClick : MonoBehaviour
    {
        private bool _isRunning = false;
        [SerializeField]
        private Image _progressBar = null;

        public void LoadMapAsyncWrapper(string sceneName)
        {
            if (!_isRunning) {
                _isRunning = true;
                StartCoroutine(LoadMapAsync(sceneName));
            }
        }

        public IEnumerator LoadMapAsync(string sceneName)
        {
            yield return null;

            AsyncOperation asyncMapLoading = SceneManager.LoadSceneAsync(sceneName);
            _progressBar.enabled = true;

            while (!asyncMapLoading.isDone) {
                // Unity only uses values between 0 and 0.9 for tracking progress.
                float progress = Mathf.Clamp01(asyncMapLoading.progress / 0.9f);
                _progressBar.fillAmount = progress;

                yield return null;
            }
        }
    }
}
