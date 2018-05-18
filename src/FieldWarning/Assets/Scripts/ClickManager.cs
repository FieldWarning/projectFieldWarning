using UnityEngine;
using System;

public class ClickManager {
    public bool isActive;

    private int button;
    private float shortestLongClick;
    private float LongClickTime;
    private Action onClickStart;
    private Action onShortClick;
    private Action onLongClick;
    private Action onHoldClick;

    public ClickManager(int button,  float clickDelay, Action onClickStart, Action onShortClick, Action onLongClick, Action onHoldClick) {
        this.button = button;
        this.shortestLongClick = clickDelay;
        this.onClickStart = onClickStart;
        this.onShortClick = onShortClick;
        this.onLongClick = onLongClick;
        this.onHoldClick = onHoldClick;
    }

    public void Update() {

        if (Input.GetMouseButtonDown(button)) {
            LongClickTime = Time.time + shortestLongClick;
            isActive = true;
            if (onClickStart != null)
                onClickStart();
        } 

        if (isActive && isLongClick()) {
            if (Input.GetMouseButton(button)) {
                if (onHoldClick != null)
                    onHoldClick();
            } else if (Input.GetMouseButtonUp(button)) {
                isActive = false;
                if (onLongClick != null)
                    onLongClick();
                else if (onShortClick != null)
                    onShortClick();
            }
        } else {
            if (Input.GetMouseButtonUp(button)) {
                isActive = false;
                if (onShortClick != null)
                    onShortClick();
            }
        }
    }

    private bool isLongClick() {
        return LongClickTime < Time.time;
    }
}
