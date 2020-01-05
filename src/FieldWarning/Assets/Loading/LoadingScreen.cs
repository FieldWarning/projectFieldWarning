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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// The loading screen handles the creation of thread for the workers and
/// querying for how much work is left. This displays the current work performed to the UI.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    static public Queue<Loader> SWorkers = new Queue<Loader>();
    static public int destinationScene;

    private Slider _slider;
    private TextMeshProUGUI _descLbl;

    // the thread which runs all the workers.
    // NOTE: some mono specific processes cannot run inside other threads.
    private Thread _thread;
    private Loader _currentWorker = null;
    private GameObject _HUD;
    private GameObject _managers;

    public GameObject loadedData;


    public void Awake()
    {
        Application.runInBackground = true;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // need to deactivate camera so its not moved by the player.. since this is a loading screen


        _slider = transform.Find("Slider").GetComponent<Slider>();
        _descLbl = GameObject.Find("LoadingLbl").GetComponent<TextMeshProUGUI>();

        Instantiate(loadedData);
        LoadedData.scene = destinationScene;
    }


    private void Update()
    {

        if (SWorkers.Count > 0)
        {
            if (_currentWorker == null)
            {
                _currentWorker = SWorkers.Peek();
            }

            if (_currentWorker.isFinished())
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
            // this stuff only gets ran once because we set this object to inactive


            // dispose of the screen when no more workers in queue
            gameObject.SetActive(false);

            var components = FindObjectsOfType<DontDestroyOnLoad>();
            foreach (var c in components)
            {
                c.id++;
                Destroy(c.gameObject);
            }
            SceneManager.LoadSceneAsync(destinationScene, LoadSceneMode.Single);

        }
    }
}
