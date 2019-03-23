/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using PFW.Model.Game;

public class MiniMap : MonoBehaviour, IPointerClickHandler
{
    public Terrain Terrain;
    private Camera _camera;
    //(x,y,z) X and Z are the important values
    private Vector3 _terrainSize;
    public Vector2 _minimapSize;
    public GameObject MiniMapUI;
    public GameObject GameSession;
    public Texture2D TankTexture;
    private VisibilityManager _visiMan;
    private Vector2 _screenSize;
    public Transform MiniMapCamera;
    public void Start()
    {
        _camera = MiniMapCamera.GetComponent<Camera>();
        _terrainSize = Terrain.terrainData.bounds.size;
        _camera.orthographicSize = _terrainSize.x / 2f;

        //convert camera to a texture
        RenderTexture.active = _camera.targetTexture;
        _camera.Render();
        Texture2D image = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
        image.Apply();
        MiniMapUI.GetComponent<RawImage>().texture = image;
        _camera.enabled = false;

        _visiMan = GameSession.GetComponent<MatchSession>()._visibilityManager;


    }
    //TODO different signs for different unit Types
    public void OnGUI()
    {
        //Draw all friendlies
        //Maybe there is a better way to have this list updated
        List<VisibleBehavior> allys = _visiMan.AllyUnits;
        foreach (VisibleBehavior unit in allys) {
            Vector3 pos = unit.UnitBehaviour.transform.position;
            Vector2 realPos = getMapPos(pos);
            GUI.color = _visiMan.LocalTeam.Color;
            GUI.DrawTexture(new Rect(realPos.x, realPos.y, 10, 10), TankTexture);
            GUI.color = Color.white;
        }
        //Draw all enemies
        List<VisibleBehavior> enemys = _visiMan.EnemyUnits;
        foreach (VisibleBehavior unit in enemys) {
            if (unit.isVisible) {
                Vector3 pos = unit.UnitBehaviour.transform.position;
                Vector2 realPos = getMapPos(pos);
                if (_visiMan.LocalTeam.Color == new Color(0.012f, 0.204f, 0.616f)) {
                    GUI.color = new Color(0.62f, 0f, 0f);
                } else {
                    GUI.color = new Color(0.012f, 0.204f, 0.616f);
                }

                GUI.DrawTexture(new Rect(realPos.x, realPos.y, 10, 10), TankTexture);
                GUI.color = Color.white;
            }

        }
    }
    public void LateUpdate()
    {
        //For some reason,which completely eludes me, this needs to be done in LateUpdate or otherwise it always returns just the targetet resolution

        _screenSize = new Vector2(Screen.width, Screen.height);
    }
    //TODO replace hardcoded numbers
    //Converts a position of an ingame Object to its position on the minimap
    private Vector2 getMapPos(Vector3 pos)
    {
        float scale = _screenSize.x / 1920;
        //adjust the position to fit on the terrain
        pos = pos - Terrain.GetPosition();
        //Scale the pos to fit the pixel size of the minimap
        pos = pos * (_minimapSize.x / _terrainSize.x);
        //306=width 10=offset from the border 
        pos = new Vector2(Screen.width - 306 * scale - 10 * scale + pos.x * scale, 306 * scale - pos.z * scale);

        return new Vector2(pos.x, pos.y);
    }
    //Maybe make it so that the camera doesnt move directly to the position, but instead moves so that it looks at the position
    //Move Camera to position on the minimap
    public void OnPointerClick(PointerEventData eventData)
    {
        float scale = _screenSize.x / 1920;
        Vector2 pos = eventData.position;
        pos.y = _screenSize.y - pos.y;
        pos = new Vector2(-(Screen.width  - pos.x - 10 * scale) + 306 * scale ,  pos.y - 10 * scale);
        pos = pos / scale;
        pos.y = _minimapSize.y - pos.y;
        pos = pos/ (_minimapSize.x / _terrainSize.x);
        pos = pos + new Vector2(Terrain.GetPosition().x, Terrain.GetPosition().z);
        
        Camera.main.transform.position = new Vector3(pos.x, Camera.main.transform.position.y, pos.y);
    }
}
