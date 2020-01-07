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

using System.Collections.Generic;
using UnityEngine;

using PFW.Loading;
using PFW.Model.Armory;
using PFW.UI.Prototype;
using PFW.UI.Ingame;
using PFW.Units;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Movement;

using Mirror;
using UnityEngine.SceneManagement;

namespace PFW.Model.Game
{
    /**
     * Represents the ongoing match.
     *
     * Holds a lot of match-specific data that would be singleton or global.
     */
    public class MatchSession : MonoBehaviour
    {
        // The currently active match session. In matches (e.g. outside the lobby)
        // there should always be exactly one match session, so in practice this
        // is intended to be a singleton.
        public static MatchSession Current { get; private set; }

        private InputManager _inputManager;
        private VisibilityManager _visibilityManager;
        private UnitRegistry _unitRegistry;

        public List<UnitDispatcher> Units =>
                _unitRegistry.Units;

        public Dictionary<Team, List<UnitDispatcher>> UnitsByTeam =>
                _unitRegistry.UnitsByTeam;
        public Dictionary<Team, List<UnitDispatcher>> EnemiesByTeam =>
                _unitRegistry.EnemiesByTeam;

        public bool isChatFocused = false;

        public List<UnitDispatcher> AllyUnits =>
                _unitRegistry.AllyUnits;
        public List<UnitDispatcher> EnemyUnits =>
                _unitRegistry.EnemyUnits;

        public List<VisionComponent> AllyVisionComponents =>
                _unitRegistry.AllyVisionComponents;
        public List<VisionComponent> EnemyVisionComponents =>
                _unitRegistry.EnemyVisionComponents;

        public Settings Settings { get; private set; }

        public PlayerBehaviour LocalPlayer { get; private set; }

        public List<PlayerData> Players { get; } = new List<PlayerData>();
        public List<Team> Teams { get; } = new List<Team>();
        public ICollection<PlatoonBehaviour> Platoons { get; } = new List<PlatoonBehaviour>();

        public TerrainMap TerrainMap { get; private set; }
        public PathfinderData PathData { get; private set; }

        public UnitFactory Factory { get; private set; }

        private NetworkManager _networkManager;

        private LoadedData _loadedData;

        private void Awake()
        {
            Current = this;
            _networkManager = FindObjectOfType<NetworkManager>();

            Team blueTeam = GameObject.Find("Team_Blue").GetComponent<Team>();
            Team redTeam = GameObject.Find("Team_Red").GetComponent<Team>();

            Deck bluePlayerDeck = ConfigReader.FindDeck("player-blue");
            Deck redPlayerDeck = ConfigReader.FindDeck("player-red");

            PlayerData bluePlayer = new PlayerData(
                    bluePlayerDeck, blueTeam, (byte)Players.Count);
            Players.Add(bluePlayer);
            blueTeam.Players.Add(bluePlayer);

            PlayerData redPlayer = new PlayerData(
                    redPlayerDeck, redTeam, (byte)Players.Count);
            Players.Add(redPlayer);
            redTeam.Players.Add(redPlayer);

            Teams.Add(blueTeam);
            Teams.Add(redTeam);

            LocalPlayer = gameObject.AddComponent<PlayerBehaviour>();
            LocalPlayer.Data = redTeam.Players[0];

            _unitRegistry = new UnitRegistry(LocalPlayer.Data.Team, Teams);

            GameObject.Find("Managers").GetComponent<DeploymentMenu>().LocalPlayer = LocalPlayer;

            _inputManager = FindObjectOfType<InputManager>();
            if (!_visibilityManager)
                _inputManager = gameObject.AddComponent<InputManager>();
            _inputManager.Session = _inputManager.Session ?? this;

            _visibilityManager = FindObjectOfType<VisibilityManager>();
            if (!_visibilityManager)
                _visibilityManager = gameObject.AddComponent<VisibilityManager>();
            _visibilityManager.UnitRegistry = _visibilityManager.UnitRegistry ?? _unitRegistry;

            // LoadedData ideally comes from the loading scene
            _loadedData = FindObjectOfType<LoadedData>();

            if (_loadedData != null)
            {
                TerrainMap = _loadedData.terrainData;
                PathData = _loadedData.pathFinderData;
                Factory = new UnitFactory();
                Settings = new Settings();
            }
        }

        private void Start()
        {  
            if (_loadedData == null)
            {
                LoadingScreen.destinationScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene("loading-scene", LoadSceneMode.Single);
            } 
            else
            {
#if UNITY_EDITOR
                // Default to hosting if entering play mode directly into a match scene:
                if (!NetworkClient.isConnected)
                    _networkManager.StartHost();
#endif
            }
        }

        public void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            Platoons.Add(platoon);
            _inputManager.RegisterPlatoonBirth(platoon);
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            Platoons.Remove(platoon);
            _inputManager.RegisterPlatoonDeath(platoon);
        }

        public void RegisterUnitBirth(UnitDispatcher unit) =>
                _unitRegistry.RegisterUnitBirth(unit);

        public void RegisterUnitDeath(UnitDispatcher unit) =>
                _unitRegistry.RegisterUnitDeath(unit);

        // TODO If we can refactor MatchSession to create the spawn points, we will be able to get rid of this:
        public void RegisterSpawnPoint(SpawnPointBehaviour spawn)
        {
            _inputManager.RegisterSpawnPoint(spawn);
        }

        public void UpdateTeamBelonging(Team newTeam)
        {
            _unitRegistry.UpdateTeamBelonging(newTeam);
            _visibilityManager.UpdateTeamBelonging();
        }
    }
}
