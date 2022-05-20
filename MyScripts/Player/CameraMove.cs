using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public static CameraMove instance;


    private Vector3 CamRot;


    private bool allowUp = true;
    private bool allowDown = true;


    public float speedUp = 20f;
    public float speedAcross = 20f;


    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -85f;
    private float maxPitch = 85f;


    private float prevYaw;
    private float prevPitch;

    public static float yawSpeed;
    public static float pitchSpeed;

    public GameObject PlayerOBJ;

    private Transform camPar;

    [HideInInspector]
    public bool Active;
    // Start is called before the first frame update
    void Start()
    {
        Active = true;
        instance = this;
        //transform.eulerAngles =  new Vector3 (0, 0, 0);
        yaw = PlayerOBJ.transform.eulerAngles.y;

        prevYaw = yaw;
        prevPitch = pitch;

        yawSpeed = 0;
        pitchSpeed = 0;

        camPar = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            if (Time.timeScale > 0 && (GlobalStats.instance==null || !GlobalStats.instance.isPaused))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            if (Time.timeScale > 0 && (GlobalStats.instance == null || !GlobalStats.instance.isPaused))
            {
                yaw += Input.GetAxis("Mouse X") * speedAcross;
                pitch -= Input.GetAxis("Mouse Y") * speedUp;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                camPar.eulerAngles = new Vector3(pitch, camPar.rotation.eulerAngles.y, 0f); //should this be localeuler? in reality shouldnt be a problem
                PlayerOBJ.transform.eulerAngles = new Vector3(0f, yaw, 0f); //Rotate parent around x not cam, so that you walk forward

                //camPar.eulerAngles = Vector3.MoveTowards(camPar.eulerAngles ,new Vector3(pitch, camPar.rotation.eulerAngles.y, 0f), speedAcross/2 * Time);
                //PlayerOBJ.transform.eulerAngles = new Vector3(0f, yaw, 0f); //Rotate parent around x not cam, so that you walk forward

                /*if (Input.GetAxis("Mouse X") != 0)
                {
                    float x = Input.GetAxis("Mouse X");
                    transform.parent.Rotate(0f, 200.0f * x * Time.deltaTime, 0f);
                }*/ //OldStyle


            }

            CamRot = camPar.rotation.eulerAngles;
        }
    }

    private void FixedUpdate()
    {
        //Yaw and Pitch speed are calculated to create the swaying effect for the gun when the player is turning
        yawSpeed = prevYaw - yaw;
        prevYaw = yaw;

        pitchSpeed = pitch - prevPitch;
        prevPitch = pitch;

        if (CharacterControllerScript.isAiming) //Dampen the effect when aiming
        {
            pitchSpeed *= 0.5f;
            yawSpeed *= 0.5f;
        }

        else //buff the effect normally as the base speed to usually not enough
        {
            pitchSpeed *= 1.5f;
            yawSpeed *= 1.5f;
        }
    }

    public Vector3 getCamRot()
    {
        return CamRot;
    }

    public void ChangeYaw(float y)
    {
        yaw += y;
    }

    public void ChangePitch(float p)
    {
        pitch += p;
    }

    public float GetPitch()
    {
        return pitch;
    }

    public float GetYaw()
    {
        return yaw;
    }

}
