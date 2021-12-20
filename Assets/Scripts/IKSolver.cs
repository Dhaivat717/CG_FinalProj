
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TargetInfo {
    public Vector3 position;
    public Vector3 normal;
    public bool grounded;

    public TargetInfo(Vector3 m_pos, Vector3 m_nor, bool m_gro = true) {
        position = m_pos;
        normal = m_nor;
        grounded = m_gro;
    }
}

public class IKSolver : MonoBehaviour {

    private static int maxIterations = 10;
    private static float weight = 1.0f;
    private static float footAngleToNormal = 20.0f; 
    public static void solveChainCCD(ref JointHinge[] joints, Transform endEffector, TargetInfo target, float tolerance, float minimumChangePerIteration = 0, float singularityRadius=0 ,bool hasFoot = false, bool printDebugLogs = false) {

        int iteration = 0;
        float error = Vector3.Distance(target.position, endEffector.position);
        float oldError;
        float errorDelta;

        
        while (iteration < maxIterations && error > tolerance) {

            for (int i = 0; i < joints.Length; i++) {
               
                int k = mod((i - 1), joints.Length);
                solveJointCCD(ref joints[k], ref endEffector, ref target, singularityRadius, hasFoot && k == joints.Length - 1);
            }
            iteration++;

            oldError = error;
            error = Vector3.Distance(target.position, endEffector.position);
            errorDelta = Mathf.Abs(oldError - error);
            if (errorDelta < minimumChangePerIteration) {
                if (printDebugLogs) Debug.Log("Only moved " + errorDelta + ". Therefore i give up solving.");
                break;
            }
        }

        if (printDebugLogs) {
            if (iteration == maxIterations) Debug.Log(endEffector.gameObject.name + " could not solve with " + iteration + " iterations. The error is " + error);
            if (iteration != maxIterations && iteration > 0) Debug.Log(endEffector.gameObject.name + " completed CCD with " + iteration + " iterations and an error of " + error);
        }
    }

   
    private static void solveJointCCD(ref JointHinge joint, ref Transform endEffector, ref TargetInfo target, float singularityRadius, bool adjustToTargetNormal) {
        Vector3 rotPoint = joint.getRotationPoint();
        Vector3 rotAxis = joint.getRotationAxis();
        Vector3 toEnd = Vector3.ProjectOnPlane((endEffector.position - rotPoint), rotAxis);
        Vector3 toTarget = Vector3.ProjectOnPlane(target.position - rotPoint, rotAxis);

        
        if (toTarget == Vector3.zero || toEnd == Vector3.zero) return;
        if (toTarget.magnitude < singularityRadius) return; 
        float angle;

        
        if (adjustToTargetNormal) {
            angle = footAngleToNormal + 90.0f - Vector3.SignedAngle(Vector3.ProjectOnPlane(target.normal, rotAxis), toEnd, rotAxis);
        }
        else {
            angle = weight * joint.getWeight() * Vector3.SignedAngle(toEnd, toTarget, rotAxis);
        }
        joint.applyRotation(angle);
    }

    
    public static IEnumerator solveChainCCDFrameByFrame(JointHinge[] joints, Transform endEffector, TargetInfo target, float tolerance, float minimumChangePerIteration = 0, float singularityRadius=0, bool hasFoot = false, bool printDebugLogs = false) {

        int iteration = 0;
        float error = Vector3.Distance(target.position, endEffector.position);
        float oldError;
        float errorDelta;

        if (printDebugLogs) Debug.Log(endEffector.gameObject.name + " is starting the CCD solving process.");
        Debug.Break();
        yield return null;

        while (iteration < maxIterations && error > tolerance) {

            if (printDebugLogs) Debug.Log("Starting iteration " + iteration + " with an error of " + error);
            Debug.Break();
            yield return null;

            for (int i = 0; i < joints.Length; i++) {
                int k = mod((i - 1), joints.Length);

                
                Vector3 rotPoint = joints[k].getRotationPoint();
                Vector3 rotAxis = joints[k].getRotationAxis();
                Vector3 toEnd = Vector3.ProjectOnPlane((endEffector.position - rotPoint), rotAxis);
                Vector3 toTarget = Vector3.ProjectOnPlane(target.position - rotPoint, rotAxis);
                DebugShapes.DrawPlane(rotPoint, rotAxis, toTarget, 1.0f, Color.yellow);
                Debug.DrawLine(rotPoint, rotPoint + toTarget, Color.blue);
                Debug.DrawLine(rotPoint, rotPoint + toEnd, Color.red);
                

                if (printDebugLogs) Debug.Log("Iteration " + iteration + ", joint " + joints[k].gameObject.name + " gonna happen now.");
                Debug.Break();
                yield return null;

                solveJointCCD(ref joints[k], ref endEffector, ref target, singularityRadius, hasFoot && k == joints.Length - 1);

                
                toEnd = Vector3.ProjectOnPlane((endEffector.position - rotPoint), rotAxis);
                DebugShapes.DrawPlane(rotPoint, rotAxis, toTarget, 1.0f, Color.yellow);
                Debug.DrawLine(rotPoint, rotPoint + toTarget, Color.blue);
                Debug.DrawLine(rotPoint, rotPoint + toEnd, Color.red);
                

                if (printDebugLogs) Debug.Log("Iteration " + iteration + ", joint " + joints[k].gameObject.name + " done.");
                Debug.Break();
                yield return null;
            }
            iteration++;

            oldError = error;
            error = Vector3.Distance(target.position, endEffector.position);
            errorDelta = Mathf.Abs(oldError - error);
            if (errorDelta < minimumChangePerIteration) {
                if (printDebugLogs) Debug.Log("Only moved " + errorDelta + ". Therefore i give up solving");
                Debug.Break();
                break;
            }
        }

        if (printDebugLogs) {
            if (error > tolerance) Debug.Log(endEffector.gameObject.name + " could not solve with " + iteration + " iterations. The error is " + error);
            else Debug.Log(endEffector.gameObject.name + " completed solving with " + iteration + " iterations and an error of " + error);
        }
        Debug.Break();
        yield return null;

    }

    

    
    


    
    private static void multiply(float[,] A, float[] B, ref float[] result) {
        if (A.GetLength(1) != B.Length || result.Length != A.GetLength(0)) {
            Debug.Log("Can't multiply these matrices.");
            return;
        }

        for (int row = 0; row < result.GetLength(0); row++) {
            float sum = 0;

            for (int k = 0; k < A.GetLength(1); k++) {
                sum += A[row, k] * B[k];
            }
            result[row] = sum;
        }
    }

    
    private static void multiply(ref float[] A, float a) {
        for (int k = 0; k < A.Length; k++)
        {
            A[k] *= a;
        }
    }

