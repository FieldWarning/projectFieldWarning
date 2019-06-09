using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{

    private Slider slider;
    private Text descLbl;

    static public Queue<Loading> workers = new Queue<Loading>();
    private Thread _thread;




    // Start is called before the first frame update
    void Start()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();
        descLbl = transform.Find("LoadingLbl").GetComponent<Text>();


    }


    void Update()
    {
        if (workers.Count > 0) 
        {
            var current = workers.Peek();
            if (! current.started)
            {
                _thread = new Thread(current.Load);
                _thread.Start();
                
            }
            descLbl.text = current.name;
            if (current.currentWorker != null)
            {
                descLbl.text = current.name + "\t" + current.currentWorker.Item2;
            }

            slider.value = (float)current.percent_done;

            if (current.finished)
            {
                workers.Dequeue();
                slider.value = slider.maxValue;
            }
        } else
        {
            gameObject.SetActive(false);
        }


    }
}
