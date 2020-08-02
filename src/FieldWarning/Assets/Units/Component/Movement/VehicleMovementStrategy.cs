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

using PFW.Units.Component.Data;
using System;

namespace PFW.Units.Component.Movement
{
    public sealed class VehicleMovementStrategy : IMovementStrategy
    {
        private const float DECELERATION_FACTOR = 2.5f;
        private const float HEADING_THRESHOLD = 3f * Mathf.Deg2Rad;
        private const float FORWARD_BIAS = 0.5f;

        private float _linVelocity;
        private float _rotVelocity;
        private float _forwardAccel;

        private float _terrainTiltForward, _terrainTiltRight;
        private float _terrainHeight;

        private readonly DataComponent _data;
        private readonly TerrainMap _terrainMap;
        public Pathfinder Pathfinder;
        private readonly Transform _transform;
        private readonly MobilityData _mobility;


        public Vector3 NextPosition;
        public Vector3 NextRotation;
        public float FinalHeading;

        public VehicleMovementStrategy(
                DataComponent data, TerrainMap map, Transform transform, MobilityData mobility)
        {
            _data = data;
            _terrainMap = map;
            _transform = transform;
            _mobility = mobility;
        }

        public void PlanMovement()
        {
            Logger.LogPathfinding(
                    LogLevel.DUMP,
                    $"PlanMovement(): waypoint = {Pathfinder.GetWaypoint()}");
            float distanceToWaypoint = 
                    Pathfinder.HasDestination() ? 
                            (Pathfinder.GetWaypoint() - NextPosition).magnitude : 0f;
            float linSpeed = Mathf.Abs(_linVelocity);
            float rotationSpeed = CalculateRotationSpeed(linSpeed);

            //float remainingTurn = CalculateRemainingTurn(targetHeading);
            float targetHeading = GetTargetHeading();
            float turnForward = 0f;
            float turnReverse = 0f;
            if (targetHeading != MovementComponent.NO_HEADING)
            {
                turnForward = (targetHeading - NextRotation.y - Mathf.PI / 2).unwrapRadian();
                turnReverse = (targetHeading - NextRotation.y + Mathf.PI / 2).unwrapRadian();
            }
            bool isReverse = ShouldReverse(
                    _linVelocity, distanceToWaypoint, rotationSpeed, turnForward, turnReverse);
            float remainingTurn = isReverse ? turnReverse : turnForward;

            float targetSpeed = CalculateTargetSpeed(
                    distanceToWaypoint, remainingTurn, linSpeed, rotationSpeed, Pathfinder.WaypointAngleChange);
            if (isReverse)
                targetSpeed = -targetSpeed;

            DoRotationalMotion(remainingTurn, rotationSpeed);
            DoLinearMotion(targetSpeed);
        }

        public void UpdateMapOrientation(Vector3 forward, Vector3 right)
        {
            int terrainType = _terrainMap == null ? 
                    TerrainMap.PLAIN : _terrainMap.GetTerrainType(_transform.position);
            if (terrainType == TerrainMap.BRIDGE)
            {
                _terrainTiltForward = 0f;
                _terrainTiltRight = 0f;
                _terrainHeight = TerrainMap.BRIDGE_HEIGHT;
            }
            else if (terrainType == TerrainMap.WATER)
            {
                _terrainTiltForward = 0f;
                _terrainTiltRight = 0f;
                _terrainHeight = _terrainMap.WATER_HEIGHT;
            }
            else
            {
                // This way of doing the rotation should look nice because the unit 
                // won't sink into the ground much assuming length and width are set 
                // correctly, but it is not very fast.

                Terrain terrain = _terrainMap.GetTerrainAtPos(_transform.position);
                if (terrain == null)
                    return;

                // TODO SampleHeight is very slow, we should cache this information.
                // Apparently our forward and backward are opposite of the Unity convention
                float frontHeight = terrain.SampleHeight(
                        _transform.position + forward * _data.Length / 2);
                float rearHeight = terrain.SampleHeight(
                        _transform.position - forward * _data.Length / 2);
                float leftHeight = terrain.SampleHeight(
                        _transform.position - right * _data.Width / 2);
                float rightHeight = terrain.SampleHeight(
                        _transform.position + right * _data.Width / 2);

                _terrainHeight = Mathf.Max(
                        (frontHeight + rearHeight) / 2, 
                        (leftHeight + rightHeight) / 2);
                _terrainTiltForward = Mathf.Atan((frontHeight - rearHeight) / _data.Length);
                _terrainTiltRight = Mathf.Atan((rightHeight - leftHeight) / _data.Width);
            }
        }

