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
using PFW.Networking;

namespace PFW.Model.Match
{
    /// <summary>
    /// Represents the ongoing match.
    /// 
    /// Holds a lot of match-specific data that would be singleton or global.
    /// </summary>
    /// Contrast: GameSession, which represents the ongoing application session.
    public class MatchSession : MonoBehaviour
    {
        // The currently active match session. In matches (e.g. outside the lobby)
        // there should always be exactly one match session, so in practice this
        // is intended to be a singleton.
        public static MatchSession Current { get; private set; }

        [SerializeField]
        private InputManager _inputManager = null;
        private VisibilityManager _visibilityManager;
        private UnitRegistry _unitRegistry;
        [SerializeField]
        private DeploymentMenu _deploymentMenu = null;

        public List<UnitDispatcher> Units =>
                _unitRegistry.Units;

        public Dictionary<Team, List<UnitDispatcher>> UnitsByTeam =>
                _unitRegistry.UnitsByTeam;
        public Dictionary<Team, List<UnitDispatcher>> EnemiesByTeam =>
                _unitRegistry.EnemiesByTeam;

        public bool IsChatFocused => _inputManager.IsChatOpen;

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
        public VisionCache VisionCache { get; private set; }
        public PathfinderData PathData { get; private set; }

        public UnitFactory Factory { get; private set; }

        private NetworkManager _networkManager;

        private LoadedData _loadedData;

        public SpawnPointBehaviour[] SpawnPoints;

        private void Awake()
        {
            Current = this;
            _networkManager = FindObjectOfType<NetworkManager>();

            // LoadedData ideally comes from the loading scene
            _loadedData = FindObjectOfType<LoadedData>();

            // If there is no loaded data, this scene is just
            // a false start and we will instantly move to
            // the loading scene (see Start() ) and then reset this
            // scene with a loaded data.
            if (_loadedData != null)
            {
                TerrainMap = _loadedData.TerrainData;
                VisionCache = new VisionCache(TerrainMap);
                PathData = _loadedData.PathFinderData;
                Factory = new UnitFactory();
                Settings = new Settings();


                Team blueTeam = GameObject.Find("Team_Blue").GetComponent<Team>();
                Team redTeam = GameObject.Find("Team_Red").GetComponent<Team>();

                Deck bluePlayerDeck = GameSession.Singleton.Decks["player-blue"];
                Deck redPlayerDeck = GameSession.Singleton.Decks["player-red"];

                PlayerData bluePlayer = new PlayerData(
                        bluePlayerDeck, blueTeam, "Reagan", (byte)Players.Count);
                Players.Add(bluePlayer);
                blueTeam.Players.Add(bluePlayer);

                PlayerData redPlayer = new PlayerData(
                        redPlayerDeck, redTeam, "Gorbachev", (byte)Players.Count);
                Players.Add(redPlayer);
                redTeam.Players.Add(redPlayer);

                Teams.Add(blueTeam);
                Teams.Add(redTeam);

                LocalPlayer = gameObject.AddComponent<PlayerBehaviour>();
                LocalPlayer.Data = redTeam.Players[0];

                _unitRegistry = new UnitRegistry(LocalPlayer.Data.Team, Teams);

                _inputManager.LocalPlayer = LocalPlayer.Data;

                _visibilityManager = FindObjectOfType<VisibilityManager>();
                if (!_visibilityManager)
                    _visibilityManager = gameObject.AddComponent<VisibilityManager>();
                _visibilityManager.UnitRegistry = _visibilityManager.UnitRegistry ?? _unitRegistry;

                _deploymentMenu.Initialize(_inputManager, LocalPlayer);

                SpawnPoints = FindObjectsOfType<SpawnPointBehaviour>();
                for (int i = 0; i < SpawnPoints.Length; i++)
                {
                    SpawnPoints[i].Id = (byte)i;
                }
            }
        }

        private void Start()
        {
            if (_loadedData == null)
            {
                LoadingScreen.SceneBuildId = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene("loading-scene", LoadSceneMode.Single);
            }
            else
            {
#if UNITY_EDITOR
                // Default to hosting if entering play mode directly into a match scene:
                if (!NetworkClient.isConnected)
                    _networkManager.StartHost();
#endif

                // Mirror requires us to register prefabs that will be spawned on the network.
                // Normally this is done by adding them to the network manager prefab,
                // but that list can get wiped when upgrading mirror, so it's
                // more maintainable to do it here:
                _networkManager.spawnPrefabs.Add(Resources.Load<GameObject>("Flare"));
                _networkManager.spawnPrefabs.Add(Resources.Load<GameObject>("Platoon"));
                _networkManager.spawnPrefabs.Add(Resources.Load<GameObject>("GhostPlatoon"));
                _networkManager.spawnPrefabs.Add(Resources.Load<GameObject>("UnitTemplatePrefabs/GroundUnit"));

                // Ready to play:
                _inputManager.enabled = true;
            }
        }

        /// <summary>
        /// Called when one team wins.
        /// </summary>
        public void OnWinner(bool winnerIsBlue)
        {
            // TODO show score and go to main menu
            Logger.LogWithoutSubsystem(
                LogLevel.WARNING,
                "MATCH ENDED, WINNER = " + (winnerIsBlue ? "blue" : "red") + " team.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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

        /// <summary>
        /// Change player + team when the team button is pressed.
        /// This should not happen in real games, it's just a
        /// development aid.
        /// </summary>
        public void RegisterPlayerChange(PlayerData newPlayer)
        {
            LocalPlayer.Data = newPlayer;
            _inputManager.LocalPlayer = newPlayer;
            Team newTeam = LocalPlayer.Data.Team;
            _unitRegistry.UpdateTeamBelonging(newTeam);
            _visibilityManager.UpdateTeamBelonging();
            _deploymentMenu.UpdateTeamBelonging();
            CommandConnection.Connection.CmdChangeTeam(newTeam.Name);
        }

        /// <summary>
        /// Change player + team when the team button is pressed.
        /// This should not happen in real games, it's just a
        /// development aid.
        /// </summary>
        public void RegisterDeckChange(PlayerData player, string newDeck)
        {
            bool exists = GameSession.Singleton.Decks.TryGetValue(newDeck, out Deck deck);
            if (!exists)
            {
                Logger.LogLoading(
                        LogLevel.ERROR, 
                        $"Player {player.Id} tried to change " +
                        $"to deck = '{newDeck}', but such a deck does not exist.");
                return;
            }

            player.Deck = deck;
            _deploymentMenu.UpdateTeamBelonging();
            // TODO sync across network
        }

        /// <summary>
        ///     Inform the selection manager that a platoon label was 
        ///     clicked so that the selection can be updated.
        /// </summary>
        /// Passing this info this way is kind of ugly, but 
        /// arguably better than exposing the selection manager just
        /// for this call?
        public void PlatoonLabelClicked(PlatoonBehaviour platoon) =>
            _inputManager.PlatoonLabelClicked(platoon);


        public List<UnitDispatcher> FindUnitsAroundPoint(
                Vector3 point, float radius) =>
            _unitRegistry.FindUnitsAroundPoint(point, radius);
        public List<UnitDispatcher> FindEnemiesAroundPoint(
                Vector3 point, float radius) =>
            _unitRegistry.FindEnemiesAroundPoint(point, radius);
        public List<UnitDispatcher> FindAlliesAroundPoint(
                Vector3 point, float radius) =>
            _unitRegistry.FindAlliesAroundPoint(point, radius);
    }
}
