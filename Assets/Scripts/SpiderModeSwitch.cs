using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderModeSwitch : MonoBehaviour {

    public SpiderController spiderController;
    public SpiderNPCController spiderNPC;
    public Camera controllerCam;
    public Camera npcCam;

    void Start() {
        
        if (controllerCam.enabled && npcCam.enabled) npcCam.enabled = false;
        if (!controllerCam.enabled && !npcCam.enabled) controllerCam.enabled = true;

        
        if (spiderController.enabled && spiderNPC.enabled) spiderNPC.enabled = false;
        if (!spiderController.enabled && !spiderNPC.enabled) spiderController.enabled = true;
    }
    void Update() {

        if (Input.GetKeyDown(KeyCode.Tab)) {
            controllerCam.enabled = !controllerCam.enabled;
            npcCam.enabled = !npcCam.enabled;
            spiderNPC.enabled = !spiderNPC.enabled;
            spiderController.enabled = !spiderController.enabled;
        }
    }
}
