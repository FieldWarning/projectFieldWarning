using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapVersion : MonoBehaviour
{
    public uint version = 1;
    public string name = "Full Feature 2 Map";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetVersionString()
    {
        return name + version;
    }
}
