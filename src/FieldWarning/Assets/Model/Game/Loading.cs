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
using UnityEngine;



/*
/// <summary>
/// Handles which order to load / execute the various specified functions.
/// </summary>
public class Loading
{
    public double PercentDone = 0;
    public bool Finished = false;
    public bool Started = false;
    public string Name;

   



    // the list of functions along with their descriptions
    private List<Worker> _workerFuncs = new List<Worker>();

    // the current function being executed
    public Worker CurrentWorker = null;

    // this is the function signature that all loader functions have to abide by
    public delegate IEnumerator LoadingCoroutineDelegate();

    /// <summary>
    /// The loading screen has a static structure list which reads all workers it contains and
    /// executes them in separate threads
    /// </summary>
    /// <param name="name"></param>
    public Loading(string name)
    {
        this.Name = name;
        //LoadingScreen.SWorkers.Enqueue(this);
    }

    public void AddMultiThreadedWorker(ParameterizedThreadStart func, string text)
    {
        _workerFuncs.Add(new MultithreadedWorker(func, text));
    }

    public void AddCoroutineWorker(LoadingCoroutineDelegate func, string text)
    {
        _workerFuncs.Add(new CoroutineWorker(func,text));
    }

    public void Start()
    {
        Started = true;

        foreach (var wf in _workerFuncs)
        {
            // each load starts with a fresh slate
            PercentDone = 0;

            wf.Run();

            PercentDone = 100;
        }

        CurrentWorker = null;
        Finished = true;

    }
}

    */