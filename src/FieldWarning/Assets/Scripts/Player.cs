using System;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]
    private UIManagerBehaviour uiManager;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private FPSDisplay fpsDisplay;
    [SerializeField]
    private Quit quit;

    private const String ERR_START = "Player has not been set up!";

    // Use this for initialization
    void Start () {
        if (uiManager == null)
            throw new Exception(ERR_START + " uiManager missing.");
        if (mainCamera == null)
            throw new Exception(ERR_START + " mainCamera missing.");
        if (fpsDisplay == null)
            throw new Exception(ERR_START + " fpsDisplay missing.");
        if (quit == null)
            throw new Exception(ERR_START + " quit missing.");
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}
