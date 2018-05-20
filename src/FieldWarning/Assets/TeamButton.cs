using UnityEngine;

public class TeamButton : MonoBehaviour {
    [SerializeField]
    private Player p;
    [SerializeField]
    private UIManagerBehaviour uiManager;
    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public void onClick() {
        uiManager.owner = p;
        VisibilityManager.updateTeamBelonging();
    }
}
