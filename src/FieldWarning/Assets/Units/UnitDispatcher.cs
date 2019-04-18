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
using PFW.Units.Component.Weapon;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Health;

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

        // TODO remove
        private UnitBehaviour _unitBehaviour;
        public PlatoonBehaviour Platoon => _unitBehaviour.Platoon;
        public Transform Transform => _unitBehaviour.transform;
        public GameObject GameObject => _unitBehaviour.gameObject;


        // TODO: This is only held by this class as a way to get it to VisibilityManager. Figure out the best way to do that.
        public VisionComponent VisionComponent;

        private VoiceComponent _voiceComponent;

        // TODO move to a component class:
        private GameObject _selectionCircle;

        // TODO pass from some factory:
        private static GameObject VOICE_PREFAB =
            Resources.Load<GameObject>("VoiceComponent_US");
        public static GameObject SELECTION_CIRCLE_PREFAB =
            Resources.Load<GameObject>("SelectionCircle");

        public UnitDispatcher(UnitBehaviour unitBehaviour)
        {
            _unitBehaviour = unitBehaviour;
            _unitBehaviour.Dispatcher = this;
            VisionComponent = new VisionComponent(GameObject, _unitBehaviour);

            _targetingComponents = unitBehaviour.GetComponents<TargetingComponent>();
            _healthComponent = new HealthComponent(
                _unitBehaviour.Data.maxHealth, Platoon, GameObject, this);
            _voiceComponent = GameObject.Instantiate(
                VOICE_PREFAB, Transform).GetComponent<VoiceComponent>();
            _selectionCircle = GameObject.Instantiate(
                SELECTION_CIRCLE_PREFAB, Transform);
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

        public void SetPlatoon(PlatoonBehaviour p) => _unitBehaviour.SetPlatoon(p);
        #region PlayVoicelines
        public void PlayAttackCommandVoiceline() =>
            _voiceComponent.PlayAttackCommandVoiceline();
        public void PlayMoveCommandVoiceline() =>
            _voiceComponent.PlayMoveCommandVoiceline();
        public void PlaySelectionVoiceline() =>
            _voiceComponent.PlaySelectionVoiceline(true);
        #endregion

        public T GetComponent<T>() => _unitBehaviour.GetComponent<T>();

        public float GetHealth() => _healthComponent.Health;
        public float MaxHealth => _unitBehaviour.Data.maxHealth;
        public void HandleHit(float receivedDamage) =>
            _healthComponent.HandleHit(receivedDamage);

        public void SetOriginalOrientation(Vector3 position, float heading) =>
            _unitBehaviour.SetOriginalOrientation(position, heading);

        public bool AreOrdersComplete() => _unitBehaviour.AreOrdersComplete();
        public void SetDestination(MoveWaypoint waypoint) =>
            _unitBehaviour.SetUnitDestination(waypoint);

        public InfantryBehaviour AsInfantry()
        {
            return _unitBehaviour as InfantryBehaviour;
        }
    }
}