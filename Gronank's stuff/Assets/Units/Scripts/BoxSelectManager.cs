using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoxSelectManager{

    public List<PlatoonBehaviour> allUnits = new List<PlatoonBehaviour>();
    private Vector3 mouseStart;
    private Vector3 mouseEnd;
    private Texture2D texture;
    private Texture2D borderTexture;
    private List<PlatoonBehaviour> selection;
    public Color color=Color.red;
    public bool active;
    public void startDrag()
    {
        mouseStart = Input.mousePosition;
        active = false;
    }
    public void updateDrag()
    {

        mouseEnd = Input.mousePosition;
        updateSelection();
        active = true;
    }
    public List<PlatoonBehaviour> endDrag()
    {
        active = false;
        updateSelection();
        return selection;

    }
    private void updateSelection()
    {
        List<PlatoonBehaviour> newSelection = allUnits.Where(x => isInside(x)).ToList();
        if (selection != null)
        {
            List<PlatoonBehaviour> old = selection.Except(newSelection).ToList();
            old.endSelection();
        }
        newSelection.update();
        selection = newSelection;
    }
    private bool isInside(PlatoonBehaviour obj){
        
        bool inside = false;
        if (!obj.GetComponent<PlatoonBehaviour>().initialized) return false;
        inside |= obj.GetComponent<PlatoonBehaviour>().units.Any(x => isInside(x.transform.position));
        inside |= isInside(obj.GetComponent<PlatoonBehaviour>().icon.transform.GetChild(0).position);
        return inside;
    }
    private bool isInside(Vector3 t)
    {
        Vector3 test = Camera.main.WorldToScreenPoint(t);
        bool insideX = (test.x - mouseStart.x) * (test.x - mouseEnd.x) < 0;
        bool insideY = (test.y - mouseStart.y) * (test.y - mouseEnd.y) < 0;
        return insideX && insideY;
    }
    public void OnGui(){

        if (texture == null)
        {
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
        if (active)
        {
            
            float lineWidth = 3;
            float startX;
            float endX;
            if(mouseStart.x<mouseEnd.x){
            startX=mouseStart.x;
            endX = mouseEnd.x;
            }
            else
            {
                startX = mouseEnd.x;
                endX = mouseStart.x;
            }
            float startY;
            float endY;
            if (mouseStart.y < mouseEnd.y)
            {
                startY = Screen.height - mouseStart.y;
                endY = Screen.height - mouseEnd.y;
            }
            else
            {
                startY = Screen.height - mouseEnd.y;
                endY = Screen.height - mouseStart.y;
            }

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
