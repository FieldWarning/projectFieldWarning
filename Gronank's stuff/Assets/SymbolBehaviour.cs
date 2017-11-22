using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SymbolBehaviour : MonoBehaviour {
    public Material iconMaterial;
    public Material[] textures;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void setIcon(UnitType t)
    {
        int i = 0;
        switch (t)
        {
            case UnitType.Infantry:
                i = 0;
                break;
            case UnitType.Tank:
                i = 1;
                break;
            case UnitType.AFV:
                i = 2;
                break;
        }
        
        var mat = textures[i];
        GetComponent<Renderer>().material = mat;

    }
}