        public bool IsMoving()
        {
            return Mathf.Abs(_linVelocity) > 0f || Mathf.Abs(_rotVelocity) > 0f;
        }

        public bool AreOrdersComplete()
        {
            return !Pathfinder.HasDestination();
        }

        /// <summary>
        /// Target heading currently only depends on the waypoint 
        /// and final heading, but units will also need to 
        /// face armor and weapons
        /// </summary>
        private float GetTargetHeading()
        {
            float destinationHeading = FinalHeading;

            if (Pathfinder.HasDestination()) 
            {
                Vector3 diff = Pathfinder.GetWaypoint() - NextPosition;
                if (diff.magnitude > Pathfinder.FinalCompletionDist)
                    destinationHeading = diff.getRadianAngle();
            }

            return destinationHeading;
        }

        /// <summary>
        /// Calculate the unit's maximum rotational speed in 
        /// rads/sec at the given linear speed.
        /// All angles need to have units of radians.
        /// </summary>
        private float CalculateRotationSpeed(float linSpeed)
        {
            float turnRadius = Mathf.Max(
                    _data.MinTurnRadius, 
                    linSpeed * linSpeed / _data.MaxLateralAccel);

            float rotSpeed = Mathf.Deg2Rad * _data.MaxRotationSpeed;
            if (turnRadius > 0f)
                rotSpeed = Mathf.Min(rotSpeed, linSpeed / turnRadius);

            return rotSpeed;
        }

        /// <summary>
        /// Returns true if the unit should be moving in reverse.
        /// </summary>
        private bool ShouldReverse(
                float linVelocity, 
                float linDist, 
                float rotationSpeed, 
                float turnForward, 
                float turnReverse)
        {
            if (linDist < Pathfinder.FinalCompletionDist)
                return false;

            if (Pathfinder.Command == MoveCommandType.REVERSE)
                return true;

            float timeForward = Mathf.Abs(turnForward) / rotationSpeed 
                                + linDist / _data.MovementSpeed;
            float timeReverse = Mathf.Abs(turnReverse) / rotationSpeed 
                                + linDist / _data.ReverseSpeed;

            float accelTime = 2 * Mathf.Abs(linVelocity) 
                              / (_data.AccelRate * (1 + DECELERATION_FACTOR));
            if (linVelocity > 0) 
            {
                timeReverse += accelTime;
            } 
            else 
            {
                timeForward += accelTime;
            }

            return timeReverse + FORWARD_BIAS < timeForward;
        }

        /// <summary>
        /// Finds the linear speed that gets the unit to the 
        /// desired distance/angle the fastest. 
        /// All angles in units of radians.
        /// </summary>
        private float CalculateTargetSpeed(
                float linDist, float remainingTurn, float linSpeed, float rotSpeed, float waypointAngleChange)
        {

            // Need to face approximately the right direction before speeding up
            float angDist = Mathf.Max(0f, Mathf.Abs(remainingTurn) - HEADING_THRESHOLD);
            if (angDist > Mathf.PI / 5)
                return _data.OptimumTurnSpeed;

            // Need to slow down to make tight turns, but not along straighter paths
            // This is just an arbitrary formula that works well
            float turnTightness = Math.Min(1f, waypointAngleChange / 110f);
            float slowdown = 0.01f + 0.8f * turnTightness * turnTightness * turnTightness;
            
            // Want to go just fast enough to cover the linear and angular distance
            // if the unit starts slowing down now
            float longestDist = Mathf.Max(
                    linDist - 0.5f * _data.AccelDampTime * linSpeed,
                    _data.MinTurnRadius * angDist);
            float targetSpeed = Mathf.Sqrt(
                    longestDist * _data.AccelRate * DECELERATION_FACTOR /
                    (slowdown - slowdown*slowdown/2));

            // But not so fast that it cannot make the turn
            if (linSpeed > _data.OptimumTurnSpeed && angDist > 0f)
                targetSpeed = Mathf.Min(targetSpeed, 0.25f * linDist * rotSpeed / angDist);

            return targetSpeed;
        }

