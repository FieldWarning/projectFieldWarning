using UnityEngine;

public class DestructibleStructure : MonoBehaviour {
    public GameObject intactModel;
    public GameObject ruinsModel;

	// Use this for initialization
	void Start () {
        if (intactModel == null)
            throw new System.Exception("Structure has no undamaged model!");
        if (ruinsModel == null)
            throw new System.Exception("Structure has no damaged model!");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other) {
        intactModel.SetActive(false);
        ruinsModel.SetActive(true);
    }

    void OnParticleCollision(GameObject other) {
        Debug.Log("works");
        //intactModel.SetActive(false);
        //ruinsModel.SetActive(true);
    }
}
