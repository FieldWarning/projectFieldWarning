using System.Collections;
using System.Threading;
using UnityEngine;

public delegate IEnumerator WorkerCoroutineDelegate();
public delegate void WorkerThreadDelegate();
public delegate void WorkerConcurrentDelegate();

public abstract class Worker
{
    public double PercentDone = 0;
    public bool Finished = false;
    public bool Started = false;
    public string description;
    
    public Worker(string desc)
    {
        
        description = desc;
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void Begun()
    {
        Started = true;
        PercentDone = 0;
    }

    protected void Ended()
    {
        PercentDone = 100;
        Finished = true;
    }

    public abstract void Run();
}

public class CoroutineWorker : Worker
{
    private WorkerCoroutineDelegate function;

    public CoroutineWorker(WorkerCoroutineDelegate func, string desc) :base(desc)
    {
        this.function = func;
    }

    public override void Run()
    {
        var runner = GameObject.FindObjectOfType<CoroutineRunner>();
        runner.StartCoroutine(coroutine());
        
    }

    public IEnumerator coroutine()
    {
        Begun();
        yield return function();
        Ended();
    }
}

public class MultithreadedWorker : Worker
{
    private WorkerThreadDelegate function;

    public MultithreadedWorker(WorkerThreadDelegate func, string desc):base(desc)
    {
        this.function = func;
    }

    public override void Run()
    {
        Thread _thread = new Thread(thread);
        _thread.Start();
    }

    private void thread(object obj)
    {
        Begun();
        function();
        Ended();
    }

}







