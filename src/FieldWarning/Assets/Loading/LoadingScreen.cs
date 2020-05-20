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

using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PFW.Model.Armory;

namespace PFW.Loading
{
    /// <summary>
    /// The loading screen handles the creation of thread for the workers and
    /// querying for how much work is left. This displays the current work performed to the UI.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        static public Queue<Loader> SWorkers = new Queue<Loader>();
        static public int SceneBuildId;

        private Slider _slider;
        private TextMeshProUGUI _descLbl;
        private TextMeshProUGUI _versionLbl;

        private Loader _currentWorker = null;

        [SerializeField]
        private GameObject _loadedData = null;


        private void Awake()
        {
            Application.runInBackground = true;
        }

        // Start is called before the first frame update
        private void Start()
        {
            _slider = transform.Find("Slider").GetComponent<Slider>();
            _descLbl = GameObject.Find("LoadingLbl").GetComponent<TextMeshProUGUI>();
            _versionLbl = GameObject.Find("MapVersionLbl").GetComponent<TextMeshProUGUI>();


            Instantiate(_loadedData);
            LoadedData.SceneBuildId = SceneBuildId;

            MapVersion version = GameObject.Find("map").GetComponent<MapVersion>();

            if (version)
            {
                _versionLbl.SetText("Map Name: " + version.Name + "\nVersion:" + version.Version);
            }
            else
            {
                _versionLbl.SetText("Unable to locate map version info");
            }
        }


        private void Update()
        {
            if (SWorkers.Count > 0)
            {
                if (_currentWorker == null)
                {
                    _currentWorker = SWorkers.Peek();
                }

                if (_currentWorker.IsFinished())
                {
                    SWorkers.Dequeue();
                    _slider.value = _slider.maxValue;
                    _currentWorker = null;
                }
                else
                {
                    _descLbl.text = _currentWorker.GetDescription();

                    _slider.value = (float)_currentWorker.GetPercentComplete();
                }
            }
            else
            {
                // dispose of the screen when no more workers in queue
                gameObject.SetActive(false);

                SceneManager.LoadSceneAsync(SceneBuildId, LoadSceneMode.Single);
            }
        }
    }
}
