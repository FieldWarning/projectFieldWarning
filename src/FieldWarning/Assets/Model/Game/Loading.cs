using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class Loading
{
    private static Mutex mut = new Mutex();
    public double percent_done = 0;
    public bool finished = false;
    public bool started = false;
    public string name;

    private List<Tuple<LoadingThreadDelegate,string>> funcs = 
        new List<Tuple<LoadingThreadDelegate, string>>();
    public Tuple<LoadingThreadDelegate, string> currentWorker = null;


    public delegate void LoadingThreadDelegate();

    public Loading(string name)
    {
        this.name = name;
        LoadingScreen.workers.Enqueue(this);
    }

    public void AddWorker(LoadingThreadDelegate func, string text = "")
    {
        funcs.Add(new Tuple<LoadingThreadDelegate,string>(func,text));
    }


    public void Load()
    {
        started = true;
        foreach (var f in funcs)
        {
            percent_done = 0;
            //TODO set current name to function
            currentWorker = f;
            currentWorker.Item1();
            percent_done = 100;
        }

        currentWorker = null;
        finished = true;

    }
}

