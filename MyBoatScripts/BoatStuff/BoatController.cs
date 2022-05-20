using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    Rigidbody rb;
    //public float speed;
    public float turnSpeed;
    
    public Transform EnginePivot;
    private Vector3 objCenter;

    private float enginePower;
    public float maxPower = 50f;
    public float accelPower = 5f;
    private float currentSpeed;
    public float maxSpeed = 20f;

    public Floater engineFloater;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objCenter = transform.up;
        //rb.centerOfMass = CenterOfMass.position;
    }

    // Update is called once per frame
    void Update()
    {
        currentSpeed = (transform.InverseTransformDirection(rb.velocity).z) * 3.6f;
        if (Input.GetKey("w") && currentSpeed<maxSpeed)
        {
            if (currentSpeed < maxSpeed && enginePower < maxPower)
            {
                enginePower += 1f * accelPower;
            }
            //rb.AddForceAtPosition(EnginePivot.forward * speed, EnginePivot.position, ForceMode.Force);
        }
        else if (Input.GetKey("s") && currentSpeed > -maxSpeed)
        {
            if (currentSpeed < maxSpeed && enginePower < maxPower)
            {
                enginePower += -1f * accelPower;
            }
            //rb.AddForceAtPosition(EnginePivot.forward * -speed, EnginePivot.position, ForceMode.Force);
        }
        else
        {
            enginePower = 0;
        }

        //apply forward force
        if(engineFloater.underwater)
            rb.AddForceAtPosition(EnginePivot.forward * enginePower, EnginePivot.position);

        if (Input.GetKey("a"))
        {
            rb.AddTorque(EnginePivot.up * -turnSpeed ); //turn
            rb.AddTorque(EnginePivot.forward * turnSpeed / 5);//lean slightly to mimic real turning
        }
        if (Input.GetKey("d"))
        {
            rb.AddTorque(EnginePivot.up * turnSpeed ); //turn
            rb.AddTorque(EnginePivot.forward * -turnSpeed / 5);
            //rb.AddTorque((EnginePivot.forward * -turnSpeed / 500)*currentSpeed);//lean slightly to mimic real turning
        }
        //Debug.Log(currentSpeed + " Speed");
    }
}
