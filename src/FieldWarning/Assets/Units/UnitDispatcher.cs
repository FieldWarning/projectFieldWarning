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
using Mirror;
using System.Collections.Generic;

using PFW.Units.Component.Data;
using PFW.Units.Component.Death;
using PFW.Units.Component.Weapon;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Health;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Armor;
using PFW.Model.Match;
using PFW.UI.Ingame.UnitLabel;

namespace PFW.Units
{
    /// <summary>
    /// The dispatcher represents the unit to the outside world
    /// while delegating all tasks to the various unit components.
    /// </summary>
    public sealed class UnitDispatcher : NetworkBehaviour
    {
        // Handles move orders:
        // private INavigationComponent _navigationComponent;

        // Contains weapon components which contain audio components etc:
        private TurretSystem _turretSystem;
        public List<Cannon> AllWeapons => _turretSystem.AllWeapons;

        private HealthComponent _healthComponent;

        private ArmorComponent _armorComponent;

        #region Potential future components
        //private IReconComponent _reconComponent;
        //private IStealthComponent _stealthComponent;
        //private IPlatoonComponent _platoonComponent; // Maybe contains the ILabelComponent and syncs with the platoon components of other units such that only one is visible if they are grouped
        //private ILabelComponent _labelComponent; // Maybe not directly contained, see above..
        //private ITransportableComponent _transportableComponent;
        //private ITransporterComponent _transporterComponent;
        //private ISupplyConsumptionComponent _supplyConsumptionComponent;
        //private ISupplyProvisionComponnet _supplyProvisionComponent;
        //private IZoneCaptureComponent _zoneCaptureComponent;
        //private IMoraleComponent _moraleComponent;
        // more
        #endregion

        private MovementComponent _movementComponent;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        /// <summary>
        /// The target tuple for targeting this unit. Do NOT manually
        /// create additional target tuples.
        /// </summary>
        public TargetTuple TargetTuple { get; private set; }

        // TODO: This is only held by this class as a way to get it to 
        //       VisibilityManager. Figure out the best way to do that.
        public VisionComponent VisionComponent;

        private DataComponent _unitData;
        private VoiceComponent _voiceComponent;

        // TODO move to a component class:
        private GameObject _selectionCircle;

        public PlatoonBehaviour Platoon { get; set; }

        private TargetingOverlay _targetingOverlay;

        private GameObject _art;
        private WreckComponent _deathEffect = null;

        public void Initialize(
                PlatoonBehaviour platoon, 
                GameObject art,
                GameObject deathEffect, 
                VoiceComponent voice)
        {
            _unitData = gameObject.GetComponent<DataComponent>();

            TargetType type = _unitData.ApImmunity ? TargetType.INFANTRY : TargetType.VEHICLE;
            TargetTuple = new TargetTuple(this, type);
            Platoon = platoon;

            _art = art;
            _deathEffect = deathEffect?.GetComponent<WreckComponent>();

            _voiceComponent      = voice;
            _movementComponent   = gameObject.GetComponent<MovementComponent>();
            _healthComponent     = gameObject.GetComponent<HealthComponent>();
            _armorComponent      = gameObject.GetComponent<ArmorComponent>();
            VisionComponent      = gameObject.GetComponent<VisionComponent>();
            _turretSystem        = gameObject.GetComponent<TurretSystem>();

            // Only used in this class, not really configurable,
            // and no way to get a reference to it here if it's
            // instantiated in the UnitFitter.
            // I think it's fine to leave it here.
            _selectionCircle = Instantiate(
                    Resources.Load<GameObject>("SelectionCircle"), Transform);

            _movementComponent.Initialize();

            _healthComponent.Initialize(this, _unitData);
            VisionComponent.Initialize(this, _unitData);
            _armorComponent.Initialize(_healthComponent, _unitData, _movementComponent);

            _targetingOverlay = OverlayFactory.Instance.CreateTargetingOverlay(this);
            _targetingOverlay.gameObject.transform.parent = gameObject.transform;
        }

