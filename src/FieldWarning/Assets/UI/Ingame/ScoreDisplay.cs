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
using TMPro;
using UnityEngine.UI;
using PFW.UI.Ingame;
using PFW.Model.Match;

/// <summary>
/// UI element that shows the victory point score for both teams.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _redPoints = null;
    [SerializeField]
    private TextMeshProUGUI _bluePoints = null;
    [SerializeField]
    private TextMeshProUGUI _redTick = null;
    [SerializeField]
    private TextMeshProUGUI _blueTick = null;
    [SerializeField]
    private Image _redFlag = null;
    [SerializeField]
    private Image _blueFlag = null;

    private MatchSession _matchSession;
    private CaptureZone[] _zones;

    private float _redPts = 0;
    private float _bluePts = 0;
    private const int POINTS_TO_WIN = 500;

    private void Start()
    {
        // TODO - decide how we will set up the capture zones on each map
        // and how to easily find them
        _zones = FindObjectsOfType<CaptureZone>();
        _matchSession = MatchSession.Current;
    }

    private void Update()
    {
        // TODO track score in a different class and use this one for visuals only?
        int redTick = 0;
        int blueTick = 0;

        foreach (CaptureZone zone in _zones)
        {
            switch (zone.OwningTeam)
            {
                case Team.TeamName.UNDEFINED:
                    break;
                case Team.TeamName.USSR:
                    redTick += zone.Worth;
                    break;
                case Team.TeamName.NATO:
                    blueTick += zone.Worth;
                    break;
            }
        }

        _redPts += redTick * Time.deltaTime;
        _bluePts += blueTick * Time.deltaTime;

        UpdateScore((int)_redPts, redTick, (int)_bluePts, blueTick, POINTS_TO_WIN);

        if (_redPts > POINTS_TO_WIN && _redPts > _bluePts)
        {
            _matchSession.OnWinner(false);
        }
        else if (_bluePts > POINTS_TO_WIN && _redPts < _bluePts)
        {
            _matchSession.OnWinner(true);
        }
    }

    public void UpdateScore(int redPts, int redTick, int bluePts, int blueTick, int max)
    {
        float redPercentage = redPts / (float)max;
        float bluePercentage = bluePts / (float)max;

        _redFlag.fillAmount = redPercentage;
        _redPoints.text = redPts.ToString();
        _redTick.text = "+" + redTick;
        _blueFlag.fillAmount = bluePercentage;
        _bluePoints.text = bluePts.ToString();
        _blueTick.text = "+" + blueTick;
    }
}
