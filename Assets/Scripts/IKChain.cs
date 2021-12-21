using Raycasting;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public enum TargetMode {
    ExternallyHandled,
    DebugTarget,
    DebugTargetRay
}

public class IKChain : MonoBehaviour {

    public JointHinge[] jnts;
    public Transform eneff;

    private TargetInfo curtar;
    private float err = 0.0f;
    private bool pse = false;

    private RayCast dbgmdray;
    public bool adjlasjoitonor;

    public IKSolveMethod iksolmet;
    [Range(0.1f, 10f)]
    public float tol;

    public enum IKSolveMethod { CCD, CCDFrameByFrame };
    public bool prtdeblogs;
    public bool deacsolv = false;

    
    private Vector3 eneffvel;
    private Vector3 lsteneffpos;

    [Range(0.01f, 1f)]
    public float micngperl;
    [Range(1f, 100f)]
    public float sinrad;

   
    public TargetMode tmode;
    public LayerMask debtarraylay;
    public Transform debugTarget;

    public float getError() {
        return err;
    }
    public float getTolerance() {
        return transform.lossyScale.y * 0.00001f * tol;
    }

    public float getMinimumChangePerIterationOfSolving() {
        return transform.lossyScale.y * 0.00001f * micngperl;
    }

    public float getSingularityRadius() {
        return transform.lossyScale.y * 0.00001f * sinrad;
    }

    public Vector3 getEndeffectorVelocityPerSecond() {
        return eneffvel;
    }

    public bool isTargetExternallyHandled() {
        return tmode == TargetMode.ExternallyHandled;
    }
    public void pauseSolving() {
        pse = true;
    }
    public void unpauseSolving() {
        pse = false;
    }

    private bool hasmovoccsinlassol() {
        return (Mathf.Abs(Vector3.Distance(eneff.position, curtar.position) - err) > float.Epsilon);
    }

    void OnDrawGizmosSelected() {

        if (UnityEditor.EditorApplication.isPlaying) 
            return;
        if (!UnityEditor.Selection.Contains(transform.gameObject)) 
            return;

        Awake();

        if (debugTarget == null && (tmode == TargetMode.DebugTarget || tmode == TargetMode.DebugTargetRay)) 
            return;

        for (int k = 0; k < jnts.Length - 1; k++) {
            Debug.DrawLine(jnts[k].getRotationPoint(), jnts[k + 1].getRotationPoint(), Color.green);
        }
        Debug.DrawLine(jnts[jnts.Length - 1].getRotationPoint(), eneff.position, Color.green);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(eneff.position, getTolerance());

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(eneff.position + (getTolerance() + getMinimumChangePerIterationOfSolving()) * transform.up, getMinimumChangePerIterationOfSolving());

        Gizmos.color = Color.red;
        for (int k = 0; k < jnts.Length; k++) {
            Gizmos.DrawWireSphere(jnts[k].getRotationPoint(), getSingularityRadius());
        }
    }

    private void Awake() {
        if (tmode == TargetMode.DebugTarget) {
            dbgmdray = new RayCast(debugTarget.position + 1.0f * Vector3.up, debugTarget.position - 1.0f * Vector3.up, debugTarget, debugTarget);
            lsteneffpos = eneff.position;
        }
    }

    private void LateUpdate() {
        if (deacsolv) 
            return;

        eneffvel = (eneff.position - lsteneffpos) / Time.deltaTime;

        if (!hasmovoccsinlassol()) 
            return;
        if (!pse) {
            if (iksolmet==IKSolveMethod.CCD) {
            IKSolver.solveChainCCD(ref jnts, eneff, curtar, getTolerance(), getMinimumChangePerIterationOfSolving(), getSingularityRadius(), adjlasjoitonor, prtdeblogs);
            }
            else if (iksolmet == IKSolveMethod.CCDFrameByFrame) {
            StartCoroutine(IKSolver.solveChainCCDFrameByFrame(jnts, eneff, curtar, getTolerance(), getMinimumChangePerIterationOfSolving(), getSingularityRadius(), adjlasjoitonor, prtdeblogs));
            deacsolv = true;
            }
            err = Vector3.Distance(eneff.position, curtar.position);
        }
        lsteneffpos = eneff.position;
    }

    private void Start() {
        if (tmode == TargetMode.DebugTarget){
            curtar = getDebugTarget();
        } 
        else if (tmode == TargetMode.DebugTargetRay){
            curtar = getDebugTargetRay();
        } 
    }

    void Update() {
        if (deacsolv) 
            return;

        if (tmode == TargetMode.DebugTarget) {
            curtar = getDebugTarget();
        }
    }

    private void FixedUpdate() {
        if (deacsolv) 
            return;

        if (tmode == TargetMode.DebugTargetRay) {
            curtar = getDebugTargetRay();
        }
    }

    

    private TargetInfo getDebugTargetRay() {
        dbgmdray.draw(Color.red);
        if (dbgmdray.castRay(out RaycastHit hitInfo, debtarraylay)) {
            return new TargetInfo(hitInfo.point, hitInfo.normal);
        }
        else {
            return new TargetInfo(debugTarget.position, debugTarget.up);
        }
    }

    public void setTarget(TargetInfo target) {
        if (tmode != TargetMode.ExternallyHandled) {
            return;
        }
        curtar = target;
    }

    public JointHinge getRootJoint() {
        return jnts[0];
    }

    public Transform getEndEffector() {
        return eneff;
    }

    public TargetInfo getTarget() {
        return curtar;
    }

    public float calculateChainLength() {
        float clen = 0;
        for (int i = 0; i < jnts.Length; i++) {
            Vector3 a = jnts[i].getRotationPoint();
            Vector3 b = (i != jnts.Length - 1) ? jnts[i + 1].getRotationPoint() : eneff.position;
            clen += Vector3.Distance(a, b);
        }
        return clen;
    }

    private TargetInfo getDebugTarget() {
        return new TargetInfo(debugTarget.position, debugTarget.up);
    }
    
}
