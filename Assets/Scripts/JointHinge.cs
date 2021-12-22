using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointHinge : MonoBehaviour {

    private Vector3 rotaxlcl;
    private Vector3 perloc;

    private Vector3 dftorlcl;
    private Vector3 orlcl;
    private Vector3 mnorlcl;
    private Vector3 mxorlcl;

    public rotationAxisMode rotMode;
    public bool ngtve = false;

    private float curagl = 0;

    [Range(1f, 10.0f)]
    public float dbgicsle = 1.0f;

    public bool deacjnt = false;
    public bool usrotlmts = true;

    public Transform root;
 
    
    [Range(-90, 90)]
    public float maxAngle = 90;

    [Range(0.0f, 1.0f)]
    public float wht = 1.0f;
    public Vector3 rotationAxisOrientation;
    public Vector3 rotpntofst = Vector3.zero;
    [Range(-180, 180)]
    public float startOrientation = 0;
    [Range(-90, 90)]
    public float minAngle = -90;
    
    public enum rotationAxisMode {
        RootX,
        RootY,
        RootZ,
        LocalX,
        LocalY,
        LocalZ
    }

     void OnDrawGizmosSelected() {
        if (!UnityEditor.Selection.Contains(transform.gameObject)) return;

        Awake();

        float scale = transform.lossyScale.y * 0.005f * dbgicsle;

        Vector3 dftortn = getDefaultOrientation();
        Vector3 rtpnt = getRotationPoint();
        Vector3 mnor = getMinOrientation();
        Vector3 orn = getOrientation();
        Vector3 rotax = getRotationAxis();
        

        Gizmos.color = Color.green;
        Gizmos.DrawLine(rtpnt, rtpnt + scale * rotax);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(rtpnt, 0.01f * scale);

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawSolidArc(rtpnt, rotax, mnor, maxAngle - minAngle, 0.2f * scale);

        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawSolidArc(rtpnt, rotax, mnor, curagl - minAngle, 0.1f * scale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rtpnt, rtpnt + 0.2f * scale * orn);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rtpnt, rtpnt + 0.2f * scale * dftortn);
    }
    

    void Update() {
        if (minAngle > maxAngle) {
            maxAngle = minAngle;
        }
    }

    public void applyRotation(float agl) {
        if (deacjnt) return;

        agl = agl % 360;

        if (agl == -180) agl = 180;

        if (agl > 180) agl -= 360;

        if (agl < -180) agl += 360;

        Vector3 rotaxs = getRotationAxis();
        Vector3 rotPoint = getRotationPoint();

        if (usrotlmts) {
            agl = Mathf.Clamp(curagl + agl, minAngle, maxAngle) - curagl;
        }

        transform.RotateAround(rotPoint, rotaxs, agl);

        curagl += agl;
    }

    public float getWeight() {
        return wht;
    }

    public int isVectorWithinScope(Vector3 v) {
        Vector3 rotationAxis = getRotationAxis();
        float agl1 = Vector3.SignedAngle(getMinOrientation(), v, rotationAxis); 
        float agl2 = Vector3.SignedAngle(v, getMaxOrientation(), rotationAxis); 

        if (agl1 >= 0 && agl2 >= 0) return 0;
        else if (agl1 < 0 && agl2 < 0) {
            float agl3 = Vector3.SignedAngle(getMidOrientation(), v, rotationAxis);
            if (agl3 > 0) return +1;
            else return -1;
        }
        else if (agl1 < 0) return -1;
        else return +1;
    }

    private Vector3 getMaxOrientation() {
        return transform.TransformDirection(mxorlcl);
    }
    public Vector3 getMidOrientation() {
        return transform.TransformDirection(Quaternion.AngleAxis(0.5f * (maxAngle - minAngle), rotaxlcl) * mnorlcl);
    }

    private Vector3 getMinOrientation() {
        return transform.TransformDirection(mnorlcl);
    }

    public float getAngleRange() {
        return maxAngle - minAngle;
    }
    private Vector3 getDefaultOrientation() {
        return transform.TransformDirection(dftorlcl);
    }

    
    public Vector3 getRotationAxis() {
        return transform.TransformDirection(rotaxlcl);
    }

    public Vector3 getPerpendicular() {
        return transform.TransformDirection(perloc);
    }

    public Vector3 getRotationPoint() {
        return transform.TransformPoint(0.01f * rotpntofst);
    }

    private Vector3 getOrientation() {
        return transform.TransformDirection(orlcl);
    }

    

    private void Awake() {
        setupValues();
    }

    private void setupValues() {
        Vector3 m = Vector3.zero;
        Vector3 n = Vector3.zero; 

        if (rotMode == rotationAxisMode.RootX) {
            m = root.right;
            n = root.forward;
        }
        else if (rotMode == rotationAxisMode.RootY) {
            m = root.up;
            n = root.right;
        }
        else if (rotMode == rotationAxisMode.RootZ) {
            m = root.forward;
            n = root.up;
        }
        else if (rotMode == rotationAxisMode.LocalX) {
            m = transform.right;
            n = transform.forward;
        }
        else if (rotMode == rotationAxisMode.LocalY) {
            m = transform.up;
            n = transform.right;
        }
        else if (rotMode == rotationAxisMode.LocalZ) {
            m = transform.forward;
            n = transform.right;
        }
        if (ngtve) {
            m = -m;
            n = -n;
        }

        rotaxlcl = Quaternion.Euler(rotationAxisOrientation) * transform.InverseTransformDirection(m);
        perloc = Quaternion.Euler(rotationAxisOrientation) * transform.InverseTransformDirection(n);

        if ((rotMode == rotationAxisMode.LocalX) || (rotMode == rotationAxisMode.LocalY) || (rotMode == rotationAxisMode.LocalZ)) {
            orlcl = Quaternion.AngleAxis(startOrientation, rotaxlcl) * perloc;
            dftorlcl = Quaternion.AngleAxis(-curagl, rotaxlcl) * orlcl;
            mnorlcl = Quaternion.AngleAxis(minAngle - curagl, rotaxlcl) * orlcl;
            mxorlcl = Quaternion.AngleAxis(maxAngle - curagl, rotaxlcl) * orlcl;
        }
        else {
            dftorlcl = Quaternion.AngleAxis(startOrientation, rotaxlcl) * perloc;
            orlcl = Quaternion.AngleAxis(curagl, rotaxlcl) * dftorlcl;
            mnorlcl = Quaternion.AngleAxis(minAngle, rotaxlcl) * dftorlcl;
            mxorlcl = Quaternion.AngleAxis(maxAngle, rotaxlcl) * dftorlcl;
        }
    }
   
}
