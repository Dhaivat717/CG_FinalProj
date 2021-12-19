using UnityEngine.Rendering;
using UnityEngine;
using Raycasting;
using System.Collections;
using System.Collections.Generic;


public abstract class CameraAbstract : MonoBehaviour {
    protected Transform ctgt;
    protected Camera cam;
    
    public float tspd;
    public float rspd;
    public Transform obj;
    public bool sdbg;

    private Vector3 vel = Vector3.zero;
    public PositionInterpolation posintertype;
    
    public enum PositionInterpolation { Slerp, Lerp, SmoothDamp }
   
    [Range(0.01f, 179.99f)]
    public float camlowmar = 62.0f;

    [Range(0.01f, 179.99f)]
    public float camuppmar = 31.0f;

    [Range(1, 8)]
    public float xsen;
    [Range(1, 8)]
    public float ysen;

    [Range(0, 1)]
    public float clzommindisfac;
    private float maxcamdis;

    private RaycastHit hinfo;
    private RayCast clzomrayplatocam;
    public bool enclizom;
    public LayerMask clzomlay;
    [Range(0, 1)]
    public float clzompadfac;
    

    public bool enobhid;
    public LayerMask obhidlay;
    [Range(0, 1f)]
    public float rayradobshid;

    private SphereCast hidraycamtopla;
    private RaycastHit[] cobs;
    private ShadowCastingMode[] cobscasmde;

    

    

    protected virtual void Awake() {
        cam = GetComponent<Camera>();
        setupCamTarget();
        initializeRayCasting();
        transform.parent = null; 

        if (camuppmar >= camlowmar) {
            camuppmar = 45f;
            camlowmar = 90f;
        }
    }

    protected virtual void Update() {
        RotateCameraHorizontal(Input.GetAxis("Mouse X") * xsen, false);
        RotateCameraVertical(-Input.GetAxis("Mouse Y") * ysen, false);

        if (!cam.enabled) return; 
        if (enclizom) clipZoom();
        if (enobhid) hideObstructions();
    }

    private void setupCamTarget() {
        GameObject gobj = new GameObject(gameObject.name + " Target");
        ctgt = gobj.transform;
        ctgt.position = transform.position;
        ctgt.rotation = transform.rotation;
    }

    private void initializeRayCasting() {
        maxcamdis = Vector3.Distance(obj.position, transform.position);
        clzomrayplatocam = new RayCast(obj.position, ctgt.position, obj, null);
        hidraycamtopla = new SphereCast(transform.position, obj.position, rayradobshid * transform.lossyScale.y * 0.01f, transform, obj);
    }

    protected void LateUpdate() {
        switch (posintertype) {
            case PositionInterpolation.Slerp:
                transform.position = obj.TransformPoint(Vector3.Slerp(obj.InverseTransformPoint(transform.position), obj.InverseTransformPoint(ctgt.position), Time.deltaTime * tspd));
                break;
            case PositionInterpolation.SmoothDamp:
                transform.position = Vector3.SmoothDamp(transform.position, ctgt.position, ref vel, 1 / tspd);
                break;
            case PositionInterpolation.Lerp:
                transform.position = Vector3.Lerp(transform.position, ctgt.position, Time.deltaTime * tspd);
                break;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, ctgt.rotation, Time.deltaTime * rspd);

        if (sdbg && cam.enabled) drawDebug();
    }
    public void RotateCameraHorizontal(float angle, bool onlyTarget = true) {
        Vector3 rax = getHorizontalRotationAxis();
        ctgt.RotateAround(obj.position, rax, angle);
        if (!onlyTarget) transform.RotateAround(obj.position, rax, angle);
    }

    public void RotateCameraVertical(float ang, bool onlyTarget = true) {
        clampAngle(ref ang);
        Vector3 zeroOrientation = getHorizontalRotationAxis();
        float cang = Vector3.SignedAngle(zeroOrientation, ctgt.position - obj.transform.position, ctgt.right); 

        if (cang + ang > -camuppmar) {
            ang = -cang - camuppmar;
        }
        if (cang + ang < -camlowmar) {
            ang = -cang - camlowmar;
        }

        Vector3 rax = getVerticalRotationAxis();

        ctgt.RotateAround(obj.position, rax, ang);
        if (!onlyTarget) transform.RotateAround(obj.position, rax, ang);
    }

