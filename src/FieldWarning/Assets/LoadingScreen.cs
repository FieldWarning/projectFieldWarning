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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The loading screen handles the creation of thread for the workers and
/// querying for how much work is left. This displays the current work performed to the UI.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    static public Queue<Loading> SWorkers = new Queue<Loading>();

    private Slider _slider;
    private Text _descLbl;

    // the thread which runs all the workers.
    // NOTE: some mono specific processes cannot run inside other threads.
    private Thread _thread;
    private Loading _currentWorker = null;

    // Start is called before the first frame update
    private void Start()
    {
        _slider = transform.Find("Slider").GetComponent<Slider>();
        _descLbl = transform.Find("LoadingLbl").GetComponent<Text>();
    }

    private void Update()
    {

        if (SWorkers.Count > 0)
        {
            if (_currentWorker == null)
            {
                _currentWorker = SWorkers.Peek();
            }

            if (!_currentWorker.Started)
            {
                _thread = new Thread(_currentWorker.Load);
                _thread.Start();
            }

            _descLbl.text = _currentWorker.Name;

            if (_currentWorker.CurrentWorker != null)
            {
                // TODO: change formatting to make it look a bit better
                _descLbl.text = _currentWorker.Name + "\t" + _currentWorker.CurrentWorker.Item2;
            }

            _slider.value = (float)_currentWorker.PercentDone;

            if (_currentWorker.Finished)
            {
                SWorkers.Dequeue();
                _slider.value = _slider.maxValue;
                _currentWorker = null;
            }
        }
        else
        {
            // dispose of the screen when no more workers in queue
            gameObject.SetActive(false);
        }
    }
}
