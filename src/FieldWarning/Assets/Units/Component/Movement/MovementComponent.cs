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

using PFW.Model.Match;
using PFW.Units.Component.Data;

namespace PFW.Units.Component.Movement
{
    public sealed class MovementComponent : MonoBehaviour
    {
        public const float NO_HEADING = float.MaxValue;
        private const float ORIENTATION_RATE = 8.0f;
        private const float TRANSLATION_RATE = 6.0f;

        public Vector3 Velocity; //{ get; private set; }
        public DataComponent Data { get; private set; }
        public Pathfinder Pathfinder { get; private set; }

        public MobilityData Mobility;

        // Forward and right directions on the horizontal plane
        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }

        // This is redundant with transform.rotation.localEulerAngles, 
        // but it is necessary because the localEulerAngles will sometimes 
        // automatically change to some new equivalent angles
        private Vector3 _currentRotation;

        private VehicleMovementStrategy _moveStrategy;

        // This needs to be separate from Initialize because it is also needed by the ghost platoon
        public void InitializeGhost(TerrainMap map)
        {
            Data = gameObject.GetComponent<DataComponent>();
            Mobility = Data.MobilityData;
            _moveStrategy = new VehicleMovementStrategy(
                    Data, map, transform, Mobility);
        }

        public void Initialize()
        {
            InitializeGhost(MatchSession.Current.TerrainMap);

            // TODO perhaps pathfinder should use a movementstrategy and be created
            // in the movement strategy directly.
            Pathfinder = new Pathfinder(this, MatchSession.Current.PathData);
            _moveStrategy.Pathfinder = Pathfinder;
        }

        private void Update()
        {
            _moveStrategy.PlanMovement();

            if (_moveStrategy.IsMoving())
                _moveStrategy.UpdateMapOrientation(Forward, Right);

            // TODO I think the values generated from the movement strategy should
            // be applied directly, especially since it already scales them based on
            // delta time. The fact that this code does more calculation
            // and applies delta time again seems like a bug, should investigate.
            UpdateCurrentRotation();
            UpdateCurrentPosition();
        }

        private void UpdateCurrentPosition()  
        {
            //update position and velocity param of this unit

            Vector3 diff = (_moveStrategy.NextPosition - transform.position) * Time.deltaTime;
            Vector3 newPosition = transform.position;
            newPosition.x += TRANSLATION_RATE * diff.x;
            newPosition.y = _moveStrategy.NextPosition.y;
            newPosition.z += TRANSLATION_RATE * diff.z;

            //calculate instantaneous velocity property to be read from outside for shell-lead
            Velocity.x = TRANSLATION_RATE * diff.x;
            Velocity.y = newPosition.y - transform.position.y;
            Velocity.z = TRANSLATION_RATE * diff.z;
            Velocity = Velocity / Time.deltaTime;

            transform.position = newPosition;
        }

        private void UpdateCurrentRotation()
        {
            Vector3 diff = _moveStrategy.NextRotation - _currentRotation;
            if (diff.sqrMagnitude > 1) 
            {
                _currentRotation = _moveStrategy.NextRotation;
            } 
            else 
            {
                _currentRotation += ORIENTATION_RATE * Time.deltaTime * diff;
            }

            transform.localEulerAngles = Mathf.Rad2Deg * new Vector3(
                    -_currentRotation.x, -_currentRotation.y, _currentRotation.z);
            Forward = new Vector3(
                    -Mathf.Sin(_currentRotation.y), 0f, Mathf.Cos(_currentRotation.y));
            Right = new Vector3(Forward.z, 0f, -Forward.x);
        }

        /// <summary>
        /// Sets the unit's destination location, 
        /// with a specific given heading value.
        /// </summary>
        public void SetDestination(
                Vector3 d, 
                float heading = NO_HEADING, 
                MoveCommandType moveMode = MoveCommandType.FAST)
        {
            // multithreading
            Pathfinder.SetPathAndForget(d, moveMode);
            _moveStrategy.FinalHeading = heading;
        }

        public void CancelOrders()
        {
            // TODO use a non-hacky solution
            SetDestination(transform.position);
        }

        public void OnDestroy()
        {
            if (Pathfinder != null)
            {
                Pathfinder.Dispose();
            }
        }

        public bool AreOrdersComplete() => _moveStrategy.AreOrdersComplete();

        /// <summary>
        /// Heading given in radians
        /// </summary>
        public void Teleport(Vector3 pos, float heading)
        {
            _moveStrategy.NextPosition = pos;
            transform.position = pos;

            _moveStrategy.NextRotation = new Vector3(0f, heading, 0f);
            transform.eulerAngles = Mathf.Rad2Deg * _moveStrategy.NextRotation;
            _moveStrategy.UpdateMapOrientation(Forward, Right);
        }
    }
}
