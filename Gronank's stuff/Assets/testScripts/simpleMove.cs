using UnityEngine;
using System.Collections;

public class simpleMove : MonoBehaviour {
    float speed = 40;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Time.deltaTime*speed*Vector3.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position += Time.deltaTime * speed * Vector3.back;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Time.deltaTime * speed * Vector3.left;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Time.deltaTime * speed * Vector3.right;
        }
	}
}
