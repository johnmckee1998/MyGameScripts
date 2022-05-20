using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoatControls : MonoBehaviour
{
    public GameObject player;
    public IndependentBoatController boatControl;
    private bool lookingAt = false;

    [Tooltip("true = steering, false = throttle")]
    public bool controlType = false;

    public Material looMat;
    private Material defaultMat;
    // Start is called before the first frame update
    void Start()
    {
        defaultMat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(lookingAt && Vector3.Distance(player.transform.position, transform.position)<5f &&Time.timeScale>0)
        {
            GetComponent<Renderer>().material = looMat;
            if (!controlType)//Throttle Control
            {
                if (Input.GetKeyDown("i"))
                {
                    boatControl.ChangeEnginePower(1f);
                }
                else if (Input.GetKeyDown("k"))
                {
                    boatControl.ChangeEnginePower(-1f);
                }

                if (Input.GetKeyDown("p"))
                    boatControl.CutEngine();

            }

            else //Steering Control
            {
                if (Input.GetKey("u"))
                {
                    boatControl.TurnBoat(-1f);
                }
                else if (Input.GetKey("o"))
                {
                    boatControl.TurnBoat(1f);
                }
                else
                    boatControl.TurnBoat(0);
            }
        }
        else
            GetComponent<Renderer>().material = defaultMat;
    }

    private void OnMouseEnter()
    {
        lookingAt = true;
        //GetComponent<Renderer>().material = looMat;
    }

    private void OnMouseExit()
    {
        lookingAt = false;
        //GetComponent<Renderer>().material = defaultMat;
    }
}