    private static void Transpose(float[,] A, ref float[,] result) {

        if (A.GetLength(1) != result.GetLength(0) || A.GetLength(0) != result.GetLength(1)) {
            Debug.Log("Transpose matrix not the right dimensions.");
            return;
        }

        for (int col = 0; col < A.GetLength(0); col++) {
            for (int row = 0; row < A.GetLength(1); row++) {
                result[row, col] = A[col, row];
            }
        }
    }



    private static void multiply(float[,] A, float[,] B, ref float[,] result) {
        if (A.GetLength(1) != B.GetLength(0) || result.GetLength(0) != A.GetLength(0) || result.GetLength(1) != B.GetLength(1)) {
            Debug.Log("Can't multiply these matrices.");
            return;
        }

        for (int row = 0; row < result.GetLength(0); row++) {
            for (int col = 0; col < result.GetLength(1); col++) {
                float sum = 0;

                for (int k = 0; k < A.GetLength(1); k++) {
                    sum += A[row, k] * B[k, col];
                }
                result[row, col] = sum;
            }
        }

    }


    private static int mod(int n, int m) {
        return ((n % m) + m) % m;
    }

    public static void solveJacobianTranspose(ref JointHinge[] joints, Transform endEffector, TargetInfo target, float tolerance, bool hasFoot = false) {
        Vector3 error = target.position - endEffector.position;
        float[] err = new float[] { error.x, error.y, error.z };
        float[,] J = new float[3, joints.Length];
        float[,] JT = new float[joints.Length, 3];
        float[,] JJT = new float[3, 3];
        float[] JJTe = new float[3];
        float[] angleChange = new float[joints.Length];
        float alpha;

        int iteration = 0;
        while (iteration < maxIterations || error.magnitude < tolerance) {

           
            for (int k = 0; k < joints.Length; k++) {
                Vector3 rotAxis = joints[k].getRotationAxis();
                Vector3 cross = Vector3.Cross(rotAxis, endEffector.position - joints[k].getRotationPoint());
                J[0, k] = cross.x;
                J[1, k] = cross.y;
                J[2, k] = cross.z;
            }

            
            string jacobianString = "";
            for (int i = 0; i < 3; i++) {
                for (int k = 0; k < joints.Length; k++) {
                    jacobianString += J[i, k] + " ";
                }
                jacobianString += "\n";
            }
            Debug.Log(jacobianString);



            Transpose(J, ref JT);


            multiply(J, JT, ref JJT);

        
            multiply(JJT, err, ref JJTe);
            Vector3 m_JJTe = new Vector3(JJTe[0], JJTe[1], JJTe[2]);

            
            alpha = Vector3.Dot(error, m_JJTe) / Vector3.Dot(m_JJTe, m_JJTe);

            
            multiply(JT, err, ref angleChange);
            multiply(ref angleChange, alpha);

            
            for (int k = 0; k < joints.Length; k++) {
                joints[k].applyRotation(angleChange[k]);
            }

            error = target.position - endEffector.position; 
            iteration++;
        }
    }





}