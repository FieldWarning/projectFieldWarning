using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SelectionManager {

    public List<PlatoonBehaviour> allUnits = new List<PlatoonBehaviour>();
    public List<PlatoonBehaviour> selection { get; private set; }

    private Vector3 mouseStart;
    private Vector3 mouseEnd;
    private Texture2D texture;
    private Texture2D borderTexture;
    public Color color = Color.red;
    public bool active;

    private ClickManager clickManager;

    public SelectionManager(int button, float mouseDragThreshold) {
        clickManager = new ClickManager(button, mouseDragThreshold, startBoxSelection, onSelectShortClick, endDrag, updateBoxSelection);
    }

    public void Update() {
        clickManager.Update();
    }

    public void changeSelectionAfterOrder() {
        if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
            unselectAll(selection);
    }

    private void startBoxSelection() {
        mouseStart = Input.mousePosition;
        active = false;
    }

    private void updateBoxSelection() {
        mouseEnd = Input.mousePosition;
        updateSelection();
        active = true;
    }

    private void endDrag() {
        active = false;
        updateSelection();
    }

    private void onSelectShortClick() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
            var go = hit.transform.gameObject;
            var selectable = go.GetComponent<SelectableBehavior>();

            if (selectable != null)
                selection.Add(selectable.getPlatoon());
        }

        setSelected(selection);
    }

    private void updateSelection() {
        List<PlatoonBehaviour> newSelection = allUnits.Where(x => isInside(x)).ToList();
        if (!Input.GetKey(KeyCode.LeftShift) && selection != null) {
            List<PlatoonBehaviour> old = selection.Except(newSelection).ToList();
            unselectAll(old);
        }
        setSelected(newSelection);
        selection = newSelection;
    }

    private bool isInside(PlatoonBehaviour obj) {
        var platoon = obj.GetComponent<PlatoonBehaviour>();
        if (!platoon.initialized)
            return false;

        bool inside = false;
        inside |= platoon.units.Any(x => isInside(x.transform.position));

        // TODO: This checks if the center of the icon is within the selection box. It should instead check if any of the four corners of the icon are within the box:
        inside |= isInside(platoon.icon.transform.GetChild(0).position);
        return inside;
    }

    private bool isInside(Vector3 t) {
        Vector3 test = Camera.main.WorldToScreenPoint(t);
        bool insideX = (test.x - mouseStart.x) * (test.x - mouseEnd.x) < 0;
        bool insideY = (test.y - mouseStart.y) * (test.y - mouseEnd.y) < 0;
        return insideX && insideY;
    }

    private void unselectAll(List<PlatoonBehaviour> l) {
        l.ForEach(x => x.setSelected(false));
        l.Clear();
    }

    private void setSelected(List<PlatoonBehaviour> l) {
        l.ForEach(x => x.setSelected(true));
    }

    // Responsible for drawing the selection rectangle
    public void OnGui() {

        if (texture == null) {
            var areaTransparency = .95f;
            var borderTransparency = .75f;
            texture = new Texture2D(1, 1);
            texture.wrapMode = TextureWrapMode.Repeat;
            var a = .95f;
            texture.SetPixel(0, 0, color - areaTransparency * Color.black);
            texture.Apply();
            borderTexture = new Texture2D(1, 1);
            borderTexture.wrapMode = TextureWrapMode.Repeat;
            borderTexture.SetPixel(0, 0, color - borderTransparency * Color.black);
            borderTexture.Apply();
        }

        if (active) {
            float lineWidth = 3;
            float startX = mouseStart.x;
            float endX = mouseEnd.x;
            float startY = Screen.height - mouseStart.y;
            float endY = Screen.height - mouseEnd.y;

            Rect leftEdge = new Rect(startX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
            Rect rightEdge = new Rect(endX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
            Rect topEdge = new Rect(startX + lineWidth / 2, startY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
            Rect bottomEdge = new Rect(startX + lineWidth / 2, endY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
            Rect area = new Rect(startX + lineWidth / 2, startY + lineWidth / 2, endX - startX - lineWidth, endY - startY - lineWidth);
            GUI.DrawTexture(area, texture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(leftEdge, borderTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(rightEdge, borderTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(topEdge, borderTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(bottomEdge, borderTexture, ScaleMode.StretchToFill, true);
        }
    }
}
