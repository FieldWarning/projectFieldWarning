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

namespace PFW.Ingame.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Resources;

    using PFW.Ingame.Prototype;
    using PFW.Model.Game;

    using UnityEngine;
    using UnityEngine.EventSystems;

    /**
     * Handles almost all input during a match.
     * 
     * Some input, particularly for to selecting and deselecting units,
     * is handled in SelectionManager instead.
     */
    public class InputManager : MonoBehaviour
    {
        private readonly List<SpawnPointBehaviour> _spawnPointList = new List<SpawnPointBehaviour>();

        private Vector3 _boxSelectStart;

        private BuyTransaction _currentBuyTransaction;

        private Texture2D _firePosReticle;

        private Dictionary<string, Func<Commands, object>> _hotkeyActions;

        private Texture2D _primedReticle;

        private ClickManager _rightClickManager;

        private SelectionManager _selectionManager;

        private MatchSession _session;

        public enum Events
        {
            Clicked,
            Down,
            Released
        }

        public enum Modes
        {
            Normal,
            Purchasing,
            FirePosition,
            ReverseMove,
            FastMove
        }

        public Modes MouseState { get; private set; } = Modes.Normal;

        public MatchSession Session
        {
            get => this._session;
            set => this._session = this._session ?? value;
        }

        private PlayerData LocalPlayer => this.Session.LocalPlayer.Data;

        public static bool KeyDown(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public void AddSpawnPoint(SpawnPointBehaviour monoBehavior)
        {
            if (monoBehavior is null)
                throw new ArgumentNullException(nameof(monoBehavior), "SpawnPointBehavior cannot be null or empty.");

            if (!this._spawnPointList.Contains(monoBehavior))
                this._spawnPointList.Add(monoBehavior);
        }

        public void AfvButtonCallback()
        {
            this.BuildUnit(UnitType.AFV);
            this.MouseState = Modes.Purchasing;
        }

        public void ApplyHotkeys()
        {
            if (Commands.Unload)
                this._selectionManager.DispatchUnloadCommand();

            if (Commands.Load)
                this._selectionManager.DispatchLoadCommand();

            if (Commands.FirePos && !this._selectionManager.Empty)
                this.SetCursor(Modes.FirePosition);

            if (Commands.ReverseMove && !this._selectionManager.Empty)
                this.SetCursor(Modes.ReverseMove);

            if (Commands.FastMove && !this._selectionManager.Empty)
                this.SetCursor(Modes.FastMove);
        }

        public void ArtyButtonCallback()
        {
            if (this._currentBuyTransaction is null)
                this._currentBuyTransaction = new BuyTransaction(UnitType.Arty, this.LocalPlayer);
            else
                this._currentBuyTransaction.AddUnit();
            this.MouseState = Modes.Purchasing;
        }

        public void BuildUnit(UnitType unityType)
        {
            GhostPlatoonBehaviour behaviour = GhostPlatoonBehaviour.Build(unityType, this.LocalPlayer, 4);
            this._currentBuyTransaction.GhostPlatoons.Add(behaviour);
        }

        public void InfantryButtonCallback()
        {
            this.BuildUnit(UnitType.Infantry);
            this.MouseState = Modes.Purchasing;
        }

        public void OnGui()
        {
            this._selectionManager.OnGui();
        }

        public void RegisterActor(MonoBehaviour monoBehaviour, bool birth = true)
        {
            if (monoBehaviour is null)
                throw new ArgumentNullException(nameof(monoBehaviour), "Platoon cannot be null or empty.");
            this._session.RegisterActor(monoBehaviour, birth);
        }

        public void TankButtonCallback()
        {
            if (this._currentBuyTransaction is null)
                this._currentBuyTransaction = new BuyTransaction(UnitType.Tank, this.LocalPlayer);
            else
                this._currentBuyTransaction.AddUnit();

            this.MouseState = Modes.Purchasing;
        }

        private static void DisplayOrderFeedback()
        {
            if (Util.GetTerrainClickLocation(out RaycastHit hit))
                Instantiate(
                    Resources.Load("MoveMarker", typeof(GameObject)),
                    hit.point + new Vector3(0f, 0.01f, 0f),
                    Quaternion.Euler(new Vector3(90f, 0f, 0f)));
        }

        private void Awake()
        {
            this._selectionManager = new SelectionManager();
            this._selectionManager.Awake();
        }

        private void ExitPurchasingMode()
        {
            this._currentBuyTransaction.GhostPlatoons.Clear();
            this._currentBuyTransaction = null;
            this.MouseState = Modes.Normal;
        }

        private SpawnPointBehaviour GetClosestSpawns(Vector3 position)
        {
            var spawnPoints = this._spawnPointList.Where(x => x.Team.Equals(this.LocalPlayer.Team)).ToList();

            SpawnPointBehaviour firstSpawn = spawnPoints.First();
            var maxDistance = float.PositiveInfinity;

            for (var i = 0; i < spawnPoints.Count; i++)
            {
                if (Vector3.Distance(position, spawnPoints[i].transform.position) > maxDistance) continue;
                maxDistance = Vector3.Distance(position, spawnPoints[i].transform.position);
                firstSpawn = spawnPoints[i];
            }

            return firstSpawn;
        }

        private bool IsMouse(Events events, int button = 0)
        {
            return this.IsMouse(events, button);
        }

        private void MaybeExitPurchasingMode()
        {
            if (!this.IsMouse(Events.Clicked))
                return;
            this._currentBuyTransaction.GhostPlatoons.ForEach(
                ghost => ghost.GetComponent<GhostPlatoonBehaviour>().Destroy());
            this.ExitPurchasingMode();
        }

        private void MaybePurchaseGhostUnits(SpawnPointBehaviour closestSpawn)
        {
            if (closestSpawn is null || this.IsMouse(Events.Released)
                                     || EventSystem.current.currentSelectedGameObject is null
                                     || this._currentBuyTransaction is null)
                return;

            closestSpawn.BuyPlatoons(this._currentBuyTransaction.GhostPlatoons);

            if (KeyDown(KeyCode.LeftShift))
                this._currentBuyTransaction = this._currentBuyTransaction.Clone();
            else
                this.ExitPurchasingMode();
        }

        private void MoveGhostsToMouse()
        {
            if (Util.GetTerrainClickLocation(out RaycastHit hit))
                this._selectionManager.PrepareMoveOrderPreview(hit.point);
        }

        private void OnOrderHold()
        {
            if (Util.GetTerrainClickLocation(out RaycastHit hit))
                this._selectionManager.RotateMoveOrderPreview(hit.point);
        }

        private void SetCursor(Modes modes)
        {
            this.MouseState = modes;
            Vector2 nextMouseLocation = Vector2.zero;
            Texture2D nextMouseTexture = Texture2D.whiteTexture;
            switch (modes)
            {
                case Modes.FirePosition:
                    nextMouseLocation = new Vector2(this._firePosReticle.width / 2, this._firePosReticle.height / 2);
                    break;
                case Modes.FastMove:
                    nextMouseTexture = this._primedReticle;
                    break;
                case Modes.Normal:
                    nextMouseTexture = null;
                    break;
                case Modes.ReverseMove:
                    nextMouseTexture = this._primedReticle;
                    break;
                case Modes.Purchasing: break;
                default: throw new ArgumentOutOfRangeException(nameof(modes), modes, null);
            }

            Cursor.SetCursor(nextMouseTexture, nextMouseLocation, CursorMode.Auto);
        }

        private void ShowGhostUnitsAndMaybePurchase(RaycastHit terrainHover)
        {
            SpawnPointBehaviour closestSpawn = this.GetClosestSpawns(terrainHover.point);
            this._currentBuyTransaction.PreviewPurchase(
                terrainHover.point,
                terrainHover.point * 2 - closestSpawn.transform.position);

            this.MaybePurchaseGhostUnits(closestSpawn);
        }

        private void Start()
        {
            this._firePosReticle = (Texture2D)Resources.Load("FirePosTestTexture");
            if (this._firePosReticle is null)
                throw new MissingManifestResourceException("Could not locate Texture2D named 'FirePosTestTexture'.");

            this._primedReticle = (Texture2D)Resources.Load("PrimedCursor");
            if (this._primedReticle is null)
                throw new MissingManifestResourceException("Could not locate Texture2D named `PrimedCursor`.");

            this._rightClickManager = new ClickManager(
                1,
                this.MoveGhostsToMouse,
                this.OnOrderHold,
                this.ExitPurchasingMode,
                this.MoveGhostsToMouse);
        }

        private void Update()
        {
            this._selectionManager.Update(this.MouseState);

            switch (this.MouseState)
            {
                case Modes.Purchasing:
                    if (Util.GetTerrainClickLocation(out RaycastHit hit)
                        && hit.transform.gameObject.name.Equals("Terrain"))
                        this.ShowGhostUnitsAndMaybePurchase(hit);
                    this.MaybeExitPurchasingMode();
                    break;

                case Modes.Normal:
                    this._rightClickManager.Update();
                    break;

                case Modes.FirePosition:
                    if (this.IsMouse(Events.Down))
                        this._selectionManager.DispatchFirePosCommand();
                    if (this.IsMouse(Events.Down) && !KeyDown(KeyCode.LeftShift) || this.IsMouse(Events.Down, 1))
                        this.SetCursor(Modes.Normal);
                    break;

                case Modes.ReverseMove:
                    if (!this.IsMouse(Events.Down))
                        break;

                    this.MoveGhostsToMouse();
                    this._selectionManager.DispatchMoveCommand(false, MoveWaypoint.MoveMode.Reverse);

                    if (this.IsMouse(Events.Down) && !KeyDown(KeyCode.LeftShift) || this.IsMouse(Events.Down))
                        this.SetCursor(Modes.Normal);
                    break;

                case Modes.FastMove:
                    if (!this.IsMouse(Events.Down))
                        break;

                    this.MoveGhostsToMouse();
                    this._selectionManager.DispatchMoveCommand(false, MoveWaypoint.MoveMode.Fast);

                    if (this.IsMouse(Events.Down) && !KeyDown(KeyCode.LeftShift) || this.IsMouse(Events.Down, 1))
                        this.SetCursor(Modes.Normal);
                    break;
                default:
                    this.ApplyHotkeys();
                    break;
            }
        }
    }

    public class Commands
    {
        public static bool FastMove => Input.GetKeyDown(Hotkeys.FastMove);

        public static bool FirePos => Input.GetKeyDown(Hotkeys.FirePos);

        public static bool Load => Input.GetKeyDown(Hotkeys.Load);

        public static bool ReverseMove => Input.GetKeyDown(Hotkeys.ReverseMove);

        public static bool Unload => Input.GetKeyDown(Hotkeys.Unload);
    }
}
