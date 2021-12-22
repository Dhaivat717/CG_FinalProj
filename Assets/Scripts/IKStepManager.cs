using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(+1)]
public class IKStepManager : MonoBehaviour
{
    public enum StepMode
    {
        AlternatingTetrapodGait, QueueWait, QueueNoWait
    }
    public enum GaitStepForcing
    {
        NoForcing, ForceIfOneLegSteps, ForceAlways
    }
    public Spider peter;
    public List<IKStepper> legQueue;
    public float superSpeed = 0.006f;
    public List<IKStepper> primaryLegs;
    public StepMode peterStyle;
    private List<IKStepper> whichLeg;
    public List<IKStepper> secondaryLegs;
    public GaitStepForcing gaitStepForcing;
    private Dictionary<int, bool> delay;
    private float stopLegs;
    public bool flag1 = true;
    private List<IKStepper> mainLegs;
    public float bigS = 0.2f;

    private void AlternatingTetrapodGait()
    {

        if (Time.time < stopLegs)
        {
            return;
        }

        if (mainLegs == primaryLegs)
        {
            mainLegs = secondaryLegs;
        }
        else
        {
            mainLegs = primaryLegs;
        }

        float stepTime = calculateAverageStepTime(mainLegs);
        stopLegs = Time.time + stepTime;


        if (gaitStepForcing == GaitStepForcing.ForceAlways)
        {
            foreach (var ikStepper in mainLegs)
            {
                ikStepper.step(stepTime);
            }
        }
        else if (gaitStepForcing == GaitStepForcing.ForceIfOneLegSteps)
        {
            bool b = false;
            foreach (var ikStepper in mainLegs)
            {
                b = b || ikStepper.stepCheck();
                if (b == true)
                {
                    break;
                }
            }
            if (b == true)
            {
                foreach (var ikStepper in mainLegs)
                {
                    ikStepper.step(stepTime);
                }
            }
        }
        else
        {
            foreach (var ikStepper in mainLegs)
            {
                if (ikStepper.stepCheck()) ikStepper.step(stepTime);
            }
        }
    }

    private float calculateAverageStepTime(List<IKStepper> legQueue)
    {
        if (flag1)
        {
            float res = 0;
            foreach (var ikStepper in legQueue)
            {
                res += calculateStepTime(ikStepper);
            }
            return res / legQueue.Count;
        }
        else return bigS;
    }

    private void LateUpdate()
    {
        if (peterStyle == StepMode.AlternatingTetrapodGait)
        {
            AlternatingTetrapodGait();
        }
        else
        {
            QueuespiderStyle();
        }
    }

    private void Awake()
    {
        whichLeg = new List<IKStepper>();
        int k = 0;
        foreach (var ikStepper in legQueue.ToArray())
        {
            if (ikStepper.allowedTargetManipulationAccess())
            {
                k++;
            }
            else
            {
                legQueue.RemoveAt(k);
            }
        }

        delay = new Dictionary<int, bool>();
        foreach (var ikStepper in legQueue)
        {
            delay.Add(ikStepper.GetInstanceID(), false);
        }

        k = 0;
        foreach (var ikStepper in primaryLegs.ToArray())
        {
            if (ikStepper.allowedTargetManipulationAccess())
            {
                k++;
            }
            else
            {
                primaryLegs.RemoveAt(k);
            }
        }
        k = 0;
        foreach (var ikStepper in secondaryLegs.ToArray())
        {
            if (ikStepper.allowedTargetManipulationAccess())
            {
                k++;
            }
            else
            {
                secondaryLegs.RemoveAt(k);
            }
        }

        mainLegs = primaryLegs;
        stopLegs = bigS;
    }
    private void QueuespiderStyle()
    {

        foreach (var ikStepper in legQueue)
        {
            if (delay[ikStepper.GetInstanceID()] == true)
            {
                continue;
            }
            if (ikStepper.stepCheck())
            {
                whichLeg.Add(ikStepper);
                delay[ikStepper.GetInstanceID()] = true;

            }
        }

        int k = 0;
        foreach (var ikStepper in whichLeg.ToArray())
        {
            if (ikStepper.allowedToStep())
            {
                ikStepper.getIKChain().unpauseSolving();
                ikStepper.step(calculateStepTime(ikStepper));
                delay[ikStepper.GetInstanceID()] = false;
                whichLeg.RemoveAt(k);
            }
            else
            {

                if (peterStyle == StepMode.QueueWait)
                {
                    break;
                }
                k++;
            }
        }

        foreach (var ikStepper in whichLeg)
        {
            ikStepper.getIKChain().pauseSolving();
        }
    }


    private float calculateStepTime(IKStepper ikStepper)
    {
        if (flag1)
        {
            float k = superSpeed * peter.getScale();
            float vMag = ikStepper.getIKChain().getEndeffectorVelocityPerSecond().magnitude;
            return (vMag == 0) ? bigS : Mathf.Clamp(k / vMag, 0, bigS);
        }
        else return bigS;
    }




}

