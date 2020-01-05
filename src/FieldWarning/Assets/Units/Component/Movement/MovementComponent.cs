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

using PFW.Model.Game;
using PFW.Units.Component.Data;
using PFW.Units.Component.Weapon;

namespace PFW.Units.Component.Movement
{
    public sealed class MovementComponent : MonoBehaviour
    {
        public const float NO_HEADING = float.MaxValue;
        private const float ORIENTATION_RATE = 5.0f;
        private const float TRANSLATION_RATE = 5.0f;

        public DataComponent Data { get; private set; }
        public PlatoonBehaviour Platoon { get; set; }
        public Pathfinder Pathfinder { get; private set; }

        public MobilityType Mobility;

        // These are set by the subclass in DoMovement()
        // protected Vector3 _position;
        // protected Vector3 _rotation;

        // Forward and right directions on the horizontal plane
        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }

        // This is redundant with transform.rotation.localEulerAngles, 
        // but it is necessary because the localEulerAngles will sometimes 
        // automatically change to some new equivalent angles
        private Vector3 _currentRotation;

        public UnitDispatcher Dispatcher;

        private VehicleMovementStrategy _moveStrategy;

        private void Start()
        {
            MatchSession.Current.RegisterUnitBirth(Dispatcher);
        }

        // This needs to be separate from Initialize because it is also needed by the ghost platoon
        public void InitializeGhost(TerrainMap map)
        {
            Data = gameObject.GetComponent<DataComponent>();
            Mobility = MobilityType.MobilityTypes[Data.MobilityTypeIndex];
            _moveStrategy = new VehicleMovementStrategy(Data, map, transform, Mobility);
        }

        public void Initialize(UnitDispatcher dispatcher)
        {
            Platoon = gameObject.GetComponent<SelectableBehavior>().Platoon;
            InitializeGhost(MatchSession.Current.TerrainMap);
            Dispatcher = dispatcher;
        }

        public void WakeUp()
        {
            enabled = true;
            SetVisible(true);
            foreach (TargetingComponent targeter in GetComponents<TargetingComponent>())
                targeter.WakeUp();

            Pathfinder = new Pathfinder(this, MatchSession.Current.PathData);
            _moveStrategy.Pathfinder = Pathfinder; // TODO move contents of initialize here
        }

        private void Update()
        {
            _moveStrategy.DoMovement();

            if (_moveStrategy.IsMoving())
                _moveStrategy.UpdateMapOrientation(Forward, Right);

            UpdateCurrentRotation();
            UpdateCurrentPosition();
        }

        private void UpdateCurrentPosition()
        {
            Vector3 diff = (_moveStrategy.TargetPosition - transform.position) * Time.deltaTime;
            Vector3 newPosition = transform.position;
            newPosition.x += TRANSLATION_RATE * diff.x;
            newPosition.y = _moveStrategy.TargetPosition.y;
            newPosition.z += TRANSLATION_RATE * diff.z;

            transform.position = newPosition;
        }

        private void UpdateCurrentRotation()
        {
            Vector3 diff = _moveStrategy.TargetRotation - _currentRotation;
            if (diff.sqrMagnitude > 1) {
                _currentRotation = _moveStrategy.TargetRotation;
            } else {
                _currentRotation += ORIENTATION_RATE * Time.deltaTime * diff;
            }

            transform.localEulerAngles = Mathf.Rad2Deg * new Vector3(
                    -_currentRotation.x, -_currentRotation.y, _currentRotation.z);
            Forward = new Vector3(
                    -Mathf.Sin(_currentRotation.y), 0f, Mathf.Cos(_currentRotation.y));
            Right = new Vector3(Forward.z, 0f, -Forward.x);
        }

        // Waypoint-aware path setting. TODO there are like 5 methods for this,
        // perhaps some can be cut?
        public void SetUnitDestination(MoveWaypoint waypoint)
        {
            float a = Pathfinder.SetPath(waypoint.Destination, waypoint.MoveMode);
            if (a < Pathfinder.FOREVER)
                SetUnitFinalHeading(waypoint.Heading);
        }

        // Sets the unit's destination location, with a default heading value
        public void SetDestination(Vector3 d)
        {
            SetDestination(d, NO_HEADING);
        }

        // Sets the unit's destination location, with a specific given heading value
        public void SetDestination(Vector3 d, float heading)
        {
            if (Pathfinder.SetPath(d, MoveCommandType.FAST) < Pathfinder.FOREVER)
                SetUnitFinalHeading(heading);
        }

        // Updates the unit's final heading so that it faces the specified location
        public void SetUnitFinalFacing(Vector3 v)
        {
            Vector3 diff;
            if (Pathfinder.HasDestination())
                diff = v - Pathfinder.GetDestination();
            else
                diff = v - transform.position;

            SetUnitFinalHeading(diff.getRadianAngle());
        }

        // Updates the unit's final heading to the specified value
        public void SetUnitFinalHeading(float heading) => 
                _moveStrategy.SetUnitFinalHeading(heading);

        private void SetLayer(int l)
        {
            gameObject.layer = l;
        }

        // TODO move out of here
        public Renderer[] GetRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            return renderers;
        }

        public void SetVisible(bool vis)
        {
            var renderers = GetRenderers();
            foreach (var r in renderers)
                r.enabled = vis;

            if (vis)
                SetLayer(LayerMask.NameToLayer("Selectable"));
            else
                SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
        }

//        private float GetHeading()
//        {
//            return (Pathfinder.GetDestination() - transform.position).getDegreeAngle();
//        }

        public bool AreOrdersComplete() => _moveStrategy.AreOrdersComplete();

        // Heading given in radians
        public void Teleport(Vector3 pos, float heading)
        {
            _moveStrategy.TargetPosition = pos;
            transform.position = pos;

            _moveStrategy.TargetRotation = new Vector3(0f, heading, 0f);
            transform.eulerAngles = Mathf.Rad2Deg * _moveStrategy.TargetRotation;
            _moveStrategy.UpdateMapOrientation(Forward, Right);
        }
    }
}