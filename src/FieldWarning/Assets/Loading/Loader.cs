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

namespace PFW.Loading
{
    /// <summary>
    /// Handles which order to load / execute the various specified functions.
    /// </summary>
    public class Loader
    {
        private Queue<Worker> _workers = new Queue<Worker>();
        private Worker _currentWorker;

        public Loader()
        {
            LoadingScreen.SWorkers.Enqueue(this);
        }

        public void AddCouroutine(WorkerCoroutineDelegate func, string desc)
        {
            _workers.Enqueue(new CoroutineWorker(func, desc));
        }

        public void AddMultithreadedRoutine(WorkerThreadDelegate func, string desc)
        {
            _workers.Enqueue(new MultithreadedWorker(func, desc));
        }

        // TODO: make these into properties
        public string GetDescription()
        {
            if (_currentWorker != null)
                return _currentWorker.Description;

            return "";
        }

        public double GetPercentComplete()
        {
            if (_currentWorker != null)
                return _currentWorker.PercentDone;

            return 0;
        }

        public void SetPercentComplete(double percent)
        {
            if (_currentWorker != null)
                _currentWorker.PercentDone = percent;
        }

        public bool IsFinished()
        {
            if (_workers.Count == 0)
            {
                return true;
            }
            else if (_currentWorker == null)
            {
                _currentWorker = _workers.Peek();
                _currentWorker.Start();
            }

            if (_currentWorker.IsFinished())
            {
                _workers.Dequeue();
                _currentWorker = null;
            }

            return false;
        }
    }
}
    