        /// <summary>
        /// Initialization order: Awake() when a gameobject is created,
        /// WakeUp() enables the object, Start() runs on an enabled object.
        /// </summary>
        public void WakeUp()
        {
            _movementComponent.enabled = true;
            VisionComponent.ToggleUnitVisibility(true);

            MatchSession.Current.RegisterUnitBirth(this);
        }

        public void CancelOrders()
        {
            _movementComponent.CancelOrders();
            _turretSystem.CancelOrders();
        }

        public void SendFirePosOrder(Vector3 position)
        {
            _turretSystem.TargetPosition(position);
        }

        public bool HasTargetingOrder => _turretSystem.HasTargetingOrder;

        /// <summary>
        /// Called when a unit enters or leaves the player's selection.
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="justPreviewing">
        /// True when the unit should be shaded as if selected,
        /// but the actual selected set has not been changed yet.
        /// </param>
        public void SetSelected(bool selected, bool justPreviewing)
        {
            _selectionCircle.SetActive(selected && !justPreviewing);
        }

        #region PlayVoicelines
        public void PlayAttackCommandVoiceline() =>
                _voiceComponent.PlayAttackCommandVoiceline();
        public void PlayMoveCommandVoiceline() =>
                _voiceComponent.PlayMoveCommandVoiceline();
        public void PlaySelectionVoiceline() =>
                _voiceComponent.PlaySelectionVoiceline();
        #endregion

        public float GetHealth() => _healthComponent.Health;
        public float MaxHealth => _unitData.MaxHealth;

        public float EstimateDamage(
                DamageType damageType,
                float firepower,
                Vector3 displacement,
                float distance)
            =>
            _armorComponent.EstimateDamage(damageType, firepower, displacement, distance);

        public void HandleHit(
            DamageType damageType,
            float firepower,
            Vector3 displacementToTarget,
            float distance)
            =>
            _armorComponent.HandleHit(damageType, firepower, displacementToTarget, distance);

        public void Teleport(Vector3 position, float heading) =>
                _movementComponent.Teleport(position, heading);

        public bool AreOrdersComplete() => _movementComponent.AreOrdersComplete();
        public void SetDestination(
                Vector3 pos, 
                float heading = MovementComponent.NO_HEADING, 
                MoveCommandType moveMode = MoveCommandType.FAST) =>
                        _movementComponent.SetDestination(pos, heading, moveMode);

        public bool IsVisible => VisionComponent.IsVisible;

        /// <summary>
        /// Destroy this unit.
        /// </summary>
        [Server]
        public void Destroy()
        {
            NetworkServer.Destroy(gameObject);
        }

        public override void OnStopServer()
        {
            if (!isClient)
            {
                OnDestroyShared();
            }
            else
            {
                // Do nothing, since OnStopClient() will also be called.
            }
        }

        public override void OnStopClient() => OnDestroyShared();

        private void OnDestroyShared()
        {
            // Model lingers as a wreck for 30s
            if (_deathEffect != null)
            {
                _deathEffect.Activate(_art);
            }

            TargetTuple.Reset();

            MatchSession.Current.RegisterUnitDeath(this);

            Platoon.RemoveUnit(this);
        }

        public int PlaceTargetingPreview(Vector3 targetPosition, bool respectMaxRange)
        {
            return respectMaxRange ? 
                    _targetingOverlay.PlaceTargetingPreview(targetPosition) : 
                    _targetingOverlay.PlaceVisionPreview(targetPosition);
        }

        public void ToggleTargetingPreview(bool enabled)
        {
            _targetingOverlay.gameObject.SetActive(enabled);
        }

        public float MaxFirePosRange() => _turretSystem.MaxFirePosRange();
        public float MaxFirePosRangeIndirectFire() => _turretSystem.MaxFirePosRangeIndirectFire();

        public bool CanCaptureZones => _unitData.CanCaptureZones;

        public bool IsMoving() => !_movementComponent.AreOrdersComplete();
    }
}
