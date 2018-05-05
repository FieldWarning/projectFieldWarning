using UnityEngine;
using System;

public class ClickManager {
    public int Button;
    public float ClickDelay;
    public bool IsActive;
    private float LongClickTime;
    public Action OnClickStart;
    public Action OnShortClick;
    public Action OnLongClick;
    public Action OnHoldClick;

    public void Update() {
        
        if (Input.GetMouseButtonDown(Button)) {
            LongClickTime = Time.time + ClickDelay;
            IsActive = true;
            if (OnClickStart != null)
                OnClickStart();
        }

        if (LongClickTime < Time.time) {
            if (Input.GetMouseButton(Button)) {
                if (OnHoldClick != null)
                    OnHoldClick();
            } else if (Input.GetMouseButtonUp(Button)) {
                IsActive = false;
                if (OnLongClick != null)
                    OnLongClick();
                else if (OnShortClick != null)
                    OnShortClick();
            }
        } else {
            if (Input.GetMouseButtonUp(Button)) {
                IsActive = false;
                if (OnShortClick != null)
                    OnShortClick();
            }
        }
    }
}
