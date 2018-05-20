using UnityEngine;
using System;

public class ClickManager {
    private int button;

    private float dragThreshold;
    private Vector3 lastMousePosition;
    private float mouseDistanceTravelled = 0;

    private Action onMouseDown;
    private Action nonDragMouseRelease;
    private Action dragMouseRelease;
    private Action whileDraggingMouse;

    public ClickManager(int button,  float dragThreshold, Action onMouseDown, Action nonDragMouseRelease, Action dragMouseRelease, Action whileDraggingMouse) {

        if (onMouseDown == null || nonDragMouseRelease == null || dragMouseRelease == null || whileDraggingMouse == null)
            throw new Exception("ClickManager initialized with null action!");

        this.button = button;
        this.dragThreshold = dragThreshold;
        this.onMouseDown = onMouseDown;
        this.nonDragMouseRelease = nonDragMouseRelease;
        this.dragMouseRelease = dragMouseRelease;
        this.whileDraggingMouse = whileDraggingMouse;
    }

    public void Update() {
        if (Input.GetMouseButtonDown(button)) {
            mouseDistanceTravelled = 0;
            lastMousePosition = Input.mousePosition;
            onMouseDown();
        }

        if (Input.GetMouseButton(button) && !isDragClick()) {
            mouseDistanceTravelled += Vector3.Distance(Input.mousePosition, lastMousePosition);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(button) && isDragClick()) 
            whileDraggingMouse();


        if (Input.GetMouseButtonUp(button))
            if (isDragClick())
                dragMouseRelease();
            else
                nonDragMouseRelease();        
    }

    private bool isDragClick() {
        return mouseDistanceTravelled > dragThreshold;
    }
}
