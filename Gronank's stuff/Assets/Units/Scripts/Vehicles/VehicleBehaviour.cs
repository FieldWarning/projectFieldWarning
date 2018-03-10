using UnityEngine;

public class VehicleBehaviour : UnitBehaviour {
    bool ordersDone;


    // Use this for initialization
    void Start() {
        data = UnitData.Tank();
        base.Start();
	}
	
	// Update is called once per frame
	void Update() {
        base.Update();
	}


    protected override void doMovement() {
        ordersDone = false;
        float destinationHeading;
        if (gotDestination) {
            var diff = destination - this.transform.position;
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

        if (!gotDestination) {
            if (Mathf.Abs(turn) < 0.01f) {
                ordersDone = true;
            }
            return;
        }
        float distance = (destination - transform.localPosition).magnitude;

        if (distance < 1f) {
            gotDestination = false;
        } else {
            float offset = data.movementSpeed * Time.deltaTime;
            var turnradius = distance / (1000 * Mathf.Abs(diffheading));
            float factor = data.rotationSpeed * turnradius;

            if (factor < 1)
                offset *= factor;
            transform.Translate(offset * Vector3.forward);
        }
    }

    protected override Renderer[] getRenderers() {
        return transform.GetChild(0).GetComponentsInChildren<Renderer>();
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
        return !gotDestination;
    }
}