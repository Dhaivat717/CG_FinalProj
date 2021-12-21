using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(+1)]
public class IKStepManager : MonoBehaviour
{
    public bool flag;
    public Spider spider;

    public enum StepMode { AlternatingTetrapodGait, QueueWait, QueueNoWait }
    public StepMode spiderStyle;
    public List<IKStepper> legQueue;
    private List<IKStepper> stepQueue;
    private Dictionary<int, bool> waitingForStep;
    public List<IKStepper> gaitGroupA;
    public List<IKStepper> gaitGroupB;
    private List<IKStepper> currentGaitGroup;
    private float nextSwitchTime;
    public bool dynamicStepTime = true;
    public float stepTimePerVelocity = 0.006f;
    public float maxStepTime = 0.2f;

    public enum GaitStepForcing { NoForcing, ForceIfOneLegSteps, ForceAlways }
    public GaitStepForcing gaitStepForcing;

    private void Awake()
    {

        stepQueue = new List<IKStepper>();
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

        waitingForStep = new Dictionary<int, bool>();
        foreach (var ikStepper in legQueue)
        {
            waitingForStep.Add(ikStepper.GetInstanceID(), false);
        }

        k = 0;
        foreach (var ikStepper in gaitGroupA.ToArray())
        {
            if (!ikStepper.allowedTargetManipulationAccess()) gaitGroupA.RemoveAt(k);
            else k++;
        }
        k = 0;
        foreach (var ikStepper in gaitGroupB.ToArray())
        {
            if (!ikStepper.allowedTargetManipulationAccess()) gaitGroupB.RemoveAt(k);
            else k++;
        }

        currentGaitGroup = gaitGroupA;
        nextSwitchTime = maxStepTime;
    }

    private void LateUpdate()
    {
        if (spiderStyle == StepMode.AlternatingTetrapodGait) AlternatingTetrapodGait();
        else QueuespiderStyle();
    }

    private void QueuespiderStyle()
    {

        foreach (var ikStepper in legQueue)
        {

            // Check if Leg isnt already waiting for step.
            if (waitingForStep[ikStepper.GetInstanceID()] == true) continue;

            //Now perform check if a step is needed and if so enqueue the element
            if (ikStepper.stepCheck())
            {
                stepQueue.Add(ikStepper);
                waitingForStep[ikStepper.GetInstanceID()] = true;
                if (flag) Debug.Log(ikStepper.name + " is enqueued to step at queue position " + stepQueue.Count);
            }
        }

        if (flag) printQueue();


        int k = 0;
        foreach (var ikStepper in stepQueue.ToArray())
        {
            if (ikStepper.allowedToStep())
            {
                ikStepper.getIKChain().unpauseSolving();
                ikStepper.step(calculateStepTime(ikStepper));
                // Remove the stepping leg from the list:
                waitingForStep[ikStepper.GetInstanceID()] = false;
                stepQueue.RemoveAt(k);
                if (flag) Debug.Log(ikStepper.name + " was allowed to step and is thus removed.");
            }
            else
            {
                if (flag) Debug.Log(ikStepper.name + " is not allowed to step.");

                if (spiderStyle == StepMode.QueueWait)
                {
                    if (flag) Debug.Log("Wait selected, thus stepping ends for this frame.");
                    break;
                }
                k++;
            }
        }

        foreach (var ikStepper in stepQueue)
        {
            ikStepper.getIKChain().pauseSolving();
        }
    }

    private void AlternatingTetrapodGait()
    {

        if (Time.time < nextSwitchTime) return;


        currentGaitGroup = (currentGaitGroup == gaitGroupA) ? gaitGroupB : gaitGroupA;
        float stepTime = calculateAverageStepTime(currentGaitGroup);
        nextSwitchTime = Time.time + stepTime;

        if (flag)
        {
            string text = ((currentGaitGroup == gaitGroupA) ? "Group: A" : "Group B") + " StepTime: " + stepTime;
            Debug.Log(text);
        }

        if (gaitStepForcing == GaitStepForcing.ForceAlways)
        {
            foreach (var ikStepper in currentGaitGroup) ikStepper.step(stepTime);
        }
        else if (gaitStepForcing == GaitStepForcing.ForceIfOneLegSteps)
        {
            bool b = false;
            foreach (var ikStepper in currentGaitGroup)
            {
                b = b || ikStepper.stepCheck();
                if (b == true) break;
            }
            if (b == true) foreach (var ikStepper in currentGaitGroup) ikStepper.step(stepTime);
        }
        else
        {
            foreach (var ikStepper in currentGaitGroup)
            {
                if (ikStepper.stepCheck()) ikStepper.step(stepTime);
            }
        }
    }

    private float calculateStepTime(IKStepper ikStepper)
    {
        if (dynamicStepTime)
        {
            float k = stepTimePerVelocity * spider.getScale(); // At velocity=1, this is the steptime
            float velocityMagnitude = ikStepper.getIKChain().getEndeffectorVelocityPerSecond().magnitude;
            return (velocityMagnitude == 0) ? maxStepTime : Mathf.Clamp(k / velocityMagnitude, 0, maxStepTime);
        }
        else return maxStepTime;
    }

    private float calculateAverageStepTime(List<IKStepper> legQueue)
    {
        if (dynamicStepTime)
        {
            float stepTime = 0;
            foreach (var ikStepper in legQueue)
            {
                stepTime += calculateStepTime(ikStepper);
            }
            return stepTime / legQueue.Count;
        }
        else return maxStepTime;
    }

    private void printQueue()
    {
        if (stepQueue == null) return;
        string queueText = "[";
        if (stepQueue.Count != 0)
        {
            foreach (var ikStepper in stepQueue)
            {
                queueText += ikStepper.name + ", ";
            }
            queueText = queueText.Substring(0, queueText.Length - 2);
        }
        queueText += "]";
        Debug.Log("Queue: " + queueText);
    }
}

