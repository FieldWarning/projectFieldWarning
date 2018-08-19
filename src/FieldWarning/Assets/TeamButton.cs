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

using PFW.Ingame.UI;
using PFW.Model.Game;

public class TeamButton : MonoBehaviour
{
    [SerializeField]
    private Team _team;
    private UIManagerBehaviour _uiManager;

    void Start()
    {
        // TODO fix this hack
        var session = GameObject.Find("GameSession");
        _uiManager = session.GetComponent<UIManagerBehaviour>();
    }

    // Update is called once per frame
    void Update() { }

    public void onClick()
    {
        _uiManager.Owner = _team.Players[0];
        VisibilityManager.updateTeamBelonging();
    }
}
