using UnityEngine;
using System.Collections;

public class TeamButton : MonoBehaviour {
    public Team team;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void onClick()
    {

        UIManagerBehaviour.currentTeam = team;
        VisibilityManager.updateTeamBelonging();
    }
}
