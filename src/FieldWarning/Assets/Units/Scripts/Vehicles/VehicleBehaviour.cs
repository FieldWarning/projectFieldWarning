using UnityEngine;

public class VehicleBehaviour : UnitBehaviour {

	const float DecelFactor = 2.0f;

    bool ordersDone;
	float speed;

    // Use this for initialization
    new void Start() {
        data = UnitData.Tank();
        base.Start();
	}
	
	// Update is called once per frame
	new void Update() {
        base.Update();
	}


    protected override void doMovement() {
        ordersDone = false;
        float destinationHeading;
		Vector3 waypoint = pathfinder.GetWaypoint();
		if (pathfinder.HasDestination()) {
			var diff = waypoint - this.transform.position;
            destinationHeading = diff.getRadianAngle();
        } else {
            destinationHeading = finalHeading;
        }

        destinationHeading = unwrap(destinationHeading);
        var currentHeading = Mathf.Deg2Rad * transform.localEulerAngles.y;
        var diffheading = unwrap(destinationHeading + currentHeading - Mathf.PI / 2);
        var turn = Mathf.Sign(diffheading) * data.rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(turn) > Mathf.Abs(diffheading)) turn = diffheading;
        transform.Rotate(Vector3.up, -turn);

		float targetSpeed;
		if (!pathfinder.HasDestination()) {
			targetSpeed = 0f;
            if (Mathf.Abs(turn) < 0.01f) {
                ordersDone = true;
            }
		} else {
			float destDist = (destination - transform.localPosition).magnitude;
			targetSpeed = Mathf.Min (data.movementSpeed, Mathf.Sqrt (2 * destDist * data.accelRate * DecelFactor));

			float waypointDist = (waypoint - transform.localPosition).magnitude;
			var turnradius = waypointDist / (1000 * Mathf.Abs(diffheading));
			float turnFactor = data.rotationSpeed * turnradius;
            if (turnFactor < 1)
                targetSpeed *= turnFactor;
        }

		if (targetSpeed > speed) {
			speed = Mathf.Min (targetSpeed, speed + data.accelRate * Time.deltaTime);
		} else {
			speed = Mathf.Max (targetSpeed, speed - DecelFactor * data.accelRate * Time.deltaTime);
		}
		transform.Translate(speed * Time.deltaTime * Vector3.forward);
    }

    protected override Renderer[] getRenderers() {
		// Child 0 is the collider
        return transform.GetChild(1).GetComponentsInChildren<Renderer>();
    }

    public override void setOriginalOrientation(Vector3 pos, Quaternion rotation, bool wake = true) {
        if (wake)
            WakeUp();
        transform.position = pos;
        transform.localRotation = rotation;
    }

    public override void updateMapOrientation() {
        var p = this.transform.position;
        var y = Ground.terrainData.GetInterpolatedHeight(p.x, p.z);
        this.transform.position = new Vector3(p.x, y, p.z);
    }

    public override bool ordersComplete() {
		return !pathfinder.HasDestination();
    }
}