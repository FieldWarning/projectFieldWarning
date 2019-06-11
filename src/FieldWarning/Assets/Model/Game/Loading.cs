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
using System.Collections.Generic;
using System.Threading;

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
    private List<Tuple<LoadingThreadDelegate,string>> _funcs = 
        new List<Tuple<LoadingThreadDelegate, string>>();

    // the current function being executed
    public Tuple<LoadingThreadDelegate, string> CurrentWorker = null;

    // this is the function signature that all loader functions have to abide by
    public delegate void LoadingThreadDelegate();

    /// <summary>
    /// The loading screen has a static structure list which reads all workers it contains and
    /// executes them in separate threads
    /// </summary>
    /// <param name="name"></param>
    public Loading(string name)
    {
        this.Name = name;
        LoadingScreen.SWorkers.Enqueue(this);
    }

    public void AddWorker(LoadingThreadDelegate func, string text = "")
    {
        _funcs.Add(new Tuple<LoadingThreadDelegate,string>(func,text));
    }

    public void Load()
    {
        Started = true;

        foreach (var f in _funcs)
        {
            // each load starts with a fresh slate
            PercentDone = 0;

            // keep track of this current worker so the loading screen knows
            // whats being worked on now
            CurrentWorker = f;

            // Item1 is our worker
            CurrentWorker.Item1();
            PercentDone = 100;
        }

        CurrentWorker = null;
        Finished = true;

    }
}

