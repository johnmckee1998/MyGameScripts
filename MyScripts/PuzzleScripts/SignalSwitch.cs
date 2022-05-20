using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalSwitch : MonoBehaviour
{
    


    public bool currentSignal;


    public LogicGate connectedGate; 
    public LogicGate.GateInput connectedInput;

    public LogicGate[] secondaryGates;
    public LogicGate.GateInput[] secondaryConnectedInputs;

    [Space]
    private Material startMat;
    public Material lookAtMaterial;
    private Renderer ren;

    [Header("Signal Light Stuff")]
    public Renderer lightRen;
    public Material lightOffMat;
    public Material lightOnMat;

    private bool lookingAt;
    private bool prevLooking; //what was lookingAt in the previous update
    [Space]
    public float interactDist = 3f;
    public LayerMask rayCastIgnore;
    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<Renderer>();
        startMat = ren.material;

        if (currentSignal)
            lightRen.material = lightOnMat;
        else
            lightRen.material = lightOffMat;
    }

    private void Update()
    {
        if (CharacterControllerScript.instance != null)
        {
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDist)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out hit, interactDist, ~rayCastIgnore))
                {
                    if (hit.collider.gameObject.Equals(gameObject))
                        lookingAt = true;
                    else
                        lookingAt = false;
                }
                else
                    lookingAt = false;

            }
            else lookingAt = false;
        }

        if (lookingAt && Time.timeScale >0)
        {
            if (Input.GetButtonDown("Interact"))
            {
                currentSignal = !currentSignal;

                if (currentSignal)
                    lightRen.material = lightOnMat;
                else
                    lightRen.material = lightOffMat;
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (connectedInput == LogicGate.GateInput.input1)
            connectedGate.input1 = currentSignal;
        else if (connectedInput == LogicGate.GateInput.input2)
            connectedGate.input2 = currentSignal;

        if (secondaryGates.Length > 0)
        {
            for(int i=0; i< secondaryGates.Length; i++)
            {
                if (secondaryConnectedInputs[i] == LogicGate.GateInput.input1)
                    secondaryGates[i].input1 = currentSignal;
                else
                    secondaryGates[i].input2 = currentSignal;
            }
                
        }

        //only update material when lookingat changes, otherwise that is alot of material changes 
        if (prevLooking != lookingAt)
        {
            if(lookingAt)
                ren.material = lookAtMaterial;
            else
                ren.material = startMat;
        }

        prevLooking = lookingAt;
    }

    /*
    private void OnMouseOver()
    {
        if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDist)
        {
            lookingAt = true;
            ren.material = lookAtMaterial;
        }
    }

    private void OnMouseExit()
    {
        lookingAt = false;
        ren.material = startMat;
    }*/

    public void ToggleSwitch()
    {
        currentSignal = !currentSignal;

        if (currentSignal)
            lightRen.material = lightOnMat;
        else
            lightRen.material = lightOffMat;
    }
}
