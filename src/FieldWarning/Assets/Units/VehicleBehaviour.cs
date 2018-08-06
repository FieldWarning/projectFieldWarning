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

public class VehicleBehaviour : UnitBehaviour
{
    private const float DECELERATION_FACTOR = 2.0f;

    private float _speed;

    // Use this for initialization
    new void Start()
    {
        base.Start();
        data = UnitData.Tank();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }


    protected override void DoMovement()
    {
        Vector3 waypoint = pathfinder.GetWaypoint();

        float destinationHeading = CalculateDestinationHeading(waypoint);
        float remainingTurn = TurnTowardDestination(destinationHeading);

        float targetSpeed = CalculateTargetSpeed(remainingTurn, waypoint);
        UpdateRealSpeed(targetSpeed);

        transform.Translate(_speed * Time.deltaTime * Vector3.forward);
    }

    private float CalculateDestinationHeading(Vector3 waypoint)
    {
        float destinationHeading;

        if (pathfinder.HasDestination()) {
            var diff = waypoint - this.transform.position;
            destinationHeading = diff.getRadianAngle();

        } else {
            destinationHeading = finalHeading;
        }

        return destinationHeading;
    }

    private float TurnTowardDestination(float destinationHeading)
    {
        destinationHeading = destinationHeading.unwrapRadian();
        var currentHeading = Mathf.Deg2Rad * transform.localEulerAngles.y;
        var remainingTurn = (destinationHeading + currentHeading - Mathf.PI / 2).unwrapRadian();
        var turn = Mathf.Sign(remainingTurn) * data.rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(turn) > Mathf.Abs(remainingTurn))
            turn = remainingTurn;

        //var normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(transform.position.x / Terrain.activeTerrain.terrainData.bounds.size.x, transform.position.y / Terrain.activeTerrain.terrainData.bounds.size.y);

        //var desiredForward = new Vector3(Mathf.Sin(currentHeading + turn), 0, Mathf.Cos(currentHeading + turn));
        //var left = Vector3.Cross(Vector3.up, desiredForward);
        //var actualForward = Vector3.Cross(left, normal);

        //transform.up = normal;
        //transform.forward = actualForward;


        ////transform.Rotate(Vector3.up, -turn);
        ////transform.Rotate(Vector3.right, transform.eulerAngles.y - normal);

        transform.Rotate(Vector3.up, -turn);

        return remainingTurn;
    }

    private float CalculateTargetSpeed(float headingDiff, Vector3 waypoint)
    {
        float targetSpeed;

        if (!pathfinder.HasDestination()) {
            targetSpeed = 0f;

        } else {
            float destDist = (destination - transform.localPosition).magnitude;
            targetSpeed = Mathf.Min(data.movementSpeed, Mathf.Sqrt(2 * destDist * data.accelRate * DECELERATION_FACTOR));

            float waypointDist = (waypoint - transform.localPosition).magnitude;
            var turnradius = waypointDist / (1000 * Mathf.Abs(headingDiff));
            float turnFactor = data.rotationSpeed * turnradius;
            if (turnFactor < 1)
                targetSpeed *= turnFactor;
        }

        return targetSpeed;
    }

    private void UpdateRealSpeed(float targetSpeed)
    {
        if (targetSpeed > _speed) {
            _speed = Mathf.Min(targetSpeed, _speed + data.accelRate * Time.deltaTime);

        } else {
            _speed = Mathf.Max(targetSpeed, _speed - DECELERATION_FACTOR * data.accelRate * Time.deltaTime);
        }
    }

    protected override Renderer[] GetRenderers()
    {
        // Child 0 is the collider
        //return transform.GetChild(1).GetComponentsInChildren<Renderer>();

        // More generic fix..
        var renderers = GetComponentsInChildren<Renderer>();

        return renderers;
    }

    public override void SetOriginalOrientation(Vector3 pos, Quaternion rotation, bool wake = true)
    {
        if (wake)
            WakeUp();
        transform.position = pos;
        transform.localRotation = rotation;
    }

    public override void UpdateMapOrientation()
    {
        var terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);

        transform.position = new Vector3(transform.position.x, terrainHeight, transform.position.z);
        //var p = this.transform.position;
        //var y = Ground.terrainData.GetInterpolatedHeight(p.x, p.z);
        //this.transform.position = new Vector3(p.x, y, p.z);
    }

    public override bool OrdersComplete()
    {
        return !pathfinder.HasDestination();
    }
}