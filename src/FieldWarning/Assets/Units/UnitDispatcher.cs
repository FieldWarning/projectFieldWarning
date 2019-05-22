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
using PFW.Units.Component.Damage;
using PFW.Units.Component.Vision;
using PFW.Units.Component.Health;
using PFW.Units.Component.Movement;
using PFW.Model.Game;

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

        private VoiceComponent _voiceComponent;

        // TODO move to a component class:
        private GameObject _selectionCircle;

        // TODO pass from some factory:
        private static GameObject VOICE_PREFAB =
                Resources.Load<GameObject>("VoiceComponent_US");
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

        public UnitDispatcher(
            MovementComponent movementComponent,
            PlatoonBehaviour platoon)
        {
            TargetTuple = new TargetTuple(this);

            _movementComponent = movementComponent;
            _movementComponent.Dispatcher = this;

            VisionComponent = new VisionComponent(GameObject, this);

            _targetingComponents = movementComponent.GetComponents<TargetingComponent>();

            _healthComponent = new HealthComponent(
                    _movementComponent.Data.maxHealth,
                    platoon,
                    GameObject,
                    this,
                    TargetTuple);

            _voiceComponent = GameObject.Instantiate(
                    VOICE_PREFAB,
                    Transform).GetComponent<VoiceComponent>();

            _selectionCircle = GameObject.Instantiate(
                    SELECTION_CIRCLE_PREFAB,
                    Transform);

            Transform.gameObject.AddComponent<SelectableBehavior>().Platoon = Platoon;

            Platoon = platoon;
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
        public float MaxHealth => _movementComponent.Data.maxHealth;
        
        // TODO: move HandelHit to ArmorComponent

        /// <summary>
        /// Calculate the total damage dealt within a successful hit, then update health, armor and ERA values accordingly
        /// </summary>
        /// <param name="receivedDamage"></param>
        public void HandleHit(WeaponData.WeaponDamage receivedDamage, float? distanceToTarget, float? distanceToCentre)
        {
            // First we put up a Target struct to be used in the damage calculations
            Damage.Target unitAsTarget = new Damage.Target();

            // TODO: implement armor and ERA systems
            unitAsTarget.Armor = 0.0f;
            unitAsTarget.EraData = new Damage.Era();

            unitAsTarget.Health = _healthComponent.Health;

            Damage.Target finalState = unitAsTarget;

            // Calculate its damage using its damage type
            switch (receivedDamage.DamageType)
            {
                case DamageTypes.KE:
                    KEDamage keDamage = new KEDamage (
                        receivedDamage.KineticData.GetValueOrDefault(),
                        unitAsTarget, distanceToTarget.GetValueOrDefault()
                    );
                    finalState = keDamage.CalculateDamage();
                    break;
                case DamageTypes.HEAT:
                    HeatDamage heatDamage = new HeatDamage(receivedDamage.HeatData.GetValueOrDefault(), unitAsTarget);
                    finalState = heatDamage.CalculateDamage();
                    break;
                case DamageTypes.HE:
                    HEDamage heDamage = new HEDamage (
                        receivedDamage.HEData.GetValueOrDefault(),
                        unitAsTarget, distanceToCentre.GetValueOrDefault()
                    );
                    finalState = heDamage.CalculateDamage();
                    break;
                case DamageTypes.FIRE:
                    FireDamage fireDamage = new FireDamage(receivedDamage.FireData.GetValueOrDefault(), unitAsTarget);
                    finalState = fireDamage.CalculateDamage();
                    break;
                case DamageTypes.SMALLARMS:
                    SmallarmsDamage lightarmsDamage = new SmallarmsDamage(receivedDamage.LightarmsData.GetValueOrDefault(), unitAsTarget);
                    break;
                default:
                    Debug.LogError("Not a valid damage type!");
                    break;
            }

            _healthComponent.UpdateHealth(finalState.Health);
            // Update armor and ERA values as well when the mechanisms are implemented
        }

        public void SetOriginalOrientation(Vector3 position, float heading) =>
                _movementComponent.SetOriginalOrientation(position, heading);

        public bool AreOrdersComplete() => _movementComponent.AreOrdersComplete();
        public void SetDestination(MoveWaypoint waypoint) =>
                _movementComponent.SetUnitDestination(waypoint);

        public InfantryMovementComponent AsInfantry()
        {
            return _movementComponent as InfantryMovementComponent;
        }

        public bool IsVisible => VisionComponent.IsVisible;
    }
}