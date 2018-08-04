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
    const float DECELERATION_FACTOR = 2.0f;

    float speed;

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


    protected override void doMovement()
    {
        Vector3 waypoint = pathfinder.GetWaypoint();

        float destinationHeading = calculateDestinationHeading(waypoint);
        float remainingTurn = turnTowardDestination(destinationHeading);

        float targetSpeed = calculateTargetSpeed(remainingTurn, waypoint);
        updateRealSpeed(targetSpeed);
        transform.Translate(speed * Time.deltaTime * Vector3.forward);
    }

    private float calculateDestinationHeading(Vector3 waypoint)
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

    private float turnTowardDestination(float destinationHeading)
    {
        destinationHeading = destinationHeading.unwrapRadian();
        var currentHeading = Mathf.Deg2Rad * transform.localEulerAngles.y;
        var remainingTurn = (destinationHeading + currentHeading - Mathf.PI / 2).unwrapRadian();
        var turn = Mathf.Sign(remainingTurn) * data.rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(turn) > Mathf.Abs(remainingTurn))
            turn = remainingTurn;

        transform.Rotate(Vector3.up, -turn);

        return remainingTurn;
    }

    private float calculateTargetSpeed(float headingDiff, Vector3 waypoint)
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

    private void updateRealSpeed(float targetSpeed)
    {
        if (targetSpeed > speed) {
            speed = Mathf.Min(targetSpeed, speed + data.accelRate * Time.deltaTime);
        } else {
            speed = Mathf.Max(targetSpeed, speed - DECELERATION_FACTOR * data.accelRate * Time.deltaTime);
        }
    }

    protected override Renderer[] getRenderers()
    {
        // Child 0 is the collider
        return transform.GetChild(1).GetComponentsInChildren<Renderer>();
    }

    public override void setOriginalOrientation(Vector3 pos, Quaternion rotation, bool wake = true)
    {
        if (wake)
            WakeUp();
        transform.position = pos;
        transform.localRotation = rotation;
    }

    public override void updateMapOrientation()
    {
        var p = this.transform.position;
        var y = Ground.terrainData.GetInterpolatedHeight(p.x, p.z);
        this.transform.position = new Vector3(p.x, y, p.z);
    }

    public override bool ordersComplete()
    {
        return !pathfinder.HasDestination();
    }
}