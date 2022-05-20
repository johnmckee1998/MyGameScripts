using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleCarController : MonoBehaviour
{ //NOTE: is grounded is a function for wheel colliders - could be used to detect flips

    public bool playerDriving;

    [Space]
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    private float currentMaxTorque;
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float brakeForce = 100f;
    public float handBrakeForce = 200f; //unused atm
    public float DampeningRate = 100f;

    [Tooltip("Stiffness modifier for each wheel when difting")]
    public float driftTractionSideways = 0.5f;
    public float driftTractionForwards = 0.5f;

    [Tooltip("Max Speed in KMPH")]
    public float MaxSpeed = 200;
    private float curMaxSpeed;

    public Transform CenterOfMass;

    private Rigidbody rb;
    private TextMeshPro speedo;

    private float CurrentSpeed;

    private AudioSource carNoise;

    [Header("Lights")]
    public GameObject brakeLights;
    public GameObject reverseLights;
    public GameObject headLights;


    private bool drift;

    public GearStats[] gears;
    private int maxGear=0;
    private int curGear=0;

    [Header("Fuckery")]
    [Tooltip("0 means the value is ignored, all other values are used")]
    public float suspensionSpringStrength = 0;
    JointSpring susSpring;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //rb.centerOfMass -= new Vector3(0, rb.centerOfMass.y/2f,0);
        if(CenterOfMass!=null)
            rb.centerOfMass = CenterOfMass.localPosition;

        if(GetComponentInChildren<TextMeshPro>())
            speedo = GetComponentInChildren<TextMeshPro>();


        Time.fixedDeltaTime = 0.01f; //default is 0.02

        
        susSpring.damper = suspensionSpringStrength;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.leftWheel.ConfigureVehicleSubsteps(5,12,15);
            axleInfo.rightWheel.ConfigureVehicleSubsteps(5,12,15);

            axleInfo.leftWheelStiffSide = axleInfo.leftWheel.sidewaysFriction.stiffness;
            axleInfo.rightWheelStiffSide = axleInfo.rightWheel.sidewaysFriction.stiffness;

            axleInfo.leftWheelStiffForward = axleInfo.leftWheel.forwardFriction.stiffness;
            axleInfo.rightWheelStiffForward = axleInfo.rightWheel.forwardFriction.stiffness;

            if (suspensionSpringStrength != 0)
            {
                susSpring.spring = axleInfo.leftWheel.suspensionSpring.spring; // only change the spring
                susSpring.targetPosition = axleInfo.leftWheel.suspensionSpring.targetPosition;
                axleInfo.leftWheel.suspensionSpring = susSpring;
                susSpring.spring = axleInfo.rightWheel.suspensionSpring.spring;
                susSpring.targetPosition = axleInfo.rightWheel.suspensionSpring.targetPosition;
                axleInfo.rightWheel.suspensionSpring = susSpring;
            }
            
            Debug.Log("Spring " + axleInfo.leftWheel.suspensionSpring.spring + " Dampen " + axleInfo.leftWheel.suspensionSpring.damper + " Target Pos " + axleInfo.leftWheel.suspensionSpring.targetPosition);
        }

        carNoise = GetComponent<AudioSource>();

        drift = false;

        //maxGear = gears.Length;
        //currentMaxTorque = gears[0].gearTorque;
        //curMaxSpeed = gears[0].maxGearSpeed;
        currentMaxTorque = maxMotorTorque;
        curMaxSpeed = MaxSpeed;
    }



    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }


    private void Update()
    {

        if (playerDriving)
        {
            if (Input.GetKey("z") || Input.GetKey(KeyCode.Space)) //apply loss of traction when handbrake is used
            {
                drift = true;
            }
            else
                drift = false;

            if (Input.GetKeyDown("x"))
            {
                rb.AddForce(Vector3.up * 1000000);
            }

            if (Input.GetKeyDown("h"))
            {
                if(headLights!=null)
                    headLights.SetActive(!headLights.activeSelf);
            }

            if (Input.GetKeyDown("l"))
            {
                ResetPlayer();
                Debug.Log("Resetting");
            }
        }

        //Gearing Code
        /*if (Input.GetKeyDown("i") && curGear+1 < maxGear)//gearup
        {
            curGear++;
            curMaxSpeed = gears[curGear].maxGearSpeed;
            currentMaxTorque = gears[curGear].gearTorque;
            
        }
        else if (Input.GetKeyDown("j") && curGear-1>=0)//gearup
        {
            curGear--;
            curMaxSpeed = gears[curGear].maxGearSpeed;
            currentMaxTorque = gears[curGear].gearTorque;

        }*/
    }

    public void FixedUpdate()
    {
        //CurrentSpeed = (Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.y) + Mathf.Abs(rb.velocity.z)) * 3.6f;
        CurrentSpeed = (transform.InverseTransformDirection(rb.velocity).z) * 3.6f; //more accurate than previous as this only considers forward velocity, not sideways as well

        //Debug.Log("CurSpeed: " + CurrentSpeed + " LocalVel: " + (transform.InverseTransformDirection(rb.velocity).z) + " M/s: " + (CurrentSpeed/3.6f));

        float noisePitch = Mathf.Clamp01(Mathf.Abs(CurrentSpeed)/curMaxSpeed); //change pitch based off of speed
        noisePitch = Mathf.Clamp(noisePitch, 0.1f, 1f); //make sure the pitch doesnt reach 0, so use 0.2 as the idle amount

        carNoise.pitch = noisePitch; //apply pitch

        float motor = currentMaxTorque;
        if (playerDriving)
            motor *= Input.GetAxis("Vertical");
        else
            motor = 0;

        if (motor == 0)
            motor = 0.001f;

        float steering = maxSteeringAngle;
        if (playerDriving)
            steering *= Input.GetAxis("Horizontal");
        else
            steering = 0;

        float brake = 0;
        if (playerDriving)
        {
            if ((Input.GetAxis("Vertical") < 0 && CurrentSpeed > 0) || (Input.GetAxis("Vertical") > 0 && CurrentSpeed < 0)) //if s is pused while moving forward OR w is pushed when moving backwards, apply some braking
                brake = brakeForce;
        }
        else
            brake = brakeForce * 2f;

        if (Input.GetKey(KeyCode.Space))
            brake = handBrakeForce;

        if(brakeLights!=null)
            brakeLights.SetActive(brake == brakeForce);

        if (reverseLights != null && playerDriving)
        {
            if (Input.GetAxis("Vertical") < 0 && CurrentSpeed < 0)
                reverseLights.SetActive(true);
            else
                reverseLights.SetActive(false);
        }

        if (brake > 0)//dont accelerate when braking
            motor = 0.01f;
        UpdateWheels(motor, steering, brake);

        /*
        foreach (AxleInfo axleInfo in axleInfos) //Apply seetings to wheels - motor acceleration, steering, braking etc.
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                if (CurrentSpeed >= MaxSpeed) //Limit torque when speed reaches max
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.rightWheel.motorTorque = 0;
                }
                else
                {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                }
            }
            axleInfo.leftWheel.brakeTorque = brake;
            axleInfo.rightWheel.brakeTorque = brake;

            if (motor == 0) //no acceleration, increase dampening rate to simulate engine braking
            {
                axleInfo.leftWheel.wheelDampingRate = DampeningRate;
                axleInfo.rightWheel.wheelDampingRate = DampeningRate;
            }
            else
            {
                axleInfo.leftWheel.wheelDampingRate = 0.25f;
                axleInfo.rightWheel.wheelDampingRate = 0.25f;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel); //update local pos of visual wheels
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        */


        if(speedo !=null)
            speedo.text = ((int)CurrentSpeed).ToString() + " KM/H " + (curGear+1);
    }

    private void OnDrawGizmos()
    {
        if (CenterOfMass != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(CenterOfMass.position, 0.5f);
        }
    }

    private void UpdateWheels(float motor, float steering, float brake)
    {
        foreach (AxleInfo axleInfo in axleInfos) //Apply seetings to wheels - motor acceleration, steering, braking etc.
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                if (CurrentSpeed >= curMaxSpeed) //Limit torque when speed reaches max
                {
                    axleInfo.leftWheel.motorTorque = 0.01f;
                    axleInfo.rightWheel.motorTorque = 0.01f;
                }
                else
                {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                }
            }
            if (brake != handBrakeForce || axleInfo.useHandbrake)
            {
                axleInfo.leftWheel.brakeTorque = brake;
                axleInfo.rightWheel.brakeTorque = brake;
            }
            


            if (drift && !axleInfo.ignoreDrift) //Reduce traction when drifitng
            {
                WheelFrictionCurve lWFF = axleInfo.leftWheel.forwardFriction; //left wheel forward friction 
                lWFF.stiffness = driftTractionForwards * axleInfo.leftWheelStiffForward;
                WheelFrictionCurve lWSF = axleInfo.leftWheel.sidewaysFriction; //left wheel sideways friction 
                lWSF.stiffness = driftTractionSideways * axleInfo.leftWheelStiffSide;
                WheelFrictionCurve RWFF = axleInfo.rightWheel.forwardFriction; //right wheel forward friction 
                RWFF.stiffness = driftTractionForwards * axleInfo.rightWheelStiffForward;
                WheelFrictionCurve RWSF = axleInfo.rightWheel.sidewaysFriction; //right wheel sideways friction 
                RWSF.stiffness = driftTractionSideways * axleInfo.rightWheelStiffSide;

                axleInfo.leftWheel.forwardFriction = lWFF;
                axleInfo.leftWheel.sidewaysFriction = lWSF;
                axleInfo.rightWheel.forwardFriction = RWFF;
                axleInfo.rightWheel.sidewaysFriction = RWSF;
            }
            else //reset stiffness when not drifting
            {
                WheelFrictionCurve lWFF = axleInfo.leftWheel.forwardFriction;
                lWFF.stiffness = axleInfo.leftWheelStiffForward;
                WheelFrictionCurve lWSF = axleInfo.leftWheel.sidewaysFriction;
                lWSF.stiffness = axleInfo.leftWheelStiffSide;
                WheelFrictionCurve RWFF = axleInfo.rightWheel.forwardFriction;
                RWFF.stiffness = axleInfo.rightWheelStiffForward;
                WheelFrictionCurve RWSF = axleInfo.rightWheel.sidewaysFriction;
                RWSF.stiffness = axleInfo.rightWheelStiffSide;

                axleInfo.leftWheel.forwardFriction = lWFF;
                axleInfo.leftWheel.sidewaysFriction = lWSF;
                axleInfo.rightWheel.forwardFriction = RWFF;
                axleInfo.rightWheel.sidewaysFriction = RWSF;
            }

            if (motor == 0 || CurrentSpeed >= curMaxSpeed + 5)//no acceleration, increase dampening rate to simulate engine braking 
            {                                                  //if the current speed is significantly higher, apply engine braking force (like if shifting down from higher gear) 
                axleInfo.leftWheel.wheelDampingRate = DampeningRate;
                axleInfo.rightWheel.wheelDampingRate = DampeningRate;
            }
            else
            {
                axleInfo.leftWheel.wheelDampingRate = 0.25f;
                axleInfo.rightWheel.wheelDampingRate = 0.25f;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel); //update local pos of visual wheels
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    private void ResetPlayer()
    {
        transform.position += Vector3.up * 3f;
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }

    public void PlayerDriving(bool b)
    {
        playerDriving = b;
    }

}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
    public bool useHandbrake; //not currently used, but can be used to only apply handbrake force to certain wheels
    [Tooltip("THESE STIFFNESS ONES ARE USED TO STORE STUFF IN SCRIPT< DONT WORRY")]
    [HideInInspector]
    public float leftWheelStiffSide; //Stored to reset after drift
    [HideInInspector]
    public float leftWheelStiffForward;
    [HideInInspector]
    public float rightWheelStiffSide; //stored so i can reset after drifting
    [HideInInspector]
    public float rightWheelStiffForward; //stored so i can reset after drifting

    public bool ignoreDrift = false;
}

[System.Serializable]
public class GearStats
{
    public float gearTorque;
    public float maxGearSpeed;
}
