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

using PFW.Model.Game;

public class TeamButton : MonoBehaviour
{
    [SerializeField]
    private Team _team;
    [SerializeField]
    private MatchSession _session;

    // We do this instead of just setting the position in inspector because otherwise the button graphics interfere with working on the map in edit mode:
    [SerializeField]
    private int _position;

    public void Awake()
    {
        transform.position = new Vector3(
            transform.position.x,
            _position,
            transform.position.z);
    }

    public void onClick()
    {
        _session.LocalPlayer.Data = _team.Players[0];
        _session.UpdateTeamBelonging(_team);
    }
}