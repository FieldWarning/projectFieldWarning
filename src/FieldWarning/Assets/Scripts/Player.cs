using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]
    private Team team;

    // Use this for initialization
    void Start () {

    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public Team getTeam() {
        return team;
    }
}
