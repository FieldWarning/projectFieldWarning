using UnityEngine;

public class Quit : MonoBehaviour {

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}