        private void DoLinearMotion(float targetSpeed)
        {
            float terrainSpeedMultiplier = _mobility.GetUnitSpeedMultiplier(
                    Pathfinder.Data._map,
                    _transform.position,
                    0f,
                    -_transform.forward);

            if (terrainSpeedMultiplier <= 0f) 
            { 
                // This is so the unit will "bump off" impassible terrain
                _linVelocity = 0f;
                Bounce();
                //_position -= transform.forward * Data.movementSpeed * Time.deltaTime;
            } 
            else 
            {
                targetSpeed = terrainSpeedMultiplier * Mathf.Clamp(
                        targetSpeed, -_data.ReverseSpeed, _data.MovementSpeed);

                _forwardAccel = Mathf.Sign(targetSpeed - _linVelocity) * _data.AccelRate;
                if (Mathf.Sign(_forwardAccel) != Mathf.Sign(_linVelocity))
                    _forwardAccel *= DECELERATION_FACTOR;

                if (Mathf.Abs(_forwardAccel) > 0) 
                {
                    float accelTime = (targetSpeed - _linVelocity) / _forwardAccel;
                    if (accelTime < _data.AccelDampTime)
                        _forwardAccel *= (0.25f + 0.75f * accelTime / _data.AccelDampTime);

                    if (_forwardAccel > 0) 
                    {
                        _linVelocity = Mathf.Min(
                                targetSpeed, 
                                _linVelocity + _forwardAccel * Time.deltaTime);
                    } 
                    else 
                    {
                        _linVelocity = Mathf.Max(
                                targetSpeed, 
                                _linVelocity + _forwardAccel * Time.deltaTime);
                    }
                }

                NextPosition += _transform.forward * _linVelocity * Time.deltaTime;
            }

            NextPosition.y = _terrainHeight;
        }

        private void DoRotationalMotion(float remainingTurn, float rotationSpeed)
        {
            if (Mathf.Abs(remainingTurn) < HEADING_THRESHOLD) 
            {
                _rotVelocity = 0f;
            }
            else 
            {
                _rotVelocity = Mathf.Sign(remainingTurn) * rotationSpeed;
            }

            float turn = _rotVelocity * Time.deltaTime;
            if (Mathf.Abs(turn) > Mathf.Abs(remainingTurn))
                turn = remainingTurn;
            NextRotation.y += turn;

            float accelTiltForward = _data.SuspensionForward * _forwardAccel;
            float accelTiltRight = _data.SuspensionSide * _linVelocity * _rotVelocity;

            NextRotation.x = _terrainTiltForward + accelTiltForward;
            NextRotation.z = _terrainTiltRight - accelTiltRight;
        }

        // Bounce off impassable terrain, hopefully back to somewhere the unit belongs
        private const float BOUNCE_RADIUS = 4f * Constants.MAP_SCALE;
        private void Bounce()
        {
            Vector3 bounceDirection = Vector3.zero;
            for (float angle = 0f; angle < 360f; angle += 30f)
            {
                Vector3 offset = 
                        BOUNCE_RADIUS * new Vector3(
                                Mathf.Sin(angle * Mathf.Deg2Rad), 
                                0f, 
                                Mathf.Cos(angle * Mathf.Deg2Rad));
                float speed = _mobility.GetUnitSpeedMultiplier(
                        _terrainMap, _transform.position + offset, 0f, -_transform.forward);
                if (speed > 0f)
                    bounceDirection += offset;
            }
            if (bounceDirection.magnitude < 0.01f)
                bounceDirection = -_transform.forward;

            NextPosition += bounceDirection.normalized * _data.MovementSpeed * Time.deltaTime;
        }
    }
}
