using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class SmoothCamera : CameraAbstract {

    
    [Range(0, 1)]
    public float rdmp;

    protected override Vector3 getHorizontalRotationAxis() {
        return obj.transform.up;
    }
    protected override Vector3 getVerticalRotationAxis() {
        return ctgt.right;
    }
    private Vector3 lobobjnor;

    protected override void Awake() {
        base.Awake();
        lobobjnor = obj.up;
        ctgt.parent = obj;
    }

    protected override void Update() {
        base.Update();
        if (rdmp != 0) {
            float ang = Vector3.SignedAngle(lobobjnor, obj.up, ctgt.right);
            RotateCameraVertical(rdmp * - ang);
            lobobjnor = obj.up;
        }
    }

    
}
