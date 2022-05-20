using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LogicGate : MonoBehaviour
{
    public enum Gate {and, or, xor, not, nand, nor, xnor}
    /* Types of Gate:
     * and -> fairly straight forward, returns true if both inputs are true, otherwise false
     * or -> returns true if either input is true
     * xor -> returns true when only one input is true (so like or but returns false when and is true)
     * not -> only 1 input, simply flips it
     * nand -> returns true when at least one input is FALSE (so true in all cases except true+true) 
     * nor -> returns true only when both inputs are false, so if any input is true then it returns false
     * xnor -> basically acts like an equals gate - returns true if false+false or true+true is given
     * 
     * 
     * Note: there is actualy bool operations of | and &, not just || and &&. the diference is the singular version are logcial ones - 
     * both inputs are alwasy check, where the double ones are conditional, where in && the second is only check if the first is true, 
     * and in || the second is only checked if the first is false. 
     * for my purposes it doesnt matter which one, so may as well use the double as it seems more efficient.
     */
    public enum GateInput { input1, input2 };

    public Gate gateType;
    [HideInInspector]
    public bool gateActive; //set to true when gate is activated - depends on exact logic of the gate

    public bool input1;
    public bool input2; //gates like 'not' only need the first input so this will be ignored

    [Space]
    [Header("Parent Gate")]
    public LogicGate connectedGate;
    public GateInput connectedInput;
    [Space]
    public LogicGate[] secondaryGates;
    public GateInput[] secondaryConnectedInputs;

    [Header("Signal Light Stuff")]
    public Renderer lightRen;
    public Material lightOffMat;
    public Material lightOnMat;

    [Space]
    [Header("Activate Event")]
    public UnityEvent gateActiveEvent;

    private bool eventActivated; //makes sure activeevent only invokes once
    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateGate();

        if (lightRen != null)
        {
            if (gateActive) //not the best way of doing it - changes material every update, should only do it on change of value
                lightRen.material = lightOnMat;
            else
                lightRen.material = lightOffMat;
        }

        if (connectedGate != null)
        {
            if (connectedInput == GateInput.input1)
                connectedGate.input1 = gateActive;
            else if (connectedInput == GateInput.input2)
                connectedGate.input2 = gateActive;
        }

        if (secondaryGates.Length > 0)
        {
            for (int i = 0; i < secondaryGates.Length; i++)
            {
                if (secondaryConnectedInputs[i] == GateInput.input1)
                    secondaryGates[i].input1 = gateActive;
                else
                    secondaryGates[i].input2 = gateActive;
            }

        }

        if (!eventActivated)
        {
            if (gateActive)
            {
                if(gateActiveEvent != null)
                {
                    try
                    {
                        gateActiveEvent.Invoke();
                    }
                    catch
                    {
                        Debug.Log("Logic Gate -> Problem Running Event: " + gameObject.name);
                    }
                    eventActivated = true;
                }
            }
        }
    }

    private void UpdateGate()
    {
        if(gateType == Gate.and)
        {
            gateActive = (input1 && input2);
            return;
        }
        if (gateType == Gate.or)
        {
            gateActive = (input1 || input2);
            return;
        }
        if (gateType == Gate.xor)
        {
            gateActive = ((input1 || input2) && (input1 != input2));  //should work -> first expression will return true if either is true, but second will stop the case of both being true from returning true
            return;
        }
        if (gateType == Gate.not)
        {
            gateActive = !input1;
            return;
        }
        if (gateType == Gate.nand)
        {
            gateActive = !(input1 && input2);
            return;
        }
        if (gateType == Gate.nor)
        {
            gateActive = (!input1 && !input2);
            return;
        }
        if (gateType == Gate.xnor)
        {
            gateActive = (input1 == input2);
            return;
        }
    }
}
