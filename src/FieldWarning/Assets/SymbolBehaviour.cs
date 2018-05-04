using UnityEngine;

public class SymbolBehaviour : MonoBehaviour {
    public Material iconMaterial;
    public Material[] textures;

	void Start () {
	
	}
	
	void Update () {
	
	}

    public void setIcon(UnitType t) {
        int i = 0;
        switch (t) {
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
