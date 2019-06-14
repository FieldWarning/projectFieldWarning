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

namespace PFW.Units.Component.Movement
{
    public class VehicleMovementComponent : MovementComponent
    {
        private const float DECELERATION_FACTOR = 2.5f;
        private const float HEADING_THRESHOLD = 3f * Mathf.Deg2Rad;
        private const float FORWARD_BIAS = 0.5f;

        private float _linVelocity;
        private float _rotVelocity;
        private float _forwardAccel;

        private float _terrainTiltForward, _terrainTiltRight;
        private float _terrainHeight;

        new void Awake()
        {
            base.Awake();
            Data = UnitData.Tank();
        }

        // Use this for initialization
        new void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        new void Update()
        {
            base.Update();
        }


        protected override void DoMovement()
        {
            float distanceToWaypoint = Pathfinder.HasDestination() ? (Pathfinder.GetWaypoint() - _position).magnitude : 0f;
            float linSpeed = Mathf.Abs(_linVelocity);
            float rotationSpeed = CalculateRotationSpeed(linSpeed);

            //float remainingTurn = CalculateRemainingTurn(targetHeading);
            float targetHeading = GetTargetHeading();
            float turnForward = 0f;
            float turnReverse = 0f;
            if (targetHeading != NO_HEADING) {
                turnForward = (targetHeading - _rotation.y - Mathf.PI / 2).unwrapRadian();
                turnReverse = (targetHeading - _rotation.y + Mathf.PI / 2).unwrapRadian();
            }
            bool isReverse = ShouldReverse(_linVelocity, distanceToWaypoint, rotationSpeed, turnForward, turnReverse);
            float remainingTurn = isReverse ? turnReverse : turnForward;

            float targetSpeed = CalculateTargetSpeed(distanceToWaypoint, remainingTurn, linSpeed, rotationSpeed);
            if (isReverse)
                targetSpeed = -targetSpeed;

            DoRotationalMotion(remainingTurn, rotationSpeed);
            DoLinearMotion(targetSpeed);
        }

        // Target heading currently only depends on the waypoint and final heading, but units will also need to face armor and weapons
        private float GetTargetHeading()
        {
            float destinationHeading = _finalHeading;

            if (Pathfinder.HasDestination()) {
                var diff = Pathfinder.GetWaypoint() - _position;
                if (diff.magnitude > Pathfinder.FinalCompletionDist)
                    destinationHeading = diff.getRadianAngle();
            }

            return destinationHeading;
        }

        // Calculate the unit's maximum rotational speed in rads/sec at the given linear speed.
        // All angles need to have units of radians
        private float CalculateRotationSpeed(float linSpeed)
        {
            float turnRadius = Mathf.Max(Data.minTurnRadius, linSpeed * linSpeed / Data.maxLateralAccel);

            float rotSpeed = Mathf.Deg2Rad * Data.maxRotationSpeed;
            if (turnRadius > 0f)
                rotSpeed = Mathf.Min(rotSpeed, linSpeed / turnRadius);

            return rotSpeed;
        }

        // Returns true if the unit should be moving in reverse
        private bool ShouldReverse(float linVelocity, float linDist, float rotationSpeed, float turnForward, float turnReverse)
        {
            if (linDist < Pathfinder.FinalCompletionDist)
                return false;

            if (Pathfinder.Command == MoveCommandType.Reverse)
                return true;

            float timeForward = Mathf.Abs(turnForward) / rotationSpeed + linDist / Data.movementSpeed;
            float timeReverse = Mathf.Abs(turnReverse) / rotationSpeed + linDist / Data.reverseSpeed;

            float accelTime = 2 * Mathf.Abs(linVelocity) / (Data.accelRate * (1 + DECELERATION_FACTOR));
            if (linVelocity > 0) {
                timeReverse += accelTime;
            } else {
                timeForward += accelTime;
            }

            return timeReverse + FORWARD_BIAS < timeForward;
        }

        // Finds the linear speed that gets the unit to the desired distance/angle the fastest.
        // All angles in units of radians
        private float CalculateTargetSpeed(float linDist, float remainingTurn, float linSpeed, float rotSpeed)
        {

            // Need to face approximately the right direction before speeding up
            float angDist = Mathf.Max(0f, Mathf.Abs(remainingTurn) - HEADING_THRESHOLD);
            if (angDist > Mathf.PI / 4)
                return Data.optimumTurnSpeed;

            // Want to go just fast enough to cover the linear and angular distance if the unit starts slowing down now
            float longestDist = Mathf.Max(linDist - 0.7f * Data.accelDampTime * linSpeed, Data.minTurnRadius * angDist);
            float targetSpeed = Mathf.Sqrt(2 * longestDist * Data.accelRate * DECELERATION_FACTOR);

            // But not so fast that it cannot make the turn
            if (linSpeed > Data.optimumTurnSpeed && angDist > 0f)
                targetSpeed = Mathf.Min(targetSpeed, 0.25f * linDist * rotSpeed / angDist);

            return targetSpeed;
        }

