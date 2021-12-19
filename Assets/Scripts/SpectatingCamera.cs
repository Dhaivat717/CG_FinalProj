using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpectatingCamera : CameraAbstract {

    private Vector3 lpos;

    protected override Vector3 getHorizontalRotationAxis() {
        return Vector3.up;
    }
    protected override Vector3 getVerticalRotationAxis() {
        return ctgt.right;
    }
    protected override void Update() {
        base.Update();
        updateCameraTarget();
    }

    protected override void Awake() {
        base.Awake();
        lpos = obj.position;
    }
    private void updateCameraTarget() {

        Vector3 trans = obj.position - lpos;
        ctgt.position += trans;
        lpos = obj.position;

        Vector3 nfor = Vector3.ProjectOnPlane(obj.position - ctgt.position, Vector3.up);
        if (nfor != Vector3.zero)
            ctgt.rotation = Quaternion.LookRotation(obj.position - ctgt.position, Vector3.up);
    }

    
}
