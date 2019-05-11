﻿/**
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
using PFW.Units.Component.Vision;
using PFW.Units;

public class MiniMap : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Terrain _terrain = null;
    //(x,y,z) X and Z are the important values
    private Vector3 _terrainSize;
    private float _minimapSize;
    private float _offsetFromRightSide;
    private float _targetedScreenSize;
    private Vector2 _screenSize;

    [SerializeField]
    private RawImage _miniMapImage = null;
    [SerializeField]
    private Texture2D _tankTexture = null;
    [SerializeField]
    private MatchSession _matchSession = null;
    [SerializeField]
    private Camera _miniMapCamera = null;
    [SerializeField]
    private SlidingCameraBehaviour _mainCamera = null;

    private void Start()
    {
        _minimapSize = gameObject.GetComponent<RectTransform>().rect.width;
        _offsetFromRightSide =
            (-1) * (transform.parent.GetComponent<RectTransform>().rect.width / 2
            + transform.parent.GetComponent<RectTransform>().anchoredPosition.x);
        _targetedScreenSize =
            transform.parent.parent.GetComponent<RectTransform>().rect.width;

        _terrainSize = _terrain.terrainData.bounds.size;
        _miniMapCamera.orthographicSize = _terrainSize.x / 2f;

        //convert camera to a texture
        RenderTexture.active = _miniMapCamera.targetTexture;
        _miniMapCamera.Render();
        Texture2D image = new Texture2D(_miniMapCamera.targetTexture.width, _miniMapCamera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, _miniMapCamera.targetTexture.width, _miniMapCamera.targetTexture.height), 0, 0);
        image.Apply();
        _miniMapImage.texture = image;
        _miniMapCamera.enabled = false;
    }

    //TODO different signs for different unit Types
    private void OnGUI()
    {
        //Draw all friendlies
        //Maybe there is a better way to have this list updated
        List<UnitDispatcher> allies = _matchSession.AllyUnits;
        foreach (UnitDispatcher unit in allies) {
            Vector3 pos = unit.Transform.position;
            Vector2 realPos = GetMapPos(pos);
            GUI.color = _matchSession.LocalPlayer.Data.Team.Color;
            GUI.DrawTexture(new Rect(realPos.x, realPos.y, 10, 10), _tankTexture);
            GUI.color = Color.white;
        }

        //Draw all enemies
        List<UnitDispatcher> enemies = _matchSession.EnemyUnits;
        foreach (UnitDispatcher unit in enemies) {
            if (unit.IsVisible) {
                Vector3 pos = unit.Transform.position;
                Vector2 realPos = GetMapPos(pos);
                if (_matchSession.LocalPlayer.Data.Team == _matchSession.Teams[0]) {
                    GUI.color = _matchSession.Teams[1].Color;
                } else {
                    GUI.color = _matchSession.Teams[0].Color;
                }

                GUI.DrawTexture(new Rect(realPos.x, realPos.y, 10, 10), _tankTexture);
                GUI.color = Color.white;
            }
        }
    }

    private void LateUpdate()
    {
        //For some reason,which completely eludes me, this needs to be done in LateUpdate or otherwise it always returns just the targeted resolution
        _screenSize = new Vector2(Screen.width, Screen.height);
    }

    //Converts a position of an ingame Object to its position on the minimap
    private Vector2 GetMapPos(Vector3 pos)
    {
        float scale = _screenSize.x / _targetedScreenSize;
        //adjust the position to fit on the terrain
        pos = pos - _terrain.GetPosition();
        //Scale the pos to fit the pixel size of the minimap
        pos = pos * (_minimapSize / _terrainSize.x);
        pos = new Vector2(Screen.width - _minimapSize * scale - _offsetFromRightSide * scale + pos.x * scale, _minimapSize * scale - pos.z * scale);

        return new Vector2(pos.x, pos.y);
    }

    //Maybe make it so that the camera doesnt move directly to the position, but instead moves so that it looks at the position
    //Move Camera to position on the minimap, basicly a reverse calculation of GetMapPos
    public void OnPointerClick(PointerEventData eventData)
    {
        float scale = _screenSize.x / _targetedScreenSize;
        Vector2 pos = eventData.position;
        pos.y = _screenSize.y - pos.y;
        pos = new Vector2(-(Screen.width - pos.x - _offsetFromRightSide * scale) + _minimapSize * scale, pos.y - _offsetFromRightSide * scale);
        pos = pos / scale;
        pos.y = _minimapSize - pos.y;
        pos = pos / (_minimapSize / _terrainSize.x);
        pos = pos + new Vector2(_terrain.GetPosition().x, _terrain.GetPosition().z);

        _mainCamera.SetTargetPosition(
                new Vector3(pos.x, _mainCamera.transform.position.y, pos.y));
    }
}