    private void clampAngle(ref float ang) {
        ang = ang % 360;
        if (ang == -180) ang = 180;
        if (ang > 180) ang -= 360;
        if (ang < -180) ang += 360;
    }

    private void clipZoom() {
        clzomrayplatocam.setDistance(maxcamdis);
        clzomrayplatocam.setEnd(ctgt.position); 

        Vector3 dir = clzomrayplatocam.getDirection().normalized;
        if (clzomrayplatocam.castRay(out hinfo, clzomlay)) {

            Vector3 npos = hinfo.point - dir * clzompadfac * maxcamdis;
            Vector3 v = npos - obj.position;
            float mdis = maxcamdis * clzommindisfac;

            if (Vector3.Angle(v, dir) > 45f || Vector3.Distance(obj.position, npos) < mdis) {
                npos = obj.position + dir * mdis;
            }
            transform.position = npos;
            ctgt.position = npos;
        }
        else {
            ctgt.position = clzomrayplatocam.getEnd();
        }
    }

    public void setTargetPosition(Vector3 pos) {
        ctgt.position = pos;
    }
    public Transform getCameraTarget() {
        return ctgt;
    }

    public void setTargetRotation(Quaternion rot) {
        ctgt.rotation = rot;
    }

    private void hideObstructions() {

        if (cobs != null) {
            for (int k = 0; k < cobs.Length; k++) {
                MeshRenderer m = cobs[k].transform.GetComponent<MeshRenderer>();
                if (m == null) continue;
                m.shadowCastingMode = cobscasmde[k];
            }
        }
        cobs = hidraycamtopla.castRayAll(obhidlay);
        cobscasmde = new ShadowCastingMode[cobs.Length];
        for (int k = 0; k < cobs.Length; k++) {
            MeshRenderer m = cobs[k].transform.GetComponent<MeshRenderer>();
            if (m == null) continue;
            cobscasmde[k] = m.shadowCastingMode;
            m.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }

    protected abstract Vector3 getHorizontalRotationAxis();
    protected abstract Vector3 getVerticalRotationAxis();

    public Camera getCamera() {
        return cam;
    }
    public Vector3 getCamTargetPosition() {
        return ctgt.position;
    }

    public Quaternion getCamTargetRotation() {
        return ctgt.rotation;
    }

    public Transform getObservedObject() {
        return obj;
    }

    
    private void drawDebug() {
        Debug.DrawLine(transform.position, obj.position, Color.gray);

        clzomrayplatocam.draw(Color.white);
        DebugShapes.DrawRay(clzomrayplatocam.getOrigin(), clzomrayplatocam.getDirection(), clzommindisfac*maxcamdis, Color.blue);
        DebugShapes.DrawRay(clzomrayplatocam.getEnd(), -clzomrayplatocam.getDirection(), clzompadfac*maxcamdis, Color.red);

        Vector3 zeroOrientation = getHorizontalRotationAxis();
        Vector3 up = Quaternion.AngleAxis(-camuppmar, ctgt.right) * zeroOrientation;
        Vector3 down = Quaternion.AngleAxis(-camlowmar, ctgt.right) * zeroOrientation;

        if (!UnityEditor.EditorApplication.isPlaying) {
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.DrawSolidArc(obj.position, ctgt.right, down, Vector3.SignedAngle(down, up, ctgt.right), maxcamdis / 4);

        }

        DebugShapes.DrawPoint(ctgt.position, Color.magenta, 0.1f);
        DebugShapes.DrawRay(ctgt.position, ctgt.forward, Color.blue);
        DebugShapes.DrawRay(ctgt.position, ctgt.right, Color.red);
        DebugShapes.DrawRay(ctgt.position, ctgt.up, Color.green);
    }

    private void OnDrawGizmosSelected() {
        if (!sdbg) return;
        if (UnityEditor.EditorApplication.isPlaying) return;
        if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
        if (obj == null) return;
        ctgt = transform;
        initializeRayCasting();
        drawDebug();
    }
}
