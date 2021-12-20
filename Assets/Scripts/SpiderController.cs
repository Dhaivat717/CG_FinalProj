using UnityEngine;
using System.Collections;
using Raycasting;

[DefaultExecutionOrder(-1)] 
public class SpiderController : MonoBehaviour {

    public Spider spider;

    [Header("Camera")]
    public SmoothCamera smoothCam;

    private Vector3 getInput() {
        Vector3 up = spider.transform.up;
        Vector3 right = spider.transform.right;
        Vector3 input = Vector3.ProjectOnPlane(smoothCam.getCameraTarget().forward, up).normalized * Input.GetAxis("Vertical") + (Vector3.ProjectOnPlane(smoothCam.getCameraTarget().right, up).normalized * Input.GetAxis("Horizontal"));
        Quaternion fromTo = Quaternion.AngleAxis(Vector3.SignedAngle(up, spider.getGroundNormal(), right), right);
        input = fromTo * input;
        float magnitude = input.magnitude;
        return (magnitude <= 1) ? input : input /= magnitude;
    }
    
    
    void FixedUpdate() {
        
        Vector3 input = getInput();

        if (Input.GetKey(KeyCode.LeftShift)) spider.run(input);
        else spider.walk(input);

        Quaternion tempCamTargetRotation = smoothCam.getCamTargetRotation();
        Vector3 tempCamTargetPosition = smoothCam.getCamTargetPosition();
        spider.turn(input);
        smoothCam.setTargetRotation(tempCamTargetRotation);
        smoothCam.setTargetPosition(tempCamTargetPosition);
    }

    void Update() {
        
        spider.setGroundcheck(!Input.GetKey(KeyCode.Space));
    }

    
}