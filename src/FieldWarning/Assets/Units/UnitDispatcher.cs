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

        #region Potential future components
        //private IAudioComponent _voiceComponent;
        //private IAudioComponent _soundComponent;
        //private IHealthComponent _healthComponent;
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
        public Transform transform => _unitBehaviour.transform;
        public GameObject gameObject => _unitBehaviour.gameObject;


        // TODO: This is only held by this class as a way to get it to VisibilityManager. Figure out the best way to do that.
        public VisibleBehavior VisibleBehavior;


        public UnitDispatcher(UnitBehaviour unitBehaviour)
        {
            _unitBehaviour = unitBehaviour;
            _unitBehaviour.Dispatcher = this;
            VisibleBehavior = new VisibleBehavior(gameObject, _unitBehaviour);

            _targetingComponents = unitBehaviour.GetComponents<TargetingComponent>();
        }

        public void SendFirePosOrder(Vector3 position)
        {
            foreach (var targeter in _targetingComponents)
                targeter.SetTarget(position);
        }

        public void SetSelected(bool selected, bool justPreviewing)
        {
            _unitBehaviour.SetSelected(selected, justPreviewing);
        }

        public void PlayAttackCommandVoiceline() =>
            _unitBehaviour.PlayAttackCommandVoiceline();
        public void PlayMoveCommandVoiceline() =>
            _unitBehaviour.PlayMoveCommandVoiceline();
        public void PlaySelectionVoiceline() =>
            _unitBehaviour.PlaySelectionVoiceline();

        public T GetComponent<T>() => _unitBehaviour.GetComponent<T>();

        public float GetHealth() => _unitBehaviour.GetHealth();
        public float MaxHealth => _unitBehaviour.Data.maxHealth;

        public void SetOriginalOrientation(Vector3 position, float heading) =>
            _unitBehaviour.SetOriginalOrientation(position, heading);

        public bool OrdersComplete() => _unitBehaviour.OrdersComplete();
        public void SetDestination(MoveWaypoint waypoint) =>
            _unitBehaviour.SetUnitDestination(waypoint);

        public InfantryBehaviour AsInfantry()
        {
            return _unitBehaviour as InfantryBehaviour;
        }
    }
}