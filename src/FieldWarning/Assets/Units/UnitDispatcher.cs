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
using PFW.Units.Component.Data;
using PFW.Units.Component.Weapon;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Health;
using PFW.Units.Component.Movement;
using PFW.Units.Component.Armor;

namespace PFW.Units
{
    /// <summary>
    /// The dispatcher represents the unit to the outside world
    /// while delegating all tasks to the various unit components.
    /// </summary>
    public class UnitDispatcher
    {
        // Handles move orders:
        // private INavigationComponent _navigationComponent;

        // Contains weapon components which contain audio components etc:
        private TargetingComponent[] _targetingComponents;

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
        public Transform Transform => _movementComponent.transform;
        public GameObject GameObject => _movementComponent.gameObject;

        /// <summary>
        /// The target tuple for targeting this unit. Do NOT manually
        /// create additional target tuples.
        /// </summary>
        public readonly TargetTuple TargetTuple;

        // TODO: This is only held by this class as a way to get it to VisibilityManager. Figure out the best way to do that.
        public VisionComponent VisionComponent;

        private DataComponent _unitData;
        private VoiceComponent _voiceComponent;

        // TODO move to a component class:
        private GameObject _selectionCircle;

        public static GameObject SELECTION_CIRCLE_PREFAB =
                Resources.Load<GameObject>("SelectionCircle");

        public PlatoonBehaviour Platoon {
            get {
                return _movementComponent.Platoon;
            }
            set {
                _movementComponent.Platoon = value;
            }
        }

        public UnitDispatcher(GameObject unitInstance, PlatoonBehaviour platoon)
        {
            TargetTuple = new TargetTuple(this);

            var behaviour = unitInstance.GetComponent<SelectableBehavior>();
            behaviour.Platoon = platoon;

            _unitData = unitInstance.GetComponent<DataComponent>();

            _voiceComponent      = unitInstance.transform.Find("VoiceComponent")
                                               .GetComponent<VoiceComponent>();
            _movementComponent   = unitInstance.GetComponent<MovementComponent>();
            _targetingComponents = unitInstance.GetComponents<TargetingComponent>();
            _healthComponent     = unitInstance.GetComponent<HealthComponent>();
            _armorComponent      = unitInstance.GetComponent<ArmorComponent>();
            VisionComponent      = unitInstance.GetComponent<VisionComponent>();

            // Only used in this class, not really configurable, and no way to get a reference
            // to it here if it's instantiated in the UnitFitter. I think it's fine to leave it here.
            _selectionCircle = GameObject.Instantiate(SELECTION_CIRCLE_PREFAB, Transform);

            _movementComponent.Initialize(this);
            _healthComponent.Initialize(this);
            VisionComponent.Initialize(this);
            _armorComponent.Initialize();
        }

        public void SendFirePosOrder(Vector3 position)
        {
            foreach (var targeter in _targetingComponents)
                targeter.SetTarget(position);
        }

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
                _voiceComponent.PlaySelectionVoiceline(true);
        #endregion

        public T GetComponent<T>() => _movementComponent.GetComponent<T>();

        public float GetHealth() => _healthComponent.Health;
        public float MaxHealth => _unitData.MaxHealth;

        public void HandleHit(
            List<WeaponData.WeaponDamage> receivedDamage,
            Vector3? displacementToTarget,
            float? distanceToCentre)
            =>
            _armorComponent.HandleHit(receivedDamage,displacementToTarget, distanceToCentre);

        public void WakeUp() => _movementComponent.WakeUp();
        public void Teleport(Vector3 position, float heading) =>
                _movementComponent.Teleport(position, heading);

        public bool AreOrdersComplete() => _movementComponent.AreOrdersComplete();
        public void SetDestination(Vector3 pos, float heading) =>
                _movementComponent.SetDestination(pos, heading);

        public bool IsVisible => VisionComponent.IsVisible;
    }
}