        private void DoLinearMotion(float targetSpeed)
        {
            float terrainSpeed = GetTerrainSpeedMultiplier();
            if (terrainSpeed <= 0f) { // This is so the unit will "bump off" impassible terrain
                _linVelocity = 0f;
                Bounce();
                //_position -= transform.forward * Data.movementSpeed * Time.deltaTime;
            } else {
                targetSpeed = terrainSpeed * Mathf.Clamp(targetSpeed, -Data.reverseSpeed, Data.movementSpeed);

                _forwardAccel = Mathf.Sign(targetSpeed - _linVelocity) * Data.accelRate;
                if (Mathf.Sign(_forwardAccel) != Mathf.Sign(_linVelocity))
                    _forwardAccel *= DECELERATION_FACTOR;

                if (Mathf.Abs(_forwardAccel) > 0) {
                    float accelTime = (targetSpeed - _linVelocity) / _forwardAccel;
                    if (accelTime < Data.accelDampTime)
                        _forwardAccel = _forwardAccel * (0.25f + 0.75f * accelTime / Data.accelDampTime);

                    if (_forwardAccel > 0) {
                        _linVelocity = Mathf.Min(targetSpeed, _linVelocity + _forwardAccel * Time.deltaTime);
                    } else {
                        _linVelocity = Mathf.Max(targetSpeed, _linVelocity + _forwardAccel * Time.deltaTime);
                    }
                }

                _position += transform.forward * _linVelocity * Time.deltaTime;
            }

            _position.y = _terrainHeight;
        }

        private void DoRotationalMotion(float remainingTurn, float rotationSpeed)
        {
            if (Mathf.Abs(remainingTurn) < HEADING_THRESHOLD) {
                _rotVelocity = 0f;
            } else {
                _rotVelocity = Mathf.Sign(remainingTurn) * rotationSpeed;
            }

            var turn = _rotVelocity * Time.deltaTime;
            if (Mathf.Abs(turn) > Mathf.Abs(remainingTurn))
                turn = remainingTurn;
            _rotation.y += turn;

            float accelTiltForward = Data.suspensionForward * _forwardAccel;
            float accelTiltRight = Data.suspensionSide * _linVelocity * _rotVelocity;

            _rotation.x = _terrainTiltForward + accelTiltForward;
            _rotation.z = _terrainTiltRight - accelTiltRight;
        }

        // Bounce off impassible terrain, hopefully back to somewhere the unit belongs
        private const float BOUNCE_RADIUS = 4f * TerrainConstants.MAP_SCALE;
        private void Bounce()
        {
            Vector3 bounceDirection = Vector3.zero;
            for (float angle = 0f; angle < 360f; angle += 30f) {
                Vector3 offset = BOUNCE_RADIUS * new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
                float speed = Data.mobility.GetUnitSpeed(Pathfinder.Data.Terrain, Pathfinder.Data.Map, transform.position + offset, 0f, -transform.forward);
                if (speed > 0f)
                    bounceDirection += offset;
            }
            if (bounceDirection.magnitude < 0.01f)
                bounceDirection = -transform.forward;

            _position += bounceDirection.normalized * Data.movementSpeed * Time.deltaTime;
        }

        protected override Renderer[] GetRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            return renderers;
        }

        // Heading given in radians
        public override void SetOriginalOrientation(Vector3 pos, float heading, bool wake = true)
        {
            if (wake)
                WakeUp();

            _position = pos;
            transform.position = pos;

            base._rotation = new Vector3(0f, heading, 0f);
            transform.eulerAngles = Mathf.Rad2Deg * base._rotation;
            UpdateMapOrientation();
        }

        public override void UpdateMapOrientation()
        {
            int terrainType = Pathfinder == null ? TerrainMap.PLAIN : Pathfinder.Data.Map.GetTerrainType(transform.position);
            if (terrainType == TerrainMap.BRIDGE) {
                _terrainTiltForward = 0f;
                _terrainTiltRight = 0f;
                _terrainHeight = TerrainMap.BRIDGE_HEIGHT;
            } else if (terrainType == TerrainMap.WATER) {
                _terrainTiltForward = 0f;
                _terrainTiltRight = 0f;
                _terrainHeight = Pathfinder.Data.Map.WATER_HEIGHT;
            } else {
                // This way of doing the rotation should look nice because the unit won't sink into the ground
                //      much assuming length and width are set correctly, but it is not very fast

                // Apparently our forward and backward are opposite of the Unity convention
                float frontHeight = Terrain.activeTerrain.SampleHeight(transform.position + Forward * Data.length / 2);
                float rearHeight = Terrain.activeTerrain.SampleHeight(transform.position - Forward * Data.length / 2);
                float leftHeight = Terrain.activeTerrain.SampleHeight(transform.position - Right * Data.width / 2);
                float rightHeight = Terrain.activeTerrain.SampleHeight(transform.position + Right * Data.width / 2);

                _terrainHeight = Mathf.Max((frontHeight + rearHeight) / 2, (leftHeight + rightHeight) / 2);
                _terrainTiltForward = Mathf.Atan((frontHeight - rearHeight) / Data.length);
                _terrainTiltRight = Mathf.Atan((rightHeight - leftHeight) / Data.width);
            }
        }

        protected override bool IsMoving()
        {
            return Mathf.Abs(_linVelocity) > 0f || Mathf.Abs(_rotVelocity) > 0f;
        }

        public override bool AreOrdersComplete()
        {
            return !Pathfinder.HasDestination();
        }
    }
}