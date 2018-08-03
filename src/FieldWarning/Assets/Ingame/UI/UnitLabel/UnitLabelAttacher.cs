using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLabelAttacher : MonoBehaviour
{
    public GameObject Label;
    private Canvas _canvas;
    private Collider _collider;

    // Use this for initialization
    void Start()
    {
        _canvas = GameObject.Find("UIWrapper").GetComponent<Canvas>();
        _collider = GetComponentInChildren<Collider>();

        Label = Instantiate(Resources.Load<GameObject>("UnitLabel"), _canvas.transform);
    }

    // Update is called once per frame
    void Update()
    {
        Label.transform.position = GetScreenPosition(_canvas);
    }

    public Vector3 GetScreenPosition(Canvas canvas, Camera cam = null)
    {
        if (cam == null)
            cam = Camera.main;

        var labelPos = cam.WorldToScreenPoint(transform.position);

        labelPos.y += 40f;

        return labelPos;
    }
}
