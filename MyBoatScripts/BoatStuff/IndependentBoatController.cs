using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndependentBoatController : MonoBehaviour
{
    Rigidbody rb;
    //public float speed;
    public float turnSpeed;
    //   private float turnAmount;

    public Transform EnginePivot;
    private Vector3 objCenter;

    private float enginePower;
    public float maxPower = 50f;
    public float accelPower = 5f;
    private float currentSpeed;
    public float maxSpeed = 20f;

    public Floater engineFloater;

    public TextMeshPro speedo;
    
    public Transform centerOfMass;

    private Vector3 rotationChange = Vector3.zero;
    private Vector3 rotRef = Vector3.zero;

    private float turning = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objCenter = transform.up;

        
        //    turnAmount = transform.eulerAngles.y;
        //rb.centerOfMass = CenterOfMass.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.position;
        //if (rb.IsSleeping())
        //{
        //    Debug.Log("RB Asleep!");
        //    rb.WakeUp();
        //}
        currentSpeed = (transform.InverseTransformDirection(rb.velocity).z) * 3.6f;
        speedo.text = ((int)currentSpeed).ToString() + " KM/PH || E: " + ((float)enginePower).ToString();
        /*if (Input.GetKey("w") && currentSpeed < maxSpeed)
        {
            if (currentSpeed < maxSpeed && enginePower < maxPower)
            {
                enginePower += 1f * accelPower;
            }
            //rb.AddForceAtPosition(EnginePivot.forward * speed, EnginePivot.position, ForceMode.Force);
        }
        else if (Input.GetKey("s") && currentSpeed > -maxSpeed)
        {
            if (currentSpeed > -maxSpeed && enginePower > -maxPower)
            {
                enginePower += -1f * accelPower;
            }
            //rb.AddForceAtPosition(EnginePivot.forward * -speed, EnginePivot.position, ForceMode.Force);
        }
        else
        {
            enginePower = 0;
        }
        */
        //apply forward force
        //if (engineFloater.underwater)
        if (currentSpeed < maxSpeed && currentSpeed > -maxSpeed)
            rb.AddForceAtPosition((EnginePivot.forward * enginePower), EnginePivot.position);

        if (turning == 0)
            rotationChange = Vector3.SmoothDamp(rotationChange, Vector3.zero, ref rotRef, 1f);

        if (rotationChange != Vector3.zero)
        {
            //Debug.Log(rotationChange + " Rot");
            Quaternion deltaRot = Quaternion.Euler(rotationChange * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRot);
        }


        //if (Input.GetKey("a"))
        //{
        //    rb.AddTorque(EnginePivot.up * -turnSpeed); //turn
        //    rb.AddTorque(EnginePivot.forward * turnSpeed / 5);//lean slightly to mimic real turning
        //}
        //if (Input.GetKey("d"))
        //{
        //    rb.AddTorque(EnginePivot.up * turnSpeed); //turn
        //    rb.AddTorque(EnginePivot.forward * -turnSpeed / 5);
        //rb.AddTorque((EnginePivot.forward * -turnSpeed / 500)*currentSpeed);//lean slightly to mimic real turning
        //}
        //Debug.Log(currentSpeed + " Speed");
    }

    public void ChangeEnginePower(float p)
    {
        if (p > 0 && enginePower < maxPower)
            enginePower += p * accelPower;
        else if(p < 0  && enginePower > -maxPower)
            enginePower += p * accelPower;
    }

    public void CutEngine()
    {
        enginePower = 0;
        Debug.Log("Cut");
    }

    public void TurnBoat(float t)
    {
        turning = t;

        if(t!=0)
            rotationChange = t * EnginePivot.up * turnSpeed * (enginePower / maxPower);


        //rb.AddTorque(EnginePivot.up * turnSpeed * t); //turn
        //rb.AddTorque(EnginePivot.forward * -t * (turnSpeed / 5));//lean slightly to mimic real turning
    }
